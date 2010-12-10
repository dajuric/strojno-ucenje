using System;
using System.Collections.Generic;
using WSD.GlobalStructures;

namespace WSD.Classifier
{
    public interface IFeatureProvider
    {
        int[,] DataTable
        {
            get;
        }

        string[] SentenceClasses
        {
            get;
        }

        WordAttribute ClassAttribute
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
