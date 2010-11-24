using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;

namespace Arff_Creator
{
    public class ArffWriter
    {
        XmlDocument xmlDoc;
        ArffDocument arffDoc;

        List<HashSet<string>> lstWordsInSentences = new List<HashSet<string>>(); //sadržava riječi koje se pojavljuju u pojedinoj rečenici
        List<string> lstWordMeaningsInSentences = new List<string>(); //sadržava značenja (ključ) višeznačne riječi u svakoj rečenici
        
        public ArffWriter(string xmlFile, int leftWindowSize, int rightWindowSize)
        {
            xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFile);

            this.LeftWindowSize = leftWindowSize;
            this.RightWindowSize = rightWindowSize;

            arffDoc = new ArffDocument();
            arffDoc.Relation = new System.IO.FileInfo(xmlFile).Name.Split('.')[0];
        }

        public void Parse()
        {
            XmlNodeList comment = xmlDoc.GetElementsByTagName("comment");
            if (comment.Count != 0)
                ParseComment(comment[0]);
            
            XmlNode dictionary = xmlDoc.GetElementsByTagName("dictionary")[0];
            ParseDictionary(dictionary);
            
            XmlNodeList sentenceList = xmlDoc.GetElementsByTagName("sentence");
            foreach (XmlNode sentence in sentenceList)
            {
                ParseSentence(sentence); 
            }
  
            FillData(lstWordsInSentences);
        }

        private void ParseComment(XmlNode comment)
        {
           arffDoc.Comment = comment.InnerText.Trim();
        }

        private void ParseDictionary(XmlNode dictionary)
        {        
            List<string> decisionAttribValues = new List<string>();

            foreach (XmlNode childNode in dictionary.ChildNodes)
            {
                decisionAttribValues.Add(childNode.Attributes["key"].Value);
            }

            ArffDocument.Attribute attrib = new ArffDocument.Attribute("definition", decisionAttribValues.ToArray());
            arffDoc.AddAttribute(attrib);
        }

        private void ParseSentence(XmlNode sentence)
        {
            string key = "";
            
            int preferedNumOfWords=this.LeftWindowSize;
            bool startFromEnd=true;

            HashSet<string> hsWords = new HashSet<string>();

            foreach (XmlNode childNode in sentence.ChildNodes)
            {
                switch (childNode.NodeType)
                {
                    case XmlNodeType.Element: //tag
                        //if(childNode.Name=="tag") //mora biti <tag key="***"/>
                            key = childNode.Attributes["key"].Value;
                            lstWordMeaningsInSentences.Add(key);    

                        preferedNumOfWords = this.RightWindowSize;
                        startFromEnd = false;
                        //Console.WriteLine(childNode.Name); 
                        break;
                    case XmlNodeType.Text: //tekst rečenice
                        List<string> words = GetWords(childNode.InnerText, preferedNumOfWords, startFromEnd);
                                          
                        foreach (string word in words)
                        {
                            if (!hsWords.Contains(word))
                                hsWords.Add(word);
                        }
                        //Console.WriteLine(childNode.InnerText.Trim() + "!");
                        break;
                }             
            }

            lstWordsInSentences.Add(hsWords);
        }

        Regex regWords = new Regex(@"[\w]+", RegexOptions.Compiled); //želim samo riječi...
        private List<string> GetWords(string partOfSentnce, int preferedNumOfWords, bool startFromEnd)
        {
            List<string> lstWords = new List<string>();

            MatchCollection matches = regWords.Matches(partOfSentnce);

            int maxLength = (matches.Count < preferedNumOfWords) ? matches.Count : preferedNumOfWords;

            if (startFromEnd)
            {
                for (int i = maxLength - 1; i >= 0; i--)
                    lstWords.Add(matches[i].Value);
            }
            else
            {
                for (int i = 0; i < maxLength; i++)
                    lstWords.Add(matches[i].Value);
            }

            return lstWords;
        }

        private void FillData(List<HashSet<string>> lstWordsInSentences)
        {
            //izdvoji jedinstvene atribute

            HashSet<string> attribs = new HashSet<string>();

            foreach (HashSet<string> sentenceWords in lstWordsInSentences)
            {
                attribs.UnionWith(sentenceWords);
            }

            foreach (string attrib in attribs)
            {
                ArffDocument.Attribute attr = new ArffDocument.Attribute(attrib, new string[]{ "da", "ne" });
                arffDoc.AddAttribute(attr);
            }

            arffDoc.DecisionAttribute = arffDoc.GetAttribute(0);

            //popuni @DATA

            int sentence=0;
            foreach (HashSet<string> sentenceWords in lstWordsInSentences)
            {
                ArffDocument.DataLine dataLine=new ArffDocument.DataLine(this.arffDoc);

                foreach (string word in attribs)
                {
                    dataLine.SetDataValue(word, 1); //za svaku riječ postavi "ne"
                }

                dataLine.SetDataValue(arffDoc.DecisionAttribute, lstWordMeaningsInSentences[sentence]);
                sentence++;

                foreach (string word in sentenceWords)
                {
                    dataLine.SetDataValue(word, 0); //za svaku riječ koja se pojavljuje u rečenici postavi "da"
                }


                arffDoc.DataLines.Add(dataLine);
            }
        }

        public int LeftWindowSize
        {
            get;
            private set;
        }

        public int RightWindowSize
        {
            get;
            private set;
        }

        public void Save(string fileName)
        {
            arffDoc.Save(fileName);
        }
    }
}
