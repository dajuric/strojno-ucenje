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
            XmlParser xmlReader = new XmlParser("D:/interest.xml", 5, 5); Console.WriteLine("Reading XML...");
            xmlReader.Parse();

            ArffWriter arffWriter = new ArffWriter(xmlReader, "D:/interest.arff"); Console.WriteLine("Writing Arff...");
            arffWriter.Write(); 
        }
    }
}
