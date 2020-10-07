using System;
using System.Collections;
using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgDictionary : IDictionary<string, DwgObject>
    {
        private Dictionary<string, DwgObject> _entries = new Dictionary<string, DwgObject>(StringComparer.OrdinalIgnoreCase);

        internal override IEnumerable<DwgObject> ChildItems => _entries.Values;

        internal override void OnBeforeObjectWrite()
        {
            base.OnBeforeObjectWrite();
            _names.Clear();
            foreach (var kvp in _entries)
            {
                var name = kvp.Key;
                var entry = kvp.Value;
                _entityHandleReferences.Add(entry.MakeHandleReference(DwgHandleReferenceCode.None));
                _names.Add(name);
                // TODO: set parent handle
            }
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache)
        {
            _entries.Clear();
            if (_entityHandleReferences.Count != _names.Count)
            {
                throw new DwgReadException("Mismatch between reported entry count and entry handles/names read.");
            }

            for (int i = 0; i < _entityHandleReferences.Count; i++)
            {
                var entryHandleReference = _entityHandleReferences[i];
                var name = _names[i];
                if (entryHandleReference.Code != DwgHandleReferenceCode.None)
                {
                    throw new DwgReadException("Incorrect child entry handle code.");
                }

                var entryHandle = ResolveHandleReference(entryHandleReference);
                if (!entryHandle.IsNull)
                {
                    var entry = objectCache.GetObject(reader, entryHandle, allowNull: true);
                    // TODO: check parent handle
                    if (entry != null)
                    {
                        _entries.Add(name, entry);
                    }
                }
            }
        }

        #region IDictionary<string, DwgObject> implementation

        public ICollection<string> Keys => ((IDictionary<string, DwgObject>)_entries).Keys;

        public ICollection<DwgObject> Values => ((IDictionary<string, DwgObject>)_entries).Values;

        public int Count => ((IDictionary<string, DwgObject>)_entries).Count;

        public bool IsReadOnly => ((IDictionary<string, DwgObject>)_entries).IsReadOnly;

        public DwgObject this[string key] { get => ((IDictionary<string, DwgObject>)_entries)[key]; set => ((IDictionary<string, DwgObject>)_entries)[key] = value; }

        public void Add(string key, DwgObject value) => ((IDictionary<string, DwgObject>)_entries).Add(key, value);

        public bool ContainsKey(string key) => ((IDictionary<string, DwgObject>)_entries).ContainsKey(key);

        public bool Remove(string key) => ((IDictionary<string, DwgObject>)_entries).Remove(key);

        public bool TryGetValue(string key, out DwgObject value) => ((IDictionary<string, DwgObject>)_entries).TryGetValue(key, out value);

        public void Add(KeyValuePair<string, DwgObject> item) => ((IDictionary<string, DwgObject>)_entries).Add(item);

        public void Clear() => ((IDictionary<string, DwgObject>)_entries).Clear();

        public bool Contains(KeyValuePair<string, DwgObject> item) => ((IDictionary<string, DwgObject>)_entries).Contains(item);

        public void CopyTo(KeyValuePair<string, DwgObject>[] array, int arrayIndex) => ((IDictionary<string, DwgObject>)_entries).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, DwgObject> item) => ((IDictionary<string, DwgObject>)_entries).Remove(item);

        public IEnumerator<KeyValuePair<string, DwgObject>> GetEnumerator() => ((IDictionary<string, DwgObject>)_entries).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IDictionary<string, DwgObject>)_entries).GetEnumerator();

        #endregion

    }
}
