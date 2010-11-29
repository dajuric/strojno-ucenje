using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WSD.Classifier
{
    public abstract class Classifier
    {
        public struct ClassiferResult
        {
            public const string UNKNOWN_CLASSIFICATION = "?";
            
            string estimatedMeaning;
            string realMeaning;
            double probability;

            public ClassiferResult(string estimatedMeaning, string realMeaning, double probability)
            {
                this.estimatedMeaning = estimatedMeaning;
                this.realMeaning = realMeaning;
                this.probability = probability;
            }
            public string EstimatedMeaning        
            {
                get { return this.estimatedMeaning; }
            }

            public string RealMeaning
            {
                get { return this.realMeaning; } 
            }

            public double Probability
            {
                get { return this.probability; }
            }

            public override string ToString()
            {
                string isValid = (this.realMeaning == this.estimatedMeaning) ? "Valid" : "NotValid";
                return " p(" + this.estimatedMeaning + ") = " + Math.Round(this.probability, 2).ToString("N2") + "; " + isValid;
            }
        }

        public const double PROBABILITY_TRESHOLD = 0;

        protected IFeatureProvider learnSet;
        protected IFeatureProvider trainSet;

        public Classifier(IFeatureProvider learnSet, IFeatureProvider trainSet)
        {
            this.learnSet = learnSet; 
            this.trainSet = trainSet;
        }

        public abstract IEnumerable<ClassiferResult> Classify(double minValidProbabilty);

        public IEnumerable<ClassiferResult> Classify()
        {
            return this.Classify(PROBABILITY_TRESHOLD);
        }
    }
}
