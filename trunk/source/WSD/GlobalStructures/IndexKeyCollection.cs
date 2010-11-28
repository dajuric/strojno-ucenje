using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WSD.GlobalStructures
{
    public class IndexKeyCollection
    {
        Dictionary<int, string> dictIndexes;
        Dictionary<string, int> dictKeys;

        public IndexKeyCollection()
        {
            dictIndexes = new Dictionary<int, string>();
            dictKeys = new Dictionary<string, int>();
        }

        public void Add(string key, int index)
        {
            if (dictIndexes.ContainsKey(index) || dictKeys.ContainsKey(key))
                throw new ArgumentException("Index and key must be unique.");
            
            dictIndexes.Add(index, key);
            dictKeys.Add(key, index);
        }

        public void Remove(int index)
        {
            if(!dictIndexes.ContainsKey(index))
                throw new ArgumentNullException("Specificied index doesn't exist.");

            string key=dictIndexes[index];
            dictKeys.Remove(key);
            dictIndexes.Remove(index);
        }

        public void Remove(string key)
        {
            if (!dictKeys.ContainsKey(key))
                throw new ArgumentNullException("Specificied key doesn't exist.");
        
            int index = dictKeys[key];
            dictIndexes.Remove(index);
            dictKeys.Remove(key);
        }

        public int this[string key]
        {
            get { return this.dictKeys[key]; }
        }

        public string this[int index]
        {
            get { return this.dictIndexes[index]; }
        }

        public int Count
        {
            get { return this.dictIndexes.Count; }
        }

        public bool ContainsKey(string key)
        {
            return this.dictKeys.ContainsKey(key);
        }

        public bool ContainsIndex(int index)
        {
            return this.dictIndexes.ContainsKey(index);
        }
    }
}
