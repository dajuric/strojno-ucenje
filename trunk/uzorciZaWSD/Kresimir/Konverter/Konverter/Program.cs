using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Konverter
{
    class Program
    {
        public static List<int> velicineTesta;
        public static List<String> klase;
        public static List<List<String>> recenice;

        public static int pronadjiKlasu(String naziv)
        {
            if(klase==null){
                klase = new List<string>();
            }

            for (int i = 0; i < klase.Count;i++ )
            {
                if (naziv.Equals(klase[i]))
                {
                    return i;
                }
            }
            klase.Add(naziv);
            int indexNove = klase.Count - 1;
            recenice.Add(new List<string>());
            return indexNove;
        }

        public static void obradiLiniju(String linija)
        {
            int indexKlase = -1;

            List<String> stareOznake = new List<string>();
            stareOznake.Add("<s>");
            stareOznake.Add("</s>");
            stareOznake.Add("<p>");
            stareOznake.Add("</p>");
            stareOznake.Add("<@>");
            stareOznake.Add("''");
            stareOznake.Add("``");
            stareOznake.Add("--");

            List<String> noveOznake = new List<string>();
            noveOznake.Add("");
            noveOznake.Add("");
            noveOznake.Add("");
            noveOznake.Add("");
            noveOznake.Add("");
            noveOznake.Add("");
            noveOznake.Add("");
            noveOznake.Add("");

            String novaLinija = "";

            int i = 0;
            bool prepisuj = false;
            while (i < linija.Length)
            {
                bool detekcija = false;
                if (String.Compare(linija, i, "<s snum", 0, 6) == 0)
                {
                    while (linija[i] != '>')
                    {
                        i++;
                    }
                    i++;
                    if (i >= linija.Length) break;
                }
                for (int j = 0; j < stareOznake.Count; j++)
                {
                    if (String.Compare(linija, i, stareOznake[j], 0, stareOznake[j].Length) == 0)
                    {
                        novaLinija += noveOznake[j];
                        i += stareOznake[j].Length;
                        if (j == 0) //samo prepisujemo stvari unutar tagova <s></s>, sve izvan ignoriramo jer nam ne treba
                        {
                            prepisuj = true;
                        }
                        else if (j == 1)
                        {
                            prepisuj = false;
                        }
                        detekcija = true;
                    }
                }
                if (detekcija == false)
                {
                    
                    if (String.Compare(linija, i, "<tag", 0, 4) == 0)
                    {
                        int prvi = i;
                        int k = i;
                        while (linija[k] != '"')
                        {
                            k++;
                        }
                        int nazivPrvi = k+1;
                        k++;
                        while (linija[k] != '"')
                        {
                            k++;
                        }
                        int nazivZadnji = k-1;
                        while (String.Compare(linija, k, "</>", 0, 3) != 0)
                        {
                            k++;
                        }
                        int zadnji = k + 2;

                        String key = linija.Substring(nazivPrvi, nazivZadnji - nazivPrvi + 1);

                        novaLinija += "<tag key=\"" + key + "\"/>";
                        indexKlase = pronadjiKlasu(key);

                        i = zadnji + 1;
                    }
                    else
                    {
                        if (prepisuj == true)
                        {
                            novaLinija += linija[i];
                        }
                        i += 1;
                    }
                }
            }
            if (!novaLinija.Equals(""))    //da eliminiramo prazne redove
            {
                novaLinija = "<sentence>\n" + novaLinija + "\n</sentence>";
                recenice[indexKlase].Add(novaLinija);
            }
            
        }

        public static void ispisiUDatoteke(String izlazPut)
        {
            for (int i = 0; i < recenice.Count; i++)
            {
                Console.WriteLine(izlazPut + "/" + klase[i] + "Test" + i.ToString() + ".xml");
                TextWriter twt = new StreamWriter(izlazPut + "/" + klase[i] + "Test" + i.ToString() + ".xml");
                twt.WriteLine("<body>");
                for (int j = 0; j < velicineTesta[i]; j++)
                {
                    twt.WriteLine(recenice[i][j]);
                }
                twt.WriteLine("</body>");
                twt.Close();

                TextWriter twl = new StreamWriter(izlazPut + "/" + klase[i] + "Learn" + i.ToString() + ".xml");
                twl.WriteLine("<body>");
                for (int j = velicineTesta[i]; j < recenice[i].Count; j++)
                {
                    twl.WriteLine(recenice[i][j]);
                }
                twl.WriteLine("</body>");
                twl.Close();
            }
        }

        static void Main(string[] args)
        {
            recenice = new List<List<string>>();
            
            Console.WriteLine("Unesi put do ulazne datoteke: ");
            String ulazPut = Console.ReadLine();
            Console.WriteLine("Unesi put do izlazne mape: ");
            String izlazPut = Console.ReadLine();
            ulazPut = "D:/uzorci/serve/serve.cor";
            izlazPut = "D:/uzorci/serve";

            TextReader tr = new StreamReader(ulazPut);
            
            String linija;
            while ((linija = tr.ReadLine()) != null)
            {
                obradiLiniju(linija);
            }

            velicineTesta = new List<int>();
            for (int i = 0; i < klase.Count; i++)
            {
                Console.WriteLine("Unesite velicinu skupa za testiranje za klasu " + klase[i] + ": ");
                int n = Int32.Parse(Console.ReadLine());
                velicineTesta.Add(n);
            }

            ispisiUDatoteke(izlazPut);

            tr.Close();            
        }
    }
}
