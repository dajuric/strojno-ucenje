using System;
using System.Collections.Generic;
using WSD.ArffIO;
using WSD.Classifier;
using WSD.Parsers;

namespace XmlToArffConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            string learnSet = "D:/interest";
            //string trainSet = "D:/t";

            //string trainSet = "D:/learnLuk";
            //string  learnSet = "D:/trainLuk";                      

            Console.WriteLine("Learn set ...\n");
            
            XmlParser xmlReaderLearn = new XmlParser(learnSet + ".xml");
            XmlFeatureProvider xmlLearnFeautures = new XmlFeatureProvider(xmlReaderLearn, 3, 3);

            ArffWriter arffWriterLearn = new ArffWriter(xmlLearnFeautures, learnSet + ".arff");

            Console.WriteLine("Writing Arff...");
            arffWriterLearn.Write();


            //Console.WriteLine("\n\n\nTrain set ...\n");

            //XmlParser xmlReaderTrain = new XmlParser(trainSet + ".xml");
            //XmlFeatureProvider xmlTrainFeautures = new XmlFeatureProvider(xmlReaderTrain, xmlLearnFeautures);

            //ArffWriter arffWriterTrain = new ArffWriter(xmlTrainFeautures, trainSet + ".arff");

            //Console.WriteLine("Writing Arff...");
            //arffWriterTrain.Write(); 
        }
    }
}
