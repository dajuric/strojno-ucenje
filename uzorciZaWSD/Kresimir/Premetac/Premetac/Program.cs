using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace Premetac
{
    class Program
    {
        public static String ulaznaDatoteka = "D:/Test.xml";
        public static String izlaznaDatoteka = "D:/Izlaz.xml";
        public static int seed=0;

        public static List<String> recenice;

        public static List<String> randomizirajRecenice(List<String> stara)
        {
            List<String> nova = new List<String>();

            Random r = new Random(seed);
            int randomIndex = 0;
            while (stara.Count > 0)
            {
                randomIndex = r.Next(0, stara.Count);
                nova.Add(stara[randomIndex]);
                stara.RemoveAt(randomIndex);
            }

            return nova;
        }

        static void Main(string[] args)
        {
            recenice = new List<string>();

            TextWriter tw = new StreamWriter(izlaznaDatoteka);
            tw.WriteLine("<body>");

            XmlTextReader reader = new XmlTextReader(ulaznaDatoteka);
            
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name.Equals("sentence"))
                        {
                            recenice.Add(reader.ReadOuterXml());
                        }
                        else if(!reader.Name.Equals("body"))
                        {
                            tw.WriteLine(reader.ReadOuterXml());
                        }
                        break;
                    default:
                        break;
                }
            }
            recenice = randomizirajRecenice(recenice);

            foreach (String s in recenice)
            {
                tw.WriteLine(s);
            }

            tw.WriteLine("</body>");
            tw.Close();
        }
    }
}
