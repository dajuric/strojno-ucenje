using System;
using System.Collections.Generic;


namespace WSD.Classifier.Statistics
{
    public class Statistics
    { 
        public Statistics(Classifier classifier)
        {
            if (!classifier.IsInitialized)
            {
                throw new Exception("Please classify at least one trainSet");
            }
            this.MyClassifier = classifier;
        }

        public Classifier MyClassifier
        {
            get;
            private set;
        }

        public double GetCoretness()
        {
            int totalSentences=0;
            int validClassifiedSentences = 0;

            foreach (Classifier.ClassiferResult result in MyClassifier.Result)
            {
                totalSentences++;

                if (result.IsValid)
                    validClassifiedSentences++;
            }

            return (double)validClassifiedSentences / totalSentences;
        }

    }
}
