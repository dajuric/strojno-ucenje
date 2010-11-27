using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WSD.Classifier
{
    class NaiveBayes
    {
        FeatureProvider learnSet;
        FeatureProvider trainSet;

        public NaiveBayes(FeatureProvider learnSet, FeatureProvider trainSet)
        {
            this.learnSet = learnSet; 
            this.trainSet = trainSet;
        }

        Dictionary<string, double> dicDecisionAttrib = new Dictionary<string, double>(); //služi samo za ubrzavanje ( npr. P(interest_1)  ) 
        private double CalculatePrbability(string decisionAttributeValue)
        {
            //ubrzavanje postupka
            if (dicDecisionAttrib.ContainsKey(decisionAttributeValue))
            {
                return dicDecisionAttrib[decisionAttributeValue];
            }

            double decisionAttribProbability = 0;
            for (int dataLine = 0; dataLine < learnSet.NumberOfSentences; dataLine++)
            {
                if (learnSet.DataRows[dataLine][learnSet.NumberOfAttributes - 1] == decisionAttributeValue)
                {
                    decisionAttribProbability++;
                }
            }

            decisionAttribProbability = decisionAttribProbability / learnSet.NumberOfSentences;
            dicDecisionAttrib.Add(decisionAttributeValue, decisionAttribProbability);
            return decisionAttribProbability;
        }

        Dictionary<string[], double> dicAttribPair = new Dictionary<string[], double>(); //služi samo za ubrzavanje (npr. P(apple | interest_1) )
        private double CalculateConditionalPrbability(string decisionAttributeValue, string attributeName)
        {
            //bool decisionAttribValExist = false;
            //foreach (string value in learnSet.DecisionAttribute.Values)
            //{
            //    if (value == decisionAttributeValue)
            //    {
            //        decisionAttribValExist = true;
            //        break;
            //    }
            //}

            //if (!decisionAttribValExist)
            //    throw new Exception("Decsion attribute value doesn't exist! -> " + decisionAttributeValue);

            if (learnSet.AttributeColumn.ContainsKey(attributeName))
            {
                return 0; //možda vratiti neku malu vjerojatnost ?
            }

            //ubrzavanje postupka
            string[] attribPair = new string[] { attributeName, decisionAttributeValue };
            if (dicAttribPair.ContainsKey(attribPair))
            {
                return dicAttribPair[attribPair];
            }

            int attribCol = learnSet.AttributeColumn[attributeName];

            double conditionalProbability = 0;
            int decisionAttribCount = 0;
            for (int dataLine = 0; dataLine < learnSet.NumberOfSentences; dataLine++)
            {
                if (learnSet.DataRows[dataLine][learnSet.NumberOfAttributes - 1] == decisionAttributeValue)
                {
                    decisionAttribCount++;

                    if (learnSet.DataRows[dataLine][attribCol] == "T")
                    {
                        conditionalProbability++;
                    }
                }           
            }

            conditionalProbability = conditionalProbability / decisionAttribCount;
            dicAttribPair.Add(attribPair, conditionalProbability);
            return conditionalProbability;
        }

        private double GetHypotesisProbability(string[] sentence, string decisionAttributeValue)
        {
            double hypotesisProbability = 1;

            foreach (string word in sentence)
            {
                hypotesisProbability *= CalculateConditionalPrbability(decisionAttributeValue, word);
            }

            hypotesisProbability *= CalculatePrbability(decisionAttributeValue);

            return hypotesisProbability;
        }

        public string Classify(double minValidProbabilty)
        {
            double maxProbability = Double.MinValue; string rightDecisionAttribValue = "";
            foreach (string[] sentence in trainSet.DataRows)
            {
                foreach (string decisionAttributeValue in learnSet.DecisionAttribute.Values)
                {
                    double hypotesisProbability = GetHypotesisProbability(sentence, decisionAttributeValue);
                    if (hypotesisProbability > maxProbability)
                    {
                        maxProbability = hypotesisProbability;
                        rightDecisionAttribValue = decisionAttributeValue;
                    }
                }
            }

            if (maxProbability >= minValidProbabilty)
                return rightDecisionAttribValue;
            else
                return "";
        }

        public string Classify()
        {
            return Classify(0);
        }
    }
}
