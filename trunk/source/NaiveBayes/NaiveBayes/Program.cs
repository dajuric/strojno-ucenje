using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NaiveBayes
{
    class Program
    {
        static void Main(string[] args)
        {
            ArffReader arffReader = new ArffReader("D:/proba.arff");
            arffReader.Parse();
        }
    }
}
