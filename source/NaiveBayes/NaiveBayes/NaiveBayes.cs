using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NaiveBayes
{
    class NaiveBayes
    {
        ArffReader arffLearnSet;
        ArffReader arffTrainSet;

        public NaiveBayes(ArffReader arffLearnSet, ArffReader arffTrainSet)
        {
            this.arffLearnSet = arffLearnSet;
            this.arffTrainSet = arffTrainSet;

            if (!arffLearnSet.IsInitialized)
                this.arffLearnSet.Parse();

            if (!arffTrainSet.IsInitialized)
                this.arffTrainSet.Parse();
        }

        public void Classify()
        {}

    }
}
