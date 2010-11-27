using System;
using System.Collections.Generic;
using System.Xml;

namespace Arff_Creator
{
    public class XmlParser
    {
        public class WordDictionary
        {
            private Dictionary<string, string> definitions;

            public WordDictionary()
            {
                definitions = new Dictionary<string, string>();
            }

            internal void Add(string word, string description)
            {
                definitions.Add(word, description);
            }

            public IDictionary<string, string> Definitions
            {
                get { return definitions; }                
            }
        }

        XmlDocument xmlDoc;
        SentenceParser sentenceParser;

        public XmlParser(string xmlFile, int leftWindowSize, int rightWindowSize)
        {
            xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFile);
            this.XmlFile = xmlFile;

            sentenceParser = new SentenceParser(leftWindowSize, rightWindowSize);
            this.IsInitialized = false;
        }

        public bool IsInitialized
        {
            get;
            private set;
        }

        public string XmlFile
        {
            get;
            private set;
        }

        public void Parse()
        {
            //komentar
            XmlNodeList comment = xmlDoc.GetElementsByTagName("comment");
            if (comment != null)
                this.Comment = ParseComment(comment[0]);
            else
                this.Comment = "";

            //rječnik
            XmlNode dictionary = xmlDoc.GetElementsByTagName("dictionary")[0];
            this.Dictionary = ParseDictionary(dictionary);

            //rečenice
            XmlNodeList sentenceList = xmlDoc.GetElementsByTagName("sentence");
            List<SentenceParser.Sentence> lstSentences = new List<SentenceParser.Sentence>();
            foreach (XmlNode sentence in sentenceList)
            {
                ParseSentence(sentence, ref lstSentences);
            }
            this.Sentences = lstSentences;

            this.IsInitialized = true;
        }

        private string ParseComment(XmlNode comment)
        {
            return comment.InnerText.Trim();
        }

        private WordDictionary ParseDictionary(XmlNode dictionary)
        {
            WordDictionary dict=new WordDictionary();

            foreach (XmlNode childNode in dictionary.ChildNodes)
            {
                dict.Add(childNode.Attributes["key"].Value, childNode.InnerText);
            }

            return dict;
        }

        private void ParseSentence(XmlNode sentenceNode, ref List<SentenceParser.Sentence> lstSentences)
        {
            foreach (XmlNode childNode in sentenceNode.ChildNodes)
            {
                SentenceParser.Sentence sentence = sentenceParser.Parse(sentenceNode);
                lstSentences.Add(sentence);
            }
        }

        public string Comment
        {
            get;
            private set;
        }

        public WordDictionary Dictionary
        {
            get;
            private set;
        }

        public List<SentenceParser.Sentence> Sentences
        {
            get;
            private set;
        }


    }
}
