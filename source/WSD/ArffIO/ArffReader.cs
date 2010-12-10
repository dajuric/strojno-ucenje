using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using WSD.GlobalStructures;

namespace WSD.ArffIO
{
    public class ArffReader : IDisposable
    {        
        StreamReader txtReader;
        
        IndexKeyCollection indexAttribPairs; //ime atributa i njegova kolumna u matrici
        List<string[]> attribValueMatrix;

        public ArffReader(string arffFile)
        {
            txtReader = new StreamReader(arffFile, System.Text.Encoding.ASCII);
            this.ArffFile = arffFile;

            indexAttribPairs = new IndexKeyCollection();
            this.IsInitialized = false;
            //Parse();
        }

        public void Dispose()
        {
            txtReader.Close();
        }

        public bool IsInitialized
        {
            get;
            private set;
        }

        public string ArffFile
        {
            get;
            private set;
        }

        public void Parse()
        {
            /******************** izdvoji ime relacije ****************/
            
            while (!txtReader.EndOfStream)
            {
                string line = txtReader.ReadLine();

                if (line.ToUpper().Contains("@RELATION"))
                {
                    this.Relation = ParseRelation(line);
                    break;
                }
            }

            /******************** izdvoji atribute ****************/

            string lastAttribute = "";
            while (!txtReader.EndOfStream)
            {
                string line = txtReader.ReadLine();

                if (line.ToUpper().Contains("@ATTRIBUTE"))
                {
                    ParseAttribute(line, ref indexAttribPairs);
                    lastAttribute = line;
                }
                else if (line.ToUpper().Contains("@DATA"))
                {
                    this.DecisionAttribute = ParseDecisionAttributeData(lastAttribute);   //zadnji atribut (izdvoji značajke)
                    break;
                }
            }

            /******************** izdvoji podatke ****************/

            while (!txtReader.EndOfStream)
            {
                //string line = txtReader.ReadLine(); već samo pročitali @data pa ovo ne smije biti uključeno
                attribValueMatrix = ParseData(txtReader);
            }

            this.IsInitialized = true;
        }

        Regex regRelationName = new Regex(@"@RELATION\s*(\'*[\w]+\'*)", RegexOptions.IgnoreCase);
        private string ParseRelation(string line)
        {
            Match m = regRelationName.Match(line);
            string relationName = m.Groups[1].Value; 

            return relationName;
        }

        Regex regAttributeName = new Regex(@"@ATTRIBUTE\s*([\w]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private void ParseAttribute(string line, ref IndexKeyCollection indexAttribPairs)
        {
            Match m = regAttributeName.Match(line);
            string attribName = m.Groups[1].Value;

            indexAttribPairs.Add(attribName, indexAttribPairs.Count);
        }

        Regex regDataInBrackets = new Regex(@"\{(.+)\}", RegexOptions.IgnoreCase | RegexOptions.IgnoreCase); //izdvaja podatke iz {} //(napraviti samo jedan regex !!!)
        Regex regExtractWords = new Regex(@"\w+", RegexOptions.IgnoreCase | RegexOptions.IgnoreCase); //izdvaja riječi (rabi se sa gronjim regEx-om)
        private WordAttribute ParseDecisionAttributeData(string line)
        {
            Match m = regAttributeName.Match(line);
            string attribName = m.Groups[1].Value;

            Match wordsInBrackets = regDataInBrackets.Match(line);
            MatchCollection matches = regExtractWords.Matches(wordsInBrackets.Groups[1].Value);

            List<string> data = new List<string>(matches.Count);
            for (int i = 0; i < matches.Count; i++)
            {
                data.Add(matches[i].Value);
            }

            return new WordAttribute(attribName, data);
        }

        Regex regData = new Regex(@"[\w]+", RegexOptions.Compiled); //ISPRAVI DA PRIHVAČA KOMENTARE!!!
        private List<string[]> ParseData(StreamReader txtReader)
        {
            List<string[]> dataLines=new List<string[]>();
            
            while (!txtReader.EndOfStream)
            {
                string line = txtReader.ReadLine();

                if(regData.IsMatch(line))
                {
                    MatchCollection matches=regData.Matches(line);

                    string[] dataLine=new string[this.AttributeColumn.Count];
                    for (int i = 0; i < dataLine.Length; i++)
                    {
                        dataLine[i] = "0";
                    }

                    for (int i = 0; i < matches.Count; i+=2)
                    {
                        int index = Int32.Parse(matches[i].Value);
                        string key = matches[i + 1].Value;
                        dataLine[index] = key;
                    }

                    dataLines.Add(dataLine);
                }
            }

            return dataLines;
        }

        public string Relation
        {
            get;
            private set;
        }

        public IndexKeyCollection AttributeColumn
        {
            get { return this.indexAttribPairs; }
        }

        public List<string[]> DataRows
        {
            get { return this.attribValueMatrix; }
        }

        public WordAttribute DecisionAttribute
        {
            get;
            private set;
        }
    }
}
