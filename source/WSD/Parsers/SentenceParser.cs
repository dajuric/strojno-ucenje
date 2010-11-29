﻿using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;

namespace WSD.Parsers
{
    public class SentenceParser
    {
        public class Sentence
        {
            public List<string> LeftWords;
            public List<string> RightWords;
            public string AmbigousWordKey;
            public string RawSentence;

            public Sentence()
            {
                LeftWords = new List<string>();
                RightWords = new List<string>();
                AmbigousWordKey = "";
                RawSentence = "";
            }

            public List<string> AllWords
            {
                get 
                {
                    List<string> allWords = new List<string>();
                    allWords.AddRange(LeftWords);
                    allWords.AddRange(RightWords);
                    return allWords; 
                }
            }
        }

        public SentenceParser(int leftWindowSize, int rightWindowSize)
        {
            this.LeftWindowSize = leftWindowSize;
            this.RightWindowSize = rightWindowSize;
        }

        StringBuilder rawSenetence = new StringBuilder(); //ne koristim string jer je ovo brže
        public Sentence Parse(XmlNode sentenceNode, string tagReplacement)
        {
            Sentence sentence = new Sentence();
            int preferedNumOfWords = this.LeftWindowSize;
            bool isLeftWindow = true;

            rawSenetence.Length = 0;

            foreach (XmlNode childNode in sentenceNode.ChildNodes)
            {
                switch (childNode.NodeType)
                {
                    case XmlNodeType.Element: //tag
                        rawSenetence.Append(tagReplacement);
                        sentence.AmbigousWordKey = childNode.Attributes[XmlParser.XmlTag.WORD_TAG_KEY].Value;                     

                        preferedNumOfWords = this.RightWindowSize;
                        isLeftWindow = false;
                        break;
                    case XmlNodeType.Text: //tekst rečenice
                        rawSenetence.Append(childNode.InnerText);
                        List<string> words = GetWords(childNode.InnerText, preferedNumOfWords, isLeftWindow);

                        if (isLeftWindow)
                            sentence.LeftWords = words;
                        else
                            sentence.RightWords = words;

                        break;
                }
            }

            sentence.RawSentence = rawSenetence.ToString().Trim();

            return sentence;
        }

        Regex regWords = new Regex(@"[\w]+", RegexOptions.Compiled); //želim samo riječi...
        private List<string> GetWords(string partOfSentnce, int preferedNumOfWords, bool isLeftWindow)
        {
            List<string> lstWords = new List<string>();

            MatchCollection matches = regWords.Matches(partOfSentnce);

            int maxLength = (matches.Count < preferedNumOfWords) ? matches.Count : preferedNumOfWords;

            //vrati riječi poslagane prema prostornoj bliskosti višeznačnoj riječi
            if (isLeftWindow)
            {
                for (int i = maxLength - 1; i >= 0; i--)
                    lstWords.Add(matches[i].Value.ToLower());
            }
            else
            {
                for (int i = 0; i < maxLength; i++)
                    lstWords.Add(matches[i].Value.ToLower());
            }

            return lstWords;
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
