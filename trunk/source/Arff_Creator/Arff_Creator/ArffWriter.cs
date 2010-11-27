using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;

namespace Arff_Creator
{
    public class ArffWriter:IDisposable
    {
        public struct Attribute
        {
            public static readonly string[] DEFAULT_VALUES = new string[] { "da", "ne" };
            
            public string Name;
            public List<string> Values;

            public Attribute(string name)
            {
                if (!name.Contains(" "))
                    this.Name = name;
                else
                    throw new Exception("Atribut mora biti jedna riječ: " + name.ToString());

                this.Values = new List<string>() { DEFAULT_VALUES[0], DEFAULT_VALUES[1] };
            }

            public Attribute(string name, List<string> values)
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
        
        XmlParser xmlReader;
        TextWriter txtWriter;

        Dictionary<string, int> dicAttributes; //ime atributa i njegova kolumna
        
        public ArffWriter(XmlParser xmlReader, string arffOutputFile)
        {            
            if (!xmlReader.IsInitialized)
                xmlReader.Parse();

            this.xmlReader = xmlReader;
            txtWriter = new StreamWriter(arffOutputFile, false, Encoding.ASCII); 
            this.ArffFile = arffOutputFile;

            this.dicAttributes = new Dictionary<string, int>();
        }

        public void Dispose()
        {
            txtWriter.Close();
        }

        public string ArffFile
        {
            get;
            private set;
        }

        public void Write()
        {
            SetAttributes(ref dicAttributes);

            WriteComment(xmlReader.Comment);

            string relation= new System.IO.FileInfo(xmlReader.XmlFile).Name.Split('.')[0];
            WriteRelation(relation);

            WriteAttributes();
            WriteData();  

            txtWriter.Flush();
        }

        private void SetAttributes(ref Dictionary<string, int> dicAttributes)
        {
            foreach (SentenceParser.Sentence sentence in xmlReader.Sentences)
            {
                foreach (string word in sentence.AllWords)
                {
                    if(!dicAttributes.ContainsKey(word))
                        dicAttributes.Add(word, dicAttributes.Count);
                }
            }

            dicAttributes.Add("definition", dicAttributes.Count); //definicijski atribut mora biti na kraju
        }

        private void WriteComment(string comment)
        {
            if (comment != "")
            {
                string[] commentParts = comment.Split('\n');

                txtWriter.WriteLine("%");
                foreach (string c in commentParts)
                {
                    txtWriter.WriteLine("%" + c.Trim());
                }

                txtWriter.WriteLine("%");
            }
        }

        private void WriteRelation(string relation)
        {
            if (relation.Contains(" "))
                txtWriter.WriteLine("@RELATION " + "'" + relation + "'");
            else
                txtWriter.WriteLine("@RELATION " + relation);

            txtWriter.WriteLine();
        }

        private void WriteAttributes()
        {
            Attribute[] sortedAttributes=new Attribute[dicAttributes.Count];

            foreach (KeyValuePair<string, int> attribCol in dicAttributes)
            {
                sortedAttributes[attribCol.Value] = new Attribute(attribCol.Key); //na kolumni value je određeni atribut
            }
           
            List<string> dictKeys=new List<string>();
            foreach(string key in xmlReader.Dictionary.Definitions.Keys)
            {
                dictKeys.Add(key);
            }
            sortedAttributes[sortedAttributes.Length - 1] = new Attribute("definition", dictKeys); //popravi definicijski atribut

            foreach (Attribute attrib in sortedAttributes)
            {
                txtWriter.WriteLine("@ATTRIBUTE " + attrib.ToString());
            }

            txtWriter.WriteLine();
        }


        private void WriteData()
        {
            txtWriter.WriteLine("@DATA");
            txtWriter.WriteLine();

            StringBuilder dataStr = new StringBuilder();

            foreach (SentenceParser.Sentence sentence in xmlReader.Sentences)
            {
                bool[] data = new bool[dicAttributes.Count];
                foreach (string word in sentence.AllWords)
                {
                    int col = dicAttributes[word];
                    data[col] = true;
                }
               
                foreach (bool val in data)
                {
                    dataStr.Append((val) ? "T " : "F "); 
                }
                dataStr.Append(sentence.AmbigousWordKey); //vrijednost atributa razreda        

                txtWriter.WriteLine(dataStr); 
                dataStr.Length = 0; //isprazni dataStr 
            }
        }
    }
}
