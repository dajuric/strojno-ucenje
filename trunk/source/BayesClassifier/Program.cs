using System;
using System.Collections.Generic;
using WSD.ArffIO;
using WSD.Classifier;

namespace BayesClassifier
{
    class Program
    {
        static void Main(string[] args)
        {
            ArffReader arffReader = new ArffReader("D:/interest.arff");
            arffReader.Parse();
        }
    }
}
