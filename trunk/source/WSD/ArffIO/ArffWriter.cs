using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using WSD.Parsers;
using WSD.GlobalStructures;

namespace WSD.ArffIO
{
    public class ArffWriter : IDisposable
    {            
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

            //dicAttributes.Add("definition", dicAttributes.Count); //definicijski atribut mora biti na kraju
            //nemoj dodati jer se vrijednosti toga atributa posebno upisuju (nakon petlji)
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
            WordAttribute[] sortedAttributes = new WordAttribute[dicAttributes.Count + 1]; //+1 za definicijski atribut

            foreach (KeyValuePair<string, int> attribCol in dicAttributes)
            {
                sortedAttributes[attribCol.Value] = new WordAttribute(attribCol.Key); //na kolumni value je određeni atribut
            }
           
            List<string> dictKeys=new List<string>();
            foreach(string key in xmlReader.Dictionary.Definitions.Keys)
            {
                dictKeys.Add(key);
            }
            sortedAttributes[sortedAttributes.Length - 1] = new WordAttribute("definition", dictKeys); //definicijski atribut

            foreach (WordAttribute attrib in sortedAttributes)
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
