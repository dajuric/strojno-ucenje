using System;
using System.Collections.Generic;
using WSD.GlobalStructures;

namespace WSD.Classifier
{
    public interface IFeatureProvider
    {
        IndexKeyCollection AttributeColumn
        {
            get;
        }

        List<string[]> DataRows
        {
            get;
        }

        WordAttribute DecisionAttribute
        {
            get;
        }

        int NumberOfSentences
        {
            get;
        }

        int NumberOfAttributes
        {
            get;
        }
    }
}
