using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using WSD.Parsers;
using WSD.Classifier;
using WSD.GlobalStructures;

namespace WSD.ArffIO
{
    public class ArffWriter : IDisposable
    {            
        XmlFeatureProvider xmlFeatures;

        TextWriter txtWriter;

        public ArffWriter(XmlFeatureProvider xmlFeatureProvider, string arffOutputFile)
        {            
            this.xmlFeatures = xmlFeatureProvider;
      
            txtWriter = new StreamWriter(arffOutputFile, false, Encoding.ASCII); 
            this.ArffFile = arffOutputFile;
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
            XmlParser xmlReader = xmlFeatures.XmlReader;
            WriteComment(xmlReader.Comment);
            string relation= new System.IO.FileInfo(xmlReader.XmlFile).Name.Split('.')[0];
            WriteRelation(relation);

            WriteAttributes();
            WriteData();  

            txtWriter.Flush();
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
            for (int idxAttrib = 0; idxAttrib < xmlFeatures.NumberOfAttributes; idxAttrib++)
            {
                string attribName = xmlFeatures.AttributeIndexPair[idxAttrib];
                txtWriter.WriteLine("@ATTRIBUTE " + new WordAttribute(attribName).ToString());
            }

            WordAttribute classAttrib = xmlFeatures.GetClassAttribute();
            txtWriter.WriteLine("@ATTRIBUTE " + new WordAttribute(classAttrib.Name, classAttrib.Values).ToString());

            txtWriter.WriteLine();
        }


        private void WriteData()
        {
            txtWriter.WriteLine("@DATA");
            txtWriter.WriteLine();

            int indexOfClassAttrib = xmlFeatures.NumberOfAttributes - 1 + 1;

            StringBuilder dataStr = new StringBuilder();

            for (int idxSenetnce = 0; idxSenetnce < xmlFeatures.NumberOfSentences; idxSenetnce++)
            {
                dataStr.Append("{");

                for (int idxAttrib = 0; idxAttrib < xmlFeatures.NumberOfAttributes; idxAttrib++)
                {
                    if (xmlFeatures.DataTable[idxSenetnce, idxAttrib] == 1) 
                    {
                        dataStr.Append(idxAttrib.ToString() + " " + WordAttribute.DEFAULT_VALUES[1]);
                        dataStr.Append("," + " ");
                    }
                }

                string[] sentenceClasses = xmlFeatures.SentenceClasses;
                dataStr.Append(indexOfClassAttrib.ToString() + " " + sentenceClasses[idxSenetnce]); //vrijednost atributa razreda 
                
                dataStr.Append("}");

                txtWriter.WriteLine(dataStr);
                dataStr.Length = 0; //isprazni dataStr 
            }         
        }

    }
}
