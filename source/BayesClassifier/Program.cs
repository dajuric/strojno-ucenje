using System;
using System.Collections.Generic;
using WSD.ArffIO;
using WSD.Classifier;
using WSD.Parsers;

namespace BayesClassifier
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Learn set ...\n");
            
            ArffReader arffReaderLearn = new ArffReader("D:/learnLuk.arff"); 
            Console.WriteLine("Reading Arff...");
            arffReaderLearn.Parse();


            Console.WriteLine("\n\n\nTrain set ...\n");

            ArffReader arffReaderTrain = new ArffReader("D:/trainLuk.arff");
            Console.WriteLine("Reading Arff...");
            arffReaderTrain.Parse();


            Console.WriteLine("\n\n\nBayes ...\n");

            IFeatureProvider learnFeatureProvider = new ArffFeatureProvider(arffReaderLearn);
            IFeatureProvider trainFeatureProvider = new ArffFeatureProvider(arffReaderTrain);
            NaiveBayes naiveBayes = new NaiveBayes(learnFeatureProvider, trainFeatureProvider);

            IEnumerable<Classifier.ClassiferResult> results = naiveBayes.Classify();

            foreach (Classifier.ClassiferResult result in results)
            {
                Console.WriteLine(result.ToString());
            }

        }
    }
}
