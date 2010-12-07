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
        SentenceParser sentenceParser;

        TextWriter txtWriter;

        Dictionary<string, int> dicAttributes; //ime atributa i njegova kolumna

        public ArffWriter(XmlParser xmlReader, string arffOutputFile, int leftWindowSize, int rightWindowSize)
        {            
            if (!xmlReader.IsInitialized)
                xmlReader.Parse();

            this.xmlReader = xmlReader;
            this.sentenceParser = xmlReader.SentenceParser;

            sentenceParser.Parse(leftWindowSize, rightWindowSize);
      
            txtWriter = new StreamWriter(arffOutputFile, false, Encoding.ASCII); 
            this.ArffFile = arffOutputFile;

            this.IsAttributesOverriden = false;
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
            if(!this.IsAttributesOverriden)
                this.dicAttributes = SetAttributes();

            WriteComment(xmlReader.Comment);

            string relation= new System.IO.FileInfo(xmlReader.XmlFile).Name.Split('.')[0];
            WriteRelation(relation);

            WriteAttributes();
            WriteData();  

            txtWriter.Flush();
        }

        private Dictionary<string, int> SetAttributes()
        {
            Dictionary<string, int> dicAttributes = new Dictionary<string,int>();
            
            foreach (SentenceParser.Sentence sentence in sentenceParser.Sentences)
            {
                foreach (string word in sentence.AllWords)
                {
                    if(!dicAttributes.ContainsKey(word))
                        dicAttributes.Add(word, dicAttributes.Count);
                }
            }

            dicAttributes.Add("definition", dicAttributes.Count); //definicijski atribut mora biti na kraju

            return dicAttributes;
        }

        public void OverrideAttributes(ArffWriter arffLearnWriter)
        {
            if (arffLearnWriter.dicAttributes == null)
                throw new Exception("Functon Write() from 'arffLearnWriter' must be executed first!");
            
            this.dicAttributes = arffLearnWriter.dicAttributes;
            this.IsAttributesOverriden = true;
        }

        public bool IsAttributesOverriden
        {
            get;
            private set;
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
            WordAttribute[] sortedAttributes = new WordAttribute[dicAttributes.Count]; //+1 za definicijski atribut

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

            foreach (SentenceParser.Sentence sentence in sentenceParser.Sentences)
            {
                bool[] data = new bool[dicAttributes.Count];
                foreach (string word in sentence.AllWords)
                {
                    if (dicAttributes.ContainsKey(word)) //neće sadržavati riječ ako su atributi uzeti iz druge arffDatoteke
                    {
                        int col = dicAttributes[word];
                        data[col] = true;
                    }
                }


                dataStr.Append("{");

                int index = 0;
                foreach (bool val in data) //zapiši praove (indexX "1", "indexY "1", ...)
                {
                    if(val==true)
                    {
                        dataStr.Append(index.ToString() + " " + WordAttribute.DEFAULT_VALUES[1]); 
                        dataStr.Append("," + " ");
                    }
                    index++;
                }

                dataStr.Append((dicAttributes.Count-1).ToString() + " " + sentence.AmbigousWordKey); //vrijednost atributa razreda        
 
                dataStr.Append("}");

                txtWriter.WriteLine(dataStr); 
                dataStr.Length = 0; //isprazni dataStr 
            }
        }
    }
}
