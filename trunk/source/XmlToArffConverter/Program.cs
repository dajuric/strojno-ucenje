using System;
using System.Collections.Generic;
using WSD.ArffIO;
using WSD.Parsers;

namespace XmlToArffConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            string learnSet = "D:/learnLuk";
            string trainSet = "D:/trainLuk";
            
            int windowsSize = Int16.MaxValue;
            
            Console.WriteLine("Learn set ...\n");
            
            XmlParser xmlReaderLearn = new XmlParser(learnSet + ".xml");
            SentenceParser sentenceParserLearn = new SentenceParser(xmlReaderLearn, windowsSize, windowsSize);
            
            Console.WriteLine("Reading XML ...");
            xmlReaderLearn.Parse();

            ArffWriter arffWriterLearn = new ArffWriter(xmlReaderLearn, sentenceParserLearn, learnSet + ".arff"); 
            
            Console.WriteLine("Writing Arff...");
            arffWriterLearn.Write();



            Console.WriteLine("\n\n\nTrain set ...\n");

            XmlParser xmlReaderTrain = new XmlParser(trainSet + ".xml");
            SentenceParser sentenceParserTrain = new SentenceParser(xmlReaderTrain, windowsSize, windowsSize);
            
            Console.WriteLine("Reading XML ...");
            xmlReaderTrain.Parse();

            ArffWriter arffWriterTrain = new ArffWriter(xmlReaderTrain,sentenceParserTrain ,trainSet + ".arff");
            
            Console.WriteLine("Synchronizing attributes with learning set...");
            arffWriterTrain.OverrideAttributes(arffWriterLearn);
            
            Console.WriteLine("Writing Arff...");
            arffWriterTrain.Write(); 
        }
    }
}
