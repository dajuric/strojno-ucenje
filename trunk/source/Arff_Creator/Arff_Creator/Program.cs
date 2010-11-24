using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arff_Creator
{
    class Program
    {
        static void Main(string[] args)
        {
            ArffWriter arffWriter = new ArffWriter("D:/proba.xml", 5, 5);
            arffWriter.Parse();
            arffWriter.Save("D:/proba.arff");
        }
    }
}
