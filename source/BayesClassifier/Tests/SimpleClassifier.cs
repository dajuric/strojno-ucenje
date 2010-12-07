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
            Console.WriteLine("Learn set ...\n");

            ArffReader arffReaderLearn = new ArffReader(learnSet + ".arff");
            Console.WriteLine("Reading Arff...");
            arffReaderLearn.Parse();


            Console.WriteLine("\n\n\nTrain set ...\n");

            ArffReader arffReaderTrain = new ArffReader(trainSet + ".arff");
            Console.WriteLine("Reading Arff...");
            arffReaderTrain.Parse();


            Console.WriteLine("\n\n\nBayes ...\n");

            //samo za dekoraciju :)
            XmlParser xmlReaderTrain = new XmlParser(trainSet + ".xml");

            IFeatureProvider learnFeatureProvider = new ArffFeatureProvider(arffReaderLearn);
            IFeatureProvider trainFeatureProvider = new ArffFeatureProvider(arffReaderTrain);
            NaiveBayes naiveBayes = new NaiveBayes(learnFeatureProvider, trainFeatureProvider);

            naiveBayes.Classify();
            IEnumerable<Classifier.ClassiferResult> results = naiveBayes.Result;

            int idxSentence = 0;
            foreach (Classifier.ClassiferResult result in results)
            {
                Console.WriteLine(xmlReaderTrain.SentenceParser.Sentences[idxSentence].RawSentence + " (" + xmlReaderTrain.Dictionary.Definitions[xmlReaderTrain.SentenceParser.Sentences[idxSentence].AmbigousWordKey] + ")");
                Console.WriteLine(result.ToString());
                Console.WriteLine();
                idxSentence++;
            }
        }

    }
}
