using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WSD.GlobalStructures
{
    public class WordAttribute
    {
        public static readonly string[] DEFAULT_VALUES = new string[] { "0", "1" };
        public static readonly string CLASS_ATTRIBUTE = "classAttrib";

        public string Name;
        public List<string> Values;

        public WordAttribute(string name)
        {
            if (!name.Contains(" "))
                this.Name = name;
            else
                throw new Exception("Atribut mora biti jedna riječ: " + name.ToString());

            this.Values = new List<string>() { DEFAULT_VALUES[0], DEFAULT_VALUES[1] };
        }

        public WordAttribute(string name, List<string> values)
        {
            if (!name.Contains(" "))
                this.Name = name;
            else
                throw new Exception("Atribut mora biti jedna riječ: " + name.ToString());

            if (values != null || values.Count != 0)
                this.Values = values;
            else
                throw new Exception("Lista atributa ne smije biti prazna.");
        }

        public override string ToString()
        {
            string values = "";
            foreach (string value in this.Values)
            {
                if (value.Contains(" "))
                    values += "'" + value + "'" + ", ";
                else
                    values += value + ", ";
            }
            values = values.Remove(values.Length - 2, 2); //makni posljednju ", "

            return this.Name + " {" + values + "}";
        }
    }
}
