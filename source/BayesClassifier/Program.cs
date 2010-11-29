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
            string learnSet = "D:/learnLuk";
            string trainSet = "D:/trainLuk";
           
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
            XmlParser xmlReaderTrain = new XmlParser(trainSet + ".xml", 0, 0);
            xmlReaderTrain.Parse();

            IFeatureProvider learnFeatureProvider = new ArffFeatureProvider(arffReaderLearn);
            IFeatureProvider trainFeatureProvider = new ArffFeatureProvider(arffReaderTrain);
            NaiveBayes naiveBayes = new NaiveBayes(learnFeatureProvider, trainFeatureProvider);

            IEnumerable<Classifier.ClassiferResult> results = naiveBayes.Classify();

            int idxSentence = 0;
            foreach (Classifier.ClassiferResult result in results)
            {
                Console.WriteLine(xmlReaderTrain.Sentences[idxSentence].RawSentence + " (" + xmlReaderTrain.Dictionary.Definitions[xmlReaderTrain.Sentences[idxSentence].AmbigousWordKey] + ")" );
                Console.WriteLine(result.ToString());
                Console.WriteLine();
                idxSentence++;
            }

        }
    }
}
