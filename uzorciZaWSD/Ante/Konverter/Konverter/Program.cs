using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Konverter
{
    class Program
    {
        static void Main(string[] args)
        {
            List<String> stareOznake = new List<string>();
            stareOznake.Add("<s>");
            stareOznake.Add("</s>");
            List<String> noveOznake = new List<string>();
            noveOznake.Add("<sentence>\n");
            noveOznake.Add("\n</sentence>");

            TextReader tr = new StreamReader("D:/pokusaji.txt");
            TextWriter tw = new StreamWriter("D:/rjesenje.xml");
            tw.WriteLine("<body>");

            String tekst = tr.ReadToEnd();
            int i=0;

            bool prepisuj = false;

            while (i < tekst.Length) 
            {
                bool detekcija = false;
                for (int j = 0; j < stareOznake.Count; j++)
                {
                    if (String.Compare(tekst, i, stareOznake[j], 0, stareOznake[j].Length) == 0)
                    {
                        tw.Write(noveOznake[j]);
                        i += stareOznake[j].Length;
                        if (j == 0) //samo prepisujemo stvari unutar tagova <s></s>, sve izvan ignoriramo jer nam ne treba
                        {
                            prepisuj = true;
                        }
                        else if (j == 1)
                        {
                            prepisuj = false;
                            tw.Write("\n"); //tako da se napravi novi red nakon svake recenice
                            tw.Write("\n");
                        }
                        detekcija = true;
                    }
                }
                if (detekcija == false)
                {
                    if(String.Compare(tekst,i, "<tag", 0, 4)==0){
                        int prvi = i;
                        int k = i;
                        while (tekst[k] != '"')
                        {
                            k++;
                        }
                        int nazivPrvi = k;
                        k++;
                        while (tekst[k] != '"')
                        {
                            k++;
                        }
                        int nazivZadnji = k;
                        while (String.Compare(tekst, k, "</>", 0, 3) != 0)
                        {
                            k++;
                        }
                        int zadnji = k + 2;

                        String key = tekst.Substring(nazivPrvi, nazivZadnji - nazivPrvi + 1);

                        tw.Write("<tag key=" + key + "/>");
                        i = zadnji + 1;
                    }
                    else
                    {
                        if (prepisuj == true)
                        {
                            tw.Write(tekst[i]);
                        }
                        i += 1;
                    }
                }
            }
            tr.Close();
            tw.WriteLine("</body>");
            tw.Close();
        }
    }
}
