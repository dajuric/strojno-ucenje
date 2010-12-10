using System;
using System.Collections.Generic;
using WSD.ArffIO;
using WSD.Classifier;
using WSD.Parsers;

namespace BayesClassifier
{
    public static class Ensemble
    {
        public static void Run(string learnSet, string trainSet)
        {
            XmlParser xmlReaderLearn=new XmlParser(learnSet);
            XmlParser xmlReaderTrain=new XmlParser(trainSet);

            EnsembleProvider ensemble = new EnsembleProvider(xmlReaderLearn, xmlReaderTrain);
            ensemble.OnProgressMessage += new EnsembleProvider.delProgressMessage(ensemble_OnProgressMessage);

            ensemble.CreateEnsemble();
            //Console.WriteLine();
            //Console.WriteLine("======================================================");
            //Console.WriteLine();

            //foreach (int leftWindow in ensemble.WindowSizes)
            //{
            //    foreach (int rightWindow in ensemble.WindowSizes)
            //    {
            //        Statistics s = ensemble[leftWindow, rightWindow];

            //        Console.WriteLine("P" + "(" + leftWindow + ", " + rightWindow + ")= "
            //                            + Math.Round(s.GetCoretness(), 2).ToString("N2"));
            //    }
            //    Console.WriteLine();
            //}
        }

        static void ensemble_OnProgressMessage(int leftWindow, int rightWindow, Statistics s)
        {
            if (rightWindow == 0)
                Console.WriteLine();
            
            Console.WriteLine("P" + "(" + leftWindow + ", " + rightWindow + ")= "
                                        + Math.Round(s.GetCoretness(), 2).ToString("N2"));
        }

    }
}
