using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NaiveBayes
{
    class FeatureProvider
    {
        ArffReader arffData;

        public FeatureProvider(ArffReader arffData)
        {
            this.arffData = arffData;

            if (!arffData.IsInitialized)
                this.arffData.Parse();
        }

        //************skup za učenje********************/

        public Dictionary<string, int> AttributeColumn
        {
            get { return this.arffData.AttributeColumn; }
        }

        public List<string[]> DataRows
        {
            get { return this.arffData.DataRows; }
        }

        public ArffReader.Attribute DecisionAttribute
        {
            get { return this.arffData.DecisionAttribute; }
        }

        public int NumberOfSentences
        {
            get { return arffData.DataRows.Count; }
        }

        public int NumberOfAttributes
        {
            get { return arffData.AttributeColumn.Count; }
        }
    }
}
