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
            
            Console.WriteLine("Reading XML ...");
            xmlReaderLearn.Parse();

            ArffWriter arffWriterLearn = new ArffWriter(xmlReaderLearn, learnSet + ".arff", windowsSize, windowsSize); 
            
            Console.WriteLine("Writing Arff...");
            arffWriterLearn.Write();


            Console.WriteLine("\n\n\nTrain set ...\n");

            XmlParser xmlReaderTrain = new XmlParser(trainSet + ".xml");
            
            Console.WriteLine("Reading XML ...");
            xmlReaderTrain.Parse();

            ArffWriter arffWriterTrain = new ArffWriter(xmlReaderTrain,trainSet + ".arff", windowsSize, windowsSize);
            
            Console.WriteLine("Synchronizing attributes with learning set...");
            arffWriterTrain.OverrideAttributes(arffWriterLearn);
            
            Console.WriteLine("Writing Arff...");
            arffWriterTrain.Write(); 
        }
    }
}
