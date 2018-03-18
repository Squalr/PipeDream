namespace Squalr.PipeDream
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Returns objects that implement an interface, without the need to manually create a type 
    /// that implements the interface
    /// </summary>
    public static class InterfaceObjectFactory
    {
        /// <summary>
        /// All of the types generated for each interface.  This dictionary is indexed by the 
        /// interface's type object
        /// </summary>
        private static Dictionary<Type, Type> InterfaceImplementations
            = new Dictionary<Type, Type>();

        /// <summary>
        /// Returns an object that implement an interface, without the need to manually create a 
        /// type that implements the interface
        /// </summary>
        /// <typeparam name="T">T must be an interface that is public.</typeparam>
        /// <returns>An object that implements the T interface</returns>
        /// <exception cref="TypeIsNotAnInterface">Thrown if T is not an interface</exception>
        public static T New<T>()
            where T : class
        {
            Type type = typeof(T);

            // If the type that implements the isn't created, create it
            if (!InterfaceImplementations.ContainsKey(type))
                CreateTypeFor(type);

            // Now that the type exists to implement the interface, use the Activator to create an 
            // object
            return (T)Activator.CreateInstance(InterfaceImplementations[type]);
        }

        static InterfaceObjectFactory()
        {
            // Initialize an assembly and module builder for use for all generated classes
            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = "InterfaceObjectFactoryAsm";

            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            // This ModuleBuilder is used for all generated classes.  It's only constructed once, 
            //the first time that the InterfaceObjectFactory is used
            ModuleBuilder = assemblyBuilder.DefineDynamicModule("InterfaceObjectFactoryModule");
        }

        /// <summary>
        /// The module builder used for all types constructed
        /// </summary>
        static ModuleBuilder ModuleBuilder;

        /// <summary>
        /// Creates a method that will generate an object that implements the interface for the 
        /// given type.
        /// </summary>
        /// <param name="type"></param>
        private static void CreateTypeFor(Type type)
        {
            // Error checking...
            // Make sure that the type is an interface

            if (!type.IsInterface)
                throw new TypeIsNotAnInterface(type);

            TypeBuilder typeBuilder = ModuleBuilder.DefineType(
                "ImplOf" + type.Name, TypeAttributes.Class | TypeAttributes.Public);
            typeBuilder.AddInterfaceImplementation(type);

            // Create Constructor
            ConstructorInfo baseConstructorInfo = typeof(object).GetConstructor(new Type[0]);

            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(
                           MethodAttributes.Public,
                           CallingConventions.Standard,
                           Type.EmptyTypes);

            ILGenerator ilGenerator = constructorBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);                      // Load "this"
            ilGenerator.Emit(OpCodes.Call, baseConstructorInfo);    // Call the base constructor
            ilGenerator.Emit(OpCodes.Ret);                          // return

            // Get a list of all methods, including methods in inherited interfaces
            // The methods that aren't accessors and will need default implementations...  However,
            // a property's accessors are also methods!
            List<MethodInfo> methods = new List<MethodInfo>();
            AddMethodsToList(methods, type);

            // Get a list of all of the properties, including properties in inherited interfaces
            List<PropertyInfo> properties = new List<PropertyInfo>();
            AddPropertiesToList(properties, type);

            // Create accessors for each property
            foreach (PropertyInfo pi in properties)
            {
                string piName = pi.Name;
                Type propertyType = pi.PropertyType;

                // Create underlying field; all properties have a field of the same type
                FieldBuilder field = typeBuilder.DefineField(
                    "_" + piName, propertyType, FieldAttributes.Private);

                // If there is a getter in the interface, create a getter in the new type
                MethodInfo getMethod = pi.GetGetMethod();
                if (null != getMethod)
                {
                    // This will prevent us from creating a default method for the property's 
                    // getter
                    methods.Remove(getMethod);

                    // Now we will generate the getter method
                    MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                        getMethod.Name,
                        MethodAttributes.Public | MethodAttributes.Virtual,
                        propertyType,
                        Type.EmptyTypes);

                    // The ILGenerator class is used to put op-codes (similar to assembly) into the
                    // method
                    ilGenerator = methodBuilder.GetILGenerator();

                    // These are the op-codes, (similar to assembly)
                    ilGenerator.Emit(OpCodes.Ldarg_0);      // Load "this"
                    ilGenerator.Emit(OpCodes.Ldfld, field); // Load the property's underlying field onto the stack
                    ilGenerator.Emit(OpCodes.Ret);          // Return the value on the stack

                    // We need to associate our new type's method with the getter method in the 
                    // interface
                    typeBuilder.DefineMethodOverride(methodBuilder, getMethod);
                }

                // If there is a setter in the interface, create a setter in the new type
                MethodInfo setMethod = pi.GetSetMethod();
                if (null != setMethod)
                {
                    // This will prevent us from creating a default method for the property's 
                    // setter
                    methods.Remove(setMethod);

                    // Now we will generate the setter method
                    MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                        setMethod.Name,
                        MethodAttributes.Public | MethodAttributes.Virtual,
                        typeof(void),
                        new Type[] { pi.PropertyType });

                    // The ILGenerator class is used to put op-codes (similar to assembly) into the
                    // method
                    ilGenerator = methodBuilder.GetILGenerator();

                    // These are the op-codes, (similar to assembly)
                    ilGenerator.Emit(OpCodes.Ldarg_0);      // Load "this"
                    ilGenerator.Emit(OpCodes.Ldarg_1);      // Load "value" onto the stack
                    ilGenerator.Emit(OpCodes.Stfld, field); // Set the field equal to the "value" 
                                                            // on the stack
                    ilGenerator.Emit(OpCodes.Ret);          // Return nothing

                    // We need to associate our new type's method with the setter method in the 
                    // interface
                    typeBuilder.DefineMethodOverride(methodBuilder, setMethod);
                }
            }

            // Create default methods.  These methods will essentially be no-ops; if there is a 
            // return value, they will either return a default value or null
            foreach (MethodInfo methodInfo in methods)
            {
                // Get the return type and argument types

                Type returnType = methodInfo.ReturnType;

                List<Type> argumentTypes = new List<Type>();
                foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
                    argumentTypes.Add(parameterInfo.ParameterType);

                // Define the method
                MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                    methodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    returnType,
                    argumentTypes.ToArray());

                // The ILGenerator class is used to put op-codes (similar to assembly) into the
                // method
                ilGenerator = methodBuilder.GetILGenerator();

                // If there's a return type, create a default value or null to return
                if (returnType != typeof(void))
                {
                    LocalBuilder localBuilder =
                        ilGenerator.DeclareLocal(returnType);   // this declares the local object, 
                                                                // int, long, float, ect
                    ilGenerator.Emit(
                        OpCodes.Ldloc, localBuilder);           // load the value on the stack to 
                                                                // return
                }

                ilGenerator.Emit(OpCodes.Ret);                  // return

                // We need to associate our new type's method with the method in the interface
                typeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
            }

            // Finally, after all the fields and methods are generated, create the type for use at
            // run-time
            Type createdType = typeBuilder.CreateTypeInfo();
            InterfaceImplementations[type] = createdType;
        }

        /// <summary>
        /// Helper method to get all MethodInfo objects from an interface.  This recurses to all 
        /// sub-interfaces
        /// </summary>
        /// <param name="methods"></param>
        /// <param name="type"></param>
        private static void AddMethodsToList(List<MethodInfo> methods, Type type)
        {
            methods.AddRange(type.GetMethods());

            foreach (Type subInterface in type.GetInterfaces())
                AddMethodsToList(methods, subInterface);
        }

        /// <summary>
        /// Helper method to get all PropertyInfo objects from an interface.  This recurses to all 
        /// sub-interfaces
        /// </summary>
        /// <param name="methods"></param>
        /// <param name="type"></param>
        private static void AddPropertiesToList(List<PropertyInfo> properties, Type type)
        {
            properties.AddRange(type.GetProperties());

            foreach (Type subInterface in type.GetInterfaces())
                AddPropertiesToList(properties, subInterface);
        }

        /// <summary>
        /// Thrown when an attempt is made to create an object of a type that is not an interface
        /// </summary>
        public class TypeIsNotAnInterface : Exception
        {
            internal TypeIsNotAnInterface(Type type)
                : base("The InterfaceObjectFactory only works with interfaces.  "
                    + "An attempt was made to create an object for the following type, "
                    + "which is not an interface: " + type.FullName)
            { }
        }
    }
}
