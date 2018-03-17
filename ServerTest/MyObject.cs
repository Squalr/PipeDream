
namespace ClientTest
{
    using System;

    [Serializable]
    public class MyObject
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public double IQ { get; set; }

        public MyObject()
        {

        }

        public MyObject(string name, int age, double iq)
        {
            this.Name = name;
            this.Age = age;
            this.IQ = iq;
        }

        public override string ToString()
        {
            return "Name: " + this.Name + Environment.NewLine +
                "Age: " + this.Age.ToString() + Environment.NewLine +
                "IQ: " + this.IQ.ToString() + Environment.NewLine;
        }
    }
}
