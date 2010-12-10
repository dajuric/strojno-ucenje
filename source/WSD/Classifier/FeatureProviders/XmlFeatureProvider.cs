using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using WSD.Parsers;
using WSD.GlobalStructures;

namespace WSD.Classifier
{
    public class XmlFeatureProvider :IFeatureProvider
    {
        XmlParser xmlReader;
        SentenceParser sentenceParser;

        IndexKeyCollection attribIndexPair; 
        int[,] attribValues; //za svaku rečenicu atributi koje ona sadrži i njegova vrijednost (1 ili 0)

        public XmlFeatureProvider(XmlParser xmlReader, int leftWindowSize, int rightWindowSize)
        {
            if (!xmlReader.IsInitialized)
                xmlReader.Parse();

            this.LeftWindowSize = leftWindowSize;
            this.RightWindowSize = rightWindowSize;

            this.xmlReader = xmlReader;
            this.sentenceParser = xmlReader.SentenceParser;

            sentenceParser.Parse(leftWindowSize, rightWindowSize);

            this.IsAttributesOverriden = false;

            this.attribIndexPair = CreateAttribIndexPairs();
            this.attribValues = CreateTableOfAttribValues(this.attribIndexPair);
        }

        public XmlFeatureProvider(XmlParser xmlReader, XmlFeatureProvider attributeOverrider)
        {
            if (!xmlReader.IsInitialized)
                xmlReader.Parse();

            this.LeftWindowSize = attributeOverrider.LeftWindowSize;
            this.RightWindowSize = attributeOverrider.RightWindowSize;

            this.xmlReader = xmlReader;
            this.sentenceParser = xmlReader.SentenceParser;

            sentenceParser.Parse(attributeOverrider.LeftWindowSize, attributeOverrider.RightWindowSize);

            this.IsAttributesOverriden = true;

            this.attribIndexPair = attributeOverrider.AttributeIndexPair;
            this.attribValues = CreateTableOfAttribValues(this.attribIndexPair);
        }

        private string[] GetSentenceClasses()
        {
            List<string> decisionAttribs = new List<string>();

            foreach (SentenceParser.Sentence sentence in sentenceParser.Sentences)
            {
               decisionAttribs.Add(sentence.AmbigousWordKey);
            }

            return decisionAttribs.ToArray();
        }

        public WordAttribute GetClassAttribute()
        {
            XmlParser.WordDictionary dict=this.xmlReader.Dictionary;
            
            List<string> data = new List<string>(dict.Definitions.Count);

            foreach (string wordKey in dict.Definitions.Keys)
            {
                data.Add(wordKey);
            }

            return new WordAttribute(WordAttribute.CLASS_ATTRIBUTE, data);
        }

        /// <summary>
        /// Napravi tablicu vrijednosti atributa za rečenice
        /// </summary>
        /// <returns></returns>
        private int[,] CreateTableOfAttribValues(IndexKeyCollection attribIndexPairs)
        {
            int numOfSentences=xmlReader.SentenceParser.Sentences.Count;
            int numOfAttribs=attribIndexPairs.Count;

            int[,] dataLines = new int[numOfSentences, numOfAttribs];

            for (int idxSentence = 0; idxSentence < numOfSentences; idxSentence++)
            {
                List<SentenceParser.Sentence> sentences = xmlReader.SentenceParser.Sentences;

                foreach (string word in sentences[idxSentence].AllWords)
                {
                    if (attribIndexPairs.ContainsKey(word))
                    {
                        int idxWord = attribIndexPair[word];
                        dataLines[idxSentence, idxWord] = 1;
                    }
                }
            }

            return dataLines;
        }

        /// <summary>
        /// Izdvoji atribute (jedinstvene riječi u prozoru veličine leftWindowSize+rightWindowSize)
        /// </summary>
        /// <returns></returns>
        private IndexKeyCollection CreateAttribIndexPairs()
        {
            IndexKeyCollection attribIndexPair = new IndexKeyCollection();

            foreach (SentenceParser.Sentence sentence in sentenceParser.Sentences)
            {
                foreach (string word in sentence.AllWords)
                {
                    if (!attribIndexPair.ContainsKey(word))
                        attribIndexPair.Add(word, attribIndexPair.Count);
                }
            }

            //attribIndexPair.Add(WordAttribute.CLASS_ATTRIBUTE, attribIndexPair.Count); //attribut klase je zadnji

            return attribIndexPair;
        }

        public bool IsAttributesOverriden
        {
            get;
            private set;
        }

        public XmlParser XmlReader
        {
            get { return this.xmlReader; }
        }

        public IndexKeyCollection AttributeIndexPair
        {
            get { return this.attribIndexPair; }
        }

        public int[,] DataTable
        {
            get { return this.attribValues; }
        }

        string[] sentenceClasses=null;
        public string[] SentenceClasses
        {
            get 
            {
                if (sentenceClasses == null)
                    sentenceClasses = this.GetSentenceClasses();

                return sentenceClasses;
            }
        }

        WordAttribute classAttrib=null;
        public WordAttribute ClassAttribute
        {
            get
            {
                if (classAttrib == null)
                    classAttrib = GetClassAttribute();

                return classAttrib;
            }
        }

        public int NumberOfSentences
        {
            get { return this.attribValues.GetLength(0); }
        }

        public int NumberOfAttributes
        {
            get { return this.attribValues.GetLength(1); }
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

    }
}
