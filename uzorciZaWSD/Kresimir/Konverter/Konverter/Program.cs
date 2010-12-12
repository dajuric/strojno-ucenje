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
        public static String tagReplacement;
        public static List<String> learn;
        public static List<String> test;

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
            stareOznake.Add("&");
            stareOznake.Add("definition");
            stareOznake.Add("\"");
            stareOznake.Add("<compound>");
            stareOznake.Add("</compound>");

            List<String> noveOznake = new List<string>();
            noveOznake.Add("");
            noveOznake.Add("");
            noveOznake.Add("");
            noveOznake.Add("");
            noveOznake.Add("");
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

        public static List<String> randomiziraj(List<String> stara, int seed)
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

        public static void ispisiUDatoteku(String izlazPut, int seed)
        {
            test = randomiziraj(test, seed);
            learn = randomiziraj(learn, seed);

            TextWriter twt = new StreamWriter(izlazPut + "/" + "Test" + ".xml");
            twt.WriteLine("<body>");
            twt.WriteLine("<comment></comment>");
            twt.WriteLine("<tagReplacement>");
            twt.WriteLine(tagReplacement);
            twt.WriteLine("</tagReplacement>");
            twt.WriteLine("<dictionary>");
            for (int i = 0; i < klase.Count; i++)
            {
                twt.WriteLine("<definition key=\"" + klase[i] + "\"></definition>");
            }
            twt.WriteLine("</dictionary>");
            foreach (String s in test)
            {
                twt.WriteLine(s);
            }
            twt.WriteLine("</body>");
            twt.Close();

            TextWriter twl = new StreamWriter(izlazPut + "/" + "Learn" + ".xml");
            twl.WriteLine("<body>");
            twl.WriteLine("<comment></comment>");
            twl.WriteLine("<tagReplacement>");
            twl.WriteLine(tagReplacement);
            twl.WriteLine("</tagReplacement>");
            twl.WriteLine("<dictionary>");
            for (int i = 0; i < klase.Count; i++)
            {
                twl.WriteLine("<definition key=\"" + klase[i] + "\"></definition>");
            }
            twl.WriteLine("</dictionary>");
            foreach (String s in learn)
            {
                twl.WriteLine(s);
            }
            twl.WriteLine("</body>");
            twl.Close();
        }
        public static void podijeliSkupove()
        {
            test = new List<string>();
            learn = new List<string>();
            for (int i = 0; i < recenice.Count; i++)
            {
                for (int j = 0; j < velicineTesta[i]; j++)
                {
                    test.Add(recenice[i][j]);
                }
                for (int j = velicineTesta[i]; j < recenice[i].Count; j++)
                {
                    learn.Add(recenice[i][j]);
                }
            }
        }

        static void Main(string[] args)
        {
            recenice = new List<List<string>>();
            
            Console.WriteLine("Unesi put do ulazne datoteke: ");
            String ulazPut = Console.ReadLine();
            Console.WriteLine("Unesi put do izlazne mape: ");
            String izlazPut = Console.ReadLine();
            ulazPut = "D:/uzorci/line/line.cor";
            izlazPut = "D:/uzorci/line";
            Console.WriteLine("Unesi tag replacement: ");
            tagReplacement = Console.ReadLine();

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

            Console.WriteLine("Unesite seed za random: ");
            int seed = Int32.Parse(Console.ReadLine());

            podijeliSkupove();
            ispisiUDatoteku(izlazPut, seed);

            tr.Close();            
        }
    }
}
