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

            //SimpleClassifier.Run(learnSet, trainSet);
            Ensemble.Run(learnSet + ".xml", trainSet + ".xml");
        }
    }
}
