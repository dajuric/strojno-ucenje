using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WSD.Classifier
{
    public class NaiveBayes : Classifier
    {
        public NaiveBayes(IFeatureProvider learnSet, IFeatureProvider trainSet)
            :base(learnSet, trainSet)
        {}

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

            //if (learnSet.AttributeColumn.ContainsKey(attributeName)) //ATRIBUTI MORAJU BITI SINHRONIZIRANI
            //{
            //    return 0; //možda vratiti neku malu vjerojatnost ?
            //}

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

                    if (learnSet.DataRows[dataLine][attribCol] == GlobalStructures.WordAttribute.DEFAULT_VALUES[1]) //"1"
                    {
                        conditionalProbability++;
                    }
                }           
            }

            conditionalProbability = conditionalProbability / decisionAttribCount;
            dicAttribPair.Add(attribPair, conditionalProbability);
            return conditionalProbability;
        }

        private double GetHypotesisProbability(string[] dataRow, string decisionAttributeValue)
        {
            double hypotesisProbability = 1; 

            int index=0;
            foreach (string data in dataRow)
            {
                if (data == GlobalStructures.WordAttribute.DEFAULT_VALUES[1]) //samo za riječi rečenice
                {
                    string word = learnSet.AttributeColumn[index];
                    hypotesisProbability *= CalculateConditionalPrbability(decisionAttributeValue, word);
                }
                index++;
            }

            hypotesisProbability *= CalculatePrbability(decisionAttributeValue);

            return hypotesisProbability;
        }

        public override IEnumerable<ClassiferResult> Classify(double minValidProbabilty)
        {           
            foreach (string[] dataRow in trainSet.DataRows)
            {
                double maxProbability = Double.MinValue; string estimatedMeaning = "";
                
                foreach (string decisionAttributeValue in learnSet.DecisionAttribute.Values)
                {
                    double hypotesisProbability = GetHypotesisProbability(dataRow, decisionAttributeValue);
                    if (hypotesisProbability > maxProbability)
                    {
                        maxProbability = hypotesisProbability;
                        estimatedMeaning = decisionAttributeValue;
                    }
                }

                if (maxProbability >= minValidProbabilty)
                    yield return new ClassiferResult(estimatedMeaning, dataRow[dataRow.Length-1], maxProbability);
                else
                    yield return new ClassiferResult(ClassiferResult.UNKNOWN_CLASSIFICATION, dataRow[dataRow.Length - 1], maxProbability);
            }         
        }
    }
}
