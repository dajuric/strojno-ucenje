﻿using System;
using System.Collections.Generic;
using System.Xml;

namespace WSD.Parsers
{
    public class XmlParser
    {
        public static class XmlTag
        {
            public const string COMMENT = "comment";
            public const string TAG_REPLACEMENT = "tagReplacement";
            public const string DICTIONARY = "dictionary";
            public const string SENTENCE = "sentence";
            public const string WORD_TAG = "tag";
            public const string WORD_TAG_KEY = "key";
        }
        
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

        public XmlParser(string xmlFile)
        {
            xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFile);
            this.XmlFile = xmlFile;

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
            XmlNodeList comment = xmlDoc.GetElementsByTagName(XmlTag.COMMENT);
            if (comment != null)
                this.Comment = ParseComment(comment[0]);
            else
                this.Comment = "";

            //zamjena za tag (obično je to naziv višeznačne riječi)
            XmlNode tagReplacement = xmlDoc.GetElementsByTagName(XmlTag.TAG_REPLACEMENT)[0];
            this.TagReplacement = ParseTargReplacement(tagReplacement);

            //rječnik
            XmlNode dictionary = xmlDoc.GetElementsByTagName(XmlTag.DICTIONARY)[0];
            this.Dictionary = ParseDictionary(dictionary);

            //rečenice
            this.SentenceNodes = xmlDoc.GetElementsByTagName(XmlTag.SENTENCE);

            this.IsInitialized = true;
        }

        private string ParseComment(XmlNode comment)
        {
            return comment.InnerText.Trim();
        }

        private string ParseTargReplacement(XmlNode tagReplacement)
        {
            return tagReplacement.InnerText.Trim();
        }

        private WordDictionary ParseDictionary(XmlNode dictionary)
        {
            WordDictionary dict = new WordDictionary();

            foreach (XmlNode childNode in dictionary.ChildNodes)
            {
                dict.Add(childNode.Attributes[XmlTag.WORD_TAG_KEY].Value, childNode.InnerText.Trim());
            }

            return dict;
        }

        public string Comment
        {
            get;
            private set;
        }

        public string TagReplacement
        {
            get;
            private set;
        }

        public WordDictionary Dictionary
        {
            get;
            private set;
        }

        public XmlNodeList SentenceNodes
        {
            get;
            private set;
        }
    }
}
