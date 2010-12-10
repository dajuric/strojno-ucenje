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

            public bool IsValid
            {
                get { return (this.realMeaning == this.estimatedMeaning); }
            }

            public override string ToString()
            {
                string isValid = (this.IsValid) ? "Valid" : "NotValid";
                return " p(" + this.estimatedMeaning + ") = " + Math.Round(this.probability, 2).ToString("N2") + "; " + isValid;
            }
        }

        public const double PROBABILITY_TRESHOLD = 0;

        protected IFeatureProvider learnSet;
        //protected IFeatureProvider trainSet;

        public Classifier(IFeatureProvider learnSet, IFeatureProvider trainSet)
        {
            this.learnSet = learnSet; 
            this.TrainSet = trainSet;

            this.IsInitialized = true;
        }

        public bool IsInitialized
        {
            get;
            private set;
        }

        protected abstract IEnumerable<ClassiferResult> GetClassification(double minValidProbabilty);

        public void Classify(double minValidProbabilty)
        {
            this.Results = GetClassification(minValidProbabilty);
            this.IsInitialized = true;
        }

        public void Classify()
        {
            this.Results = GetClassification(PROBABILITY_TRESHOLD);
            this.IsInitialized = true;
        }

        public IFeatureProvider TrainSet
        {
            get;
            set;
        }

        public IEnumerable<ClassiferResult> Results
        {
            get;
            private set;
        }
    }
}
