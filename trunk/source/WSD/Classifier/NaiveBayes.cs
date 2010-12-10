using System;
using System.Collections.Generic;
using WSD.GlobalStructures;

namespace WSD.Classifier
{
    public class NaiveBayes : Classifier
    {        
        public NaiveBayes(IFeatureProvider learnSet)
            :base(learnSet, null)
        {            
            //Console.WriteLine("Start: cache ClassAttribProb");
            cachedClassAttribNum = CacheClassAttribNumber();
            //Console.WriteLine("Stop:  cache ClassAttribProb");

            this.conditionalProbCache = CreateConditionalProbabilityCache();
            this.isAttribColumnCached = new bool[learnSet.DataTable.GetLength(1)];
        }

        Dictionary<string, int> cachedClassAttribNum;

        private Dictionary<string, int> CacheClassAttribNumber()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();

            foreach (string decisionAttrib in learnSet.ClassAttribute.Values)
            {
                dict.Add(decisionAttrib, 0);
            }

            int numOfSentences = learnSet.NumberOfSentences;
            string[] decisionAttribs = learnSet.SentenceClasses;

            for (int dataLine = 0; dataLine < numOfSentences; dataLine++) //koliko ima rečenica sa atributom klase descisionAttributeValue
            {
                dict[decisionAttribs[dataLine]]++;
            }

            //string[] keys = new string[dict.Keys.Count];
            //dict.Keys.CopyTo(keys, 0);

            //foreach (string key in keys)
            //{
            //    dict[key] = dict[key] / numOfSentences;
            //}

            return dict;
        }

        private double CalculatePrbability(string decisionAttributeValue)
        {
            return this.cachedClassAttribNum[decisionAttributeValue] / (double)learnSet.NumberOfSentences;
        }

        Dictionary<string, double[]> conditionalProbCache;
        bool[] isAttribColumnCached;

        private Dictionary<string, double[]> CreateConditionalProbabilityCache()
        {
            int numOfAllAttributes = learnSet.DataTable.GetLength(1);
            
            Dictionary<string, double[]> dict = new Dictionary<string, double[]>();
            foreach (string decisionAttrib in learnSet.ClassAttribute.Values)
            {
                double[] array = new double[numOfAllAttributes];
                dict.Add(decisionAttrib, array);
            }

            return dict;
        }

        private void UpdateConditionalProbabilityCache(ref Dictionary<string, double[]> dict, int attribColumn)
        {
            int numOfSentences = learnSet.NumberOfSentences;
            string[] decisionAttribs = learnSet.SentenceClasses;
            for (int dataLine = 0; dataLine < numOfSentences; dataLine++) //koliko ima rečenica sa atributom klase descisionAttributeValue
            {
                dict[decisionAttribs[dataLine]][attribColumn] += learnSet.DataTable[dataLine, attribColumn];
            }

            string[] keys = new string[dict.Keys.Count];
            dict.Keys.CopyTo(keys, 0);

            foreach (string key in keys)
            {
                //if (dict[key][attribColumn] == 0)
                //    dict[key][attribColumn] = 1;

                dict[key][attribColumn] = (dict[key][attribColumn] + 1) / (this.cachedClassAttribNum[key] + 2);
            }  
        }
     
        private unsafe double CalculateConditionalPrbability(string decisionAttributeValue, int attribColumn)
        {
            if (this.isAttribColumnCached[attribColumn] == false)
            {
                UpdateConditionalProbabilityCache(ref this.conditionalProbCache, attribColumn);
                this.isAttribColumnCached[attribColumn] = true;
            }

            return this.conditionalProbCache[decisionAttributeValue][attribColumn];
        }

        private double GetHypotesisProbability(int[,] allAttribValues, int idxSentence, string decisionAttributeValue)
        {
            double hypotesisProbability = 1;

            for (int idxAttrib = 0; idxAttrib < learnSet.NumberOfAttributes; idxAttrib++)
            {
                if (allAttribValues[idxSentence, idxAttrib] == 1) //samo za riječi rečenice
                    hypotesisProbability *= CalculateConditionalPrbability(decisionAttributeValue, idxAttrib);
            }

            hypotesisProbability *= CalculatePrbability(decisionAttributeValue);

            return hypotesisProbability;
        }

        protected override IEnumerable<ClassiferResult> GetClassification(double minValidProbabilty)
        {
            List<ClassiferResult> l = new List<ClassiferResult>();

            WordAttribute learnClassAttrib = learnSet.ClassAttribute;

            for (int idxSentence = 0; idxSentence < TrainSet.NumberOfSentences; idxSentence++)
            {
                double maxProbability = Double.MinValue; string estimatedMeaning = "";

                foreach (string decisionAttributeValue in learnClassAttrib.Values)
                {
                    double hypotesisProbability = GetHypotesisProbability(TrainSet.DataTable, idxSentence, decisionAttributeValue);

                    if (hypotesisProbability > maxProbability)
                    {
                        maxProbability = hypotesisProbability;
                        estimatedMeaning = decisionAttributeValue;
                    }
                }

                if (maxProbability >= minValidProbabilty)
                    l.Add(new ClassiferResult(estimatedMeaning, TrainSet.SentenceClasses[idxSentence], maxProbability));
                else
                    l.Add(new ClassiferResult(ClassiferResult.UNKNOWN_CLASSIFICATION, TrainSet.SentenceClasses[idxSentence], maxProbability));
            }

            return l;
        }
    }
}
