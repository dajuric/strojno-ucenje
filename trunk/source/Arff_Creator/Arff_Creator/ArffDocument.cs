using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace Arff_Creator
{
    class ArffDocument
    {
        public struct Attribute
        {
            private string name;
            private string[] values;
         
            public Attribute(string name, string[] values)
            {
                if (!name.Contains(" "))
                    this.name = name;
                else
                    throw new Exception("Atribut mora biti jedna riječ: " + name.ToString());

                if(values!=null || values.Length!=0)
                    this.values = values;
                else
                    throw new Exception("Lista atributa ne smije biti prazna.");
            }

            public string Name
            {
                get { return this.name; }
            }

            public string[] Values
            {
                get { return this.values; }
            }

            public override string ToString()
            {
                string values = "";
                foreach (string value in this.Values)
                {
                    if (value.Contains(" "))
                        values += "'" + value + "'" + ", ";
                    else
                        values += value + ", "; 
                }
                values = values.Remove(values.Length - 2, 2); //makni posljednju ", "

                return "@ATTRIBUTE " + this.Name + " {" + values + "}";
            }
        }

        public struct DataLine
        {
            private string[] values;
            private AttributeIndexCollection attribIndexCol;

            public DataLine(ArffDocument arffDoc)
            {
                this.attribIndexCol = arffDoc.attribIndexCol;
                this.values = new string[arffDoc.attribIndexCol.Count];
            }

            public void SetDataValue(Attribute attr, int indexOfDesiredAttributeValue)
            {
                int attribIndex = attribIndexCol[attr];
                values[attribIndex] = attr.Values[indexOfDesiredAttributeValue];
            }

            public void SetDataValue(Attribute attr, string attribValue)
            {
                int attribIndex = attribIndexCol[attr];
                values[attribIndex] = attribValue; //TREBA BITI PROVJERA nalazi li se vr. u vr. atributa (ISPRAVTI)
            }

            public void SetDataValue(string attrName, int indexOfDesiredAttributeValue)
            {
                Attribute attrib = attribIndexCol[attrName];
                int attribIndex = attribIndexCol[attrib];
               
                values[attribIndex] = attrib.Values[indexOfDesiredAttributeValue];
            }

            public string[] Values
            {
                get { return this.values; }
            }

            public override string ToString()
            {
                string values = "";

                foreach (string value in this.values)
                {
                    if (value.Contains(" "))
                        values += "'" + value + "'" + " ";
                    else
                        values += value + " ";
                }

                return values;
            }

        }

        public class AttributeIndexCollection
        {
            SortedDictionary<int, Attribute> dictAttribValues;
            Dictionary<Attribute, int> dictIndexValues;

            SortedDictionary<string, Attribute> dictAttribKeys;

            public AttributeIndexCollection()
            {
                dictAttribValues = new SortedDictionary<int, Attribute>();
                dictIndexValues = new Dictionary<Attribute, int>();
                dictAttribKeys = new SortedDictionary<string, Attribute>();
            }

            public int this[Attribute attrib]
            {
                get { return dictIndexValues[attrib]; }
            }

            public Attribute this[int index]
            {
                get { return dictAttribValues[index]; }
            }

            public Attribute this[string attribName]
            {
                get { return dictAttribKeys[attribName]; } 
            }

            public void Add(Attribute attrib, int index)
            {
                if (!dictAttribValues.ContainsKey(index) && !dictIndexValues.ContainsKey(attrib))
                {
                    dictAttribValues.Add(index, attrib); 
                    dictIndexValues.Add(attrib, index);

                    dictAttribKeys.Add(attrib.Name, attrib);
                }
            }

            public void RemoveBy(Attribute attrib)
            {
                if (dictIndexValues.ContainsKey(attrib))
                {
                    int index = dictIndexValues[attrib];

                    dictIndexValues.Remove(attrib);
                    dictAttribValues.Remove(index);

                    dictAttribKeys.Remove(attrib.Name);
                }
            }

            public void RemoveBy(int index)
            {
                if (dictAttribValues.ContainsKey(index))
                {
                    Attribute attrib = dictAttribValues[index]; 

                    dictIndexValues.Remove(attrib);
                    dictAttribValues.Remove(index);

                    dictAttribKeys.Remove(attrib.Name);
                }
            }

            public bool Contains(Attribute attrib)
            {
                return dictIndexValues.ContainsKey(attrib);
            }

            public bool Contains(int index)
            {
                return dictAttribValues.ContainsKey(index);
            }

            public int Count
            {
                get { return dictAttribValues.Count; }
            }

            public IEnumerable<Attribute> GetAttributesByIndex()
            {
                foreach (int index in dictAttribValues.Keys)
                {
                    yield return dictAttribValues[index];
                }
            }
        }

        internal AttributeIndexCollection attribIndexCol;

        public ArffDocument()
        {
            attribIndexCol = new AttributeIndexCollection();
            this.DataLines = new List<DataLine>();

            this.Comment = "";
            this.Relation = "";
        }

        public string Comment
        {
            get;
            set;
        }

        public string Relation
        {
            get;
            set;
        }

        public void AddAttribute(Attribute attrib)
        {
            attribIndexCol.Add(attrib, attribIndexCol.Count); //dodaj kao posljednji atribut
        }

        public int GetAttributeIndex(Attribute attrib)
        {
            return attribIndexCol[attrib];
        }

        public Attribute GetAttribute(int index)
        {
            return attribIndexCol[index];
        }

        public bool ContainsAttribute(Attribute attrib)
        {
            return attribIndexCol.Contains(attrib);
        }

        public Attribute DecisionAttribute
        {
            get 
            {
                return attribIndexCol[attribIndexCol.Count - 1];
            }
            set 
            {
                //pomakni decisionAtribute na kraj (pridodjeli mu najveći indeks)
                if (attribIndexCol.Count != 0 && attribIndexCol.Contains(value))
                {
                    int decisionAttribIdx = attribIndexCol[value];
                    Attribute decisionAttrib = attribIndexCol[decisionAttribIdx]; //mogu vrijednosti atributa biti promijenjene

                    int lastAttribIdx = attribIndexCol.Count - 1;
                    Attribute lastAttrib = attribIndexCol[lastAttribIdx];

                    attribIndexCol.RemoveBy(decisionAttrib);
                    attribIndexCol.RemoveBy(lastAttrib);

                    attribIndexCol.Add(decisionAttrib, lastAttribIdx);
                    attribIndexCol.Add(lastAttrib, decisionAttribIdx);
                }
            }
        }

        public List<DataLine> DataLines
        {
            get;
            private set;
        }

        public void Save(string fileName)
        {
            TextWriter txtWriter = new StreamWriter(fileName);

            //komentar
            if (this.Comment != "")
            {
                string[] comment = this.Comment.Split('\n');

                txtWriter.WriteLine("%");
                foreach (string c in comment)
                {
                    txtWriter.WriteLine("%" + c.Trim());
                }
   
                txtWriter.WriteLine("%");
            }

            //ime relacije
            if (Relation.Contains(" "))
                txtWriter.WriteLine("@RELATION " +  "'" + Relation + "'");
            else
                txtWriter.WriteLine("@RELATION " + Relation);

            txtWriter.WriteLine();

            //atributi
            foreach (Attribute attr in this.attribIndexCol.GetAttributesByIndex())
            {
                txtWriter.WriteLine(attr.ToString());
            }

            txtWriter.WriteLine();

            //podaci          
            txtWriter.WriteLine("@DATA");
            txtWriter.WriteLine();

            foreach (DataLine dataLine in this.DataLines) //bilo bi lijepo provjeriti ima li jednako vrijednosti Atributes i pojedini dataLine....
            {
                txtWriter.WriteLine(dataLine.ToString());
            }

            txtWriter.Close();
        }
    }
}
