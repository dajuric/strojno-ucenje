using System;
using System.Collections.Generic;
using WSD.ArffIO;
using WSD.Classifier;
using WSD.Parsers;

namespace BayesClassifier
{
    static class SimpleClassifier
    {
        public static void Run(string learnSet, string trainSet)
        {
            XmlParser xmlLearnParser=new XmlParser(learnSet);
            XmlFeatureProvider xmlLearnProvider = new XmlFeatureProvider(xmlLearnParser, 5, 5);

            XmlParser xmlTrainParser = new XmlParser(trainSet);
            XmlFeatureProvider xmlTrainProvider = new XmlFeatureProvider(xmlTrainParser, xmlLearnProvider);

            NaiveBayes nBayes = new NaiveBayes((IFeatureProvider)xmlLearnProvider);
            nBayes.TrainSet = xmlTrainProvider;

            nBayes.Classify();
            IEnumerable<Classifier.ClassiferResult> results = nBayes.Results;

            int idxSentence = 0;
            foreach (Classifier.ClassiferResult result in results)
            {
                //Console.WriteLine(xmlTrainParser.SentenceParser.Sentences[idxSentence].RawSentence + " (" + xmlTrainParser.Dictionary.Definitions[xmlTrainParser.SentenceParser.Sentences[idxSentence].AmbigousWordKey] + ")");
                Console.WriteLine(idxSentence + result.ToString());
                //Console.WriteLine();
                idxSentence++;
            }
        }

    }
}
