using System;
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

            internal Sentence(List<string> leftWords, List<string> rightWords, string ambigousWord, string rawSenetnce)
            {
                LeftWords = leftWords;
                RightWords = rightWords;
                AmbigousWordKey = ambigousWord;
                RawSentence = rawSenetnce;
            }

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

        private List<Sentence> wholeSentences;

        public SentenceParser(XmlParser xmlParser)
        {
            if (!xmlParser.IsInitialized)
                xmlParser.Parse();

            this.Parser = xmlParser;

            this.IsInitialized = false;
            this.Initialize();
        }

        public bool IsInitialized
        {
            get;
            private set;
        }

        private void Initialize()
        {
            XmlNodeList lstSentenceNodes = this.Parser.SentenceNodes;

            List<Sentence> lstSentences = new List<Sentence>();

            foreach (XmlNode sentenceNode in lstSentenceNodes)
            {
                Sentence sentence = this.ParseSentence(sentenceNode, this.Parser.TagReplacement);
                lstSentences.Add(sentence);
            }

            this.wholeSentences = lstSentences;
            this.IsInitialized = true;
        }

        StringBuilder rawSenetence = new StringBuilder(); //ne koristim string jer je ovo brže
        private Sentence ParseSentence(XmlNode sentenceNode, string tagReplacement)
        {
            Sentence sentence = new Sentence();
            bool isLeftWindow = true;

            rawSenetence.Length = 0;

            foreach (XmlNode childNode in sentenceNode.ChildNodes)
            {
                switch (childNode.NodeType)
                {
                    case XmlNodeType.Element: //tag
                        rawSenetence.Append(tagReplacement);
                        sentence.AmbigousWordKey = childNode.Attributes[XmlParser.XmlTag.WORD_TAG_KEY].Value;                     

                        isLeftWindow = false;
                        break;
                    case XmlNodeType.Text: //tekst rečenice
                        rawSenetence.Append(childNode.InnerText);
                        List<string> words = GetWords(childNode.InnerText, isLeftWindow);

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
        private List<string> GetWords(string partOfSentnce, bool isLeftWindow)
        {
            List<string> lstWords = new List<string>();

            MatchCollection matches = regWords.Matches(partOfSentnce);
            int maxLength = matches.Count;

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

        public void Parse(int leftWindowSize, int rightWindowSize)
        {
            List<Sentence> lstSentences = new List<Sentence>();

            foreach (Sentence wholeSentence in this.wholeSentences)
            {
                int maxLeftLength = (leftWindowSize < wholeSentence.LeftWords.Count) ? leftWindowSize : wholeSentence.LeftWords.Count;
                int maxRightLength = (rightWindowSize < wholeSentence.RightWords.Count) ? rightWindowSize : wholeSentence.RightWords.Count;

                Sentence sentence = new Sentence(wholeSentence.LeftWords.GetRange(0, maxLeftLength), wholeSentence.RightWords.GetRange(0, maxRightLength), 
                                                 wholeSentence.AmbigousWordKey, wholeSentence.RawSentence);
                lstSentences.Add(sentence);
            }

            this.Sentences = lstSentences;
        }

        public List<Sentence> Sentences
        {
            get;
            private set;
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

        public XmlParser Parser
        {
            get;
            private set;
        }
    }
}
