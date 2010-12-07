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
        
        public delegate void delProgressMessage(string message);
        public event delProgressMessage OnProgressMessage; 

        private Dictionary<int, int> indexWsizePair;
        Classifier[,] results;
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
            results = new Classifier[WindowSizes.Length, WindowSizes.Length];

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

        public Classifier this[int leftWindowSize, int rightWindowSize]
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
            int idxClassifier = 0;
            int numOfClassifiers = this.WindowSizes.Length * this.WindowSizes.Length;

            foreach (int leftWindow in WindowSizes)
            {
                foreach (int rightWindow in WindowSizes)
                {               
                    if (OnProgressMessage != null)
                    {
                        idxClassifier++;
                        OnProgressMessage("Getting data " + idxClassifier.ToString().PadLeft(2) + " of " + numOfClassifiers.ToString().PadLeft(2));
                    }

                    IFeatureProvider featureLearn, featureTrain;
                    GetData(leftWindow, rightWindow, OutputPath, out featureLearn, out featureTrain);

                    Classifier classifier = new NaiveBayes(featureLearn, featureTrain);
                    classifier.Classify();
                    //this[leftWindow, rightWindow] = classifier;
                    WSD.Classifier.Statistics.Statistics s = new WSD.Classifier.Statistics.Statistics(classifier);

                    Console.WriteLine("P" + "(" + leftWindow + ", " + rightWindow + ")= "
                                        + Math.Round(s.GetCoretness(), 2).ToString("N2"));

                    //Console.WriteLine("P" + "(" + leftWindow + ", " + rightWindow + ")");
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

        private void GetData(int leftWindow, int rightWindow, string outputArffPath, 
                             out IFeatureProvider featureLearn, out IFeatureProvider featureTrain)
        {
            XmlParser xmlReaderLearn = this.ParserLearn;
            XmlParser xmlReaderTrain = this.ParserTrain;
            
            string fileNameLearn=new FileInfo(xmlReaderLearn.XmlFile).Name.Split('.')[0];
            fileNameLearn=Path.Combine(outputArffPath, "Learn_" + fileNameLearn + "_" + leftWindow.ToString() + "x" + rightWindow.ToString() + ".arff");

            string fileNameTrain = new FileInfo(xmlReaderTrain.XmlFile).Name.Split('.')[0];
            fileNameTrain= Path.Combine(outputArffPath, "Train_" + fileNameTrain + "_" + leftWindow.ToString() + "x" + rightWindow.ToString() + ".arff");

            ArffWriter arffWriterLearn = new ArffWriter(xmlReaderLearn, fileNameLearn, leftWindow, rightWindow);

            arffWriterLearn.Write(); arffWriterLearn.Dispose();

            ArffWriter arffWriterTrain = new ArffWriter(xmlReaderTrain, fileNameTrain, leftWindow, rightWindow);

            arffWriterTrain.OverrideAttributes(arffWriterLearn);
            arffWriterTrain.Write(); arffWriterTrain.Dispose();

            ArffReader arffReaderLearn = new ArffReader(fileNameLearn);
            ArffReader arffReaderTrain = new ArffReader(fileNameTrain);

            arffReaderLearn.Parse();
            arffReaderTrain.Parse();

            featureLearn = new ArffFeatureProvider(arffReaderLearn);
            featureTrain = new ArffFeatureProvider(arffReaderTrain);

            //featureLearn = featureTrain = null;
        }

    }
}
