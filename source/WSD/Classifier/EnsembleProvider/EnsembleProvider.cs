using System;
using System.Collections.Generic;

using System.IO;

using WSD.Parsers;
using WSD.Classifier;
using WSD.ArffIO;


namespace WSD.Classifier
{
    public class EnsembleProvider
    {
        public static readonly int[] DEFAULT_WINDOW_SIZES = new int[] {0, 1, 2, 3, 4, 5, 10, 25, 50};
        
        public delegate void delProgressMessage(int leftWindow, int rightWindow, Statistics s);
        public event delProgressMessage OnProgressMessage; 

        private Dictionary<int, int> indexWsizePair;
        Statistics[,] results;
        string outputPath;
        
        public EnsembleProvider(XmlParser xmlParserLearn, XmlParser xmlParserTrain, params int[] windowSizes)
        {
            this.WindowSizes = windowSizes;

            PrepareStructures(xmlParserLearn, xmlParserTrain);
        }

        public EnsembleProvider(XmlParser xmlParserLearn, XmlParser xmlParserTrain)
        {
            this.WindowSizes = EnsembleProvider.DEFAULT_WINDOW_SIZES;

            PrepareStructures(xmlParserLearn, xmlParserTrain);
        }

        private void PrepareStructures(XmlParser xmlParserLearn, XmlParser xmlParserTrain)
        {
            this.ParserLearn = xmlParserLearn;
            this.ParserTrain = xmlParserTrain;

            indexWsizePair = InitializeDictionary();
            results = new Statistics[WindowSizes.Length, WindowSizes.Length];

            //outputPath = Path.GetTempPath();
            outputPath = @"D:\Ensemble\";
            this.IsInitialized = false;
        }

        public bool IsInitialized
        {
            get;
            private set;
        }

        public XmlParser ParserLearn
        {
            get;
            private set;
        }

        public XmlParser ParserTrain
        {
            get;
            private set;
        }

        public int[] WindowSizes
        {
            get;
            private set;
        }

        public string OutputPath
        {
            get { return this.outputPath; }
            set
            {
                if (Directory.Exists(value))
                {
                    this.outputPath = value;
                }
                else
                {
                    throw new IOException("Path: " + value + " doesn't exist.");
                }
            }
        }

        public Statistics this[int leftWindowSize, int rightWindowSize]
        {
            get
            {
                int idxLeft = indexWsizePair[leftWindowSize];
                int idxRight = indexWsizePair[rightWindowSize];

                return results[idxLeft, idxRight];
            }
            private set
            {
                int idxLeft = indexWsizePair[leftWindowSize];
                int idxRight = indexWsizePair[rightWindowSize];

                results[idxLeft, idxRight] = value;
            }
        }

        public void CreateEnsemble()
        {
            int numOfClassifiers = this.WindowSizes.Length * this.WindowSizes.Length;

            foreach (int leftWindow in WindowSizes)
            {
                foreach (int rightWindow in WindowSizes)
                {               
                     XmlFeatureProvider featuresLearn = new XmlFeatureProvider(this.ParserLearn, leftWindow, rightWindow);
                     XmlFeatureProvider featuresTrain = new XmlFeatureProvider(this.ParserTrain, featuresLearn);

                     Classifier classifier = new NaiveBayes((IFeatureProvider)featuresLearn);

                     SaveData(featuresLearn, featuresTrain);

                    classifier.TrainSet = featuresTrain;
                    classifier.Classify();

                    Statistics s = new WSD.Classifier.Statistics(classifier);
                    this[leftWindow, rightWindow] = s;

                    if (OnProgressMessage != null)
                    {
                        OnProgressMessage(leftWindow, rightWindow, s);
                    }
                }
            }

            this.IsInitialized = true;
        }

        private Dictionary<int, int> InitializeDictionary()
        {
            Dictionary<int, int> dictWindowKeys = new Dictionary<int, int>();
            
            for (int windowIndex = 0; windowIndex < WindowSizes.Length; windowIndex++)
            {
                dictWindowKeys.Add(this.WindowSizes[windowIndex], windowIndex);
            }

            return dictWindowKeys;
        }

        private void SaveData(XmlFeatureProvider featuresLearn, XmlFeatureProvider featuresTrain)
        {
            string fileNameLearn = new FileInfo(featuresLearn.XmlReader.XmlFile).Name.Split('.')[0];
            fileNameLearn = Path.Combine(this.outputPath, "Learn_" + fileNameLearn + "_" + featuresLearn.LeftWindowSize + "x" + featuresLearn.RightWindowSize + ".arff");

            string fileNameTrain = new FileInfo(featuresTrain.XmlReader.XmlFile).Name.Split('.')[0];
            fileNameTrain = Path.Combine(this.outputPath, "Train_" + fileNameTrain + "_" + featuresTrain.LeftWindowSize + "x" + featuresTrain.RightWindowSize + ".arff");

            ArffWriter arffWriterLearn = new ArffWriter(featuresLearn, fileNameLearn);
            arffWriterLearn.Write(); arffWriterLearn.Dispose();

            ArffWriter arffWriterTrain = new ArffWriter(featuresTrain, fileNameTrain);
            arffWriterTrain.Write(); arffWriterTrain.Dispose();
        }

    }
}
