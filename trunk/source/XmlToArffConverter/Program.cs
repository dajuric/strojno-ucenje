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
            int windowsSize = Int16.MaxValue;
            
            Console.WriteLine("Learn set ...\n");
            
            XmlParser xmlReaderLearn = new XmlParser("D:/learnLuk.xml", windowsSize, windowsSize);          
            Console.WriteLine("Reading XML ...");
            xmlReaderLearn.Parse();

            ArffWriter arffWriterLearn = new ArffWriter(xmlReaderLearn, "D:/learnLuk.arff"); 
            Console.WriteLine("Writing Arff...");
            arffWriterLearn.Write();


            Console.WriteLine("\n\n\nTrain set ...\n");

            XmlParser xmlReaderTrain = new XmlParser("D:/trainLuk.xml", windowsSize, windowsSize); 
            Console.WriteLine("Reading XML ...");
            xmlReaderTrain.Parse();

            ArffWriter arffWriterTrain = new ArffWriter(xmlReaderTrain, "D:/trainLuk.arff");
            Console.WriteLine("Synchronizing attributes with learning set...");
            arffWriterTrain.OverrideAttributes(arffWriterLearn);
            Console.WriteLine("Writing Arff...");
            arffWriterTrain.Write(); 
        }
    }
}
