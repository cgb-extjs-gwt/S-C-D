using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Gdc.Scd.Core.Meta.Interfaces;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class MetaCollection<T> : ICollection<T> where T : IMetaIdentifialble
    {
        private readonly Dictionary<string, T> dictionary = new Dictionary<string, T>();

        public int Count => this.dictionary.Count;

        public bool IsReadOnly => false;

        public T this[string id]
        {
            get
            {
                return this.dictionary[id];
            }

            set
            {
                this.dictionary[id] = value;
            }
        }

        public void Add(T item)
        {
            this.dictionary.Add(item.Id, item);
        }

        public void Clear()
        {
            this.dictionary.Clear();
        }

        public bool Contains(T item)
        {
            return this.dictionary.ContainsValue(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.dictionary.Values.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.dictionary.Values.GetEnumerator();
        }

        public bool Remove(string id)
        {
            return this.dictionary.Remove(id);
        }

        public bool Remove(T item)
        {
            var result = false;

            if (this.Contains(item))
            {
                result = this.Remove(item.Id);
            }

            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
