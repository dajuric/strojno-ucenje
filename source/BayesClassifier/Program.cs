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
            //string learnSet = "D:/learnLuk.xml";
            //string trainSet = "D:/trainLuk.xml";

            string learnSet = "D:/interest.xml";
            string trainSet = "D:/interest.xml";

            //SimpleClassifier.Run(learnSet, trainSet);

            Ensemble.Run(learnSet, trainSet);
        }
    }
}
