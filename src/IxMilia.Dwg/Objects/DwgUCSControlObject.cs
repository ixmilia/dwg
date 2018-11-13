using System.Collections;
using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgUCSControlObject : IDictionary<string, DwgUCS>
    {
        private Dictionary<string, DwgUCS> _ucs = new Dictionary<string, DwgUCS>();

        internal override IEnumerable<DwgObject> ChildItems => _ucs.Values;

        internal override void PreWrite()
        {
            foreach (var ucs in _ucs.Values)
            {
                _entityHandles.Add(new DwgHandleReference(DwgHandleReferenceCode.None, ucs.Handle.HandleOrOffset));
                ucs.UCSControlHandle = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, Handle.HandleOrOffset);
            }
        }

        internal override void PoseParse(BitReader reader, DwgObjectCache objectCache)
        {
            _ucs.Clear();
            foreach (var ucsHandle in _entityHandles)
            {
                if (ucsHandle.Code != DwgHandleReferenceCode.None)
                {
                    throw new DwgReadException("Incorrect child UCS handle code.");
                }

                var ucs = objectCache.GetObject<DwgUCS>(reader, ucsHandle.HandleOrOffset);
                if (ucs.UCSControlHandle.HandleOrOffset != Handle.HandleOrOffset)
                {
                    throw new DwgReadException("Incorrect UCS control object parent handle reference.");
                }

                _ucs.Add(ucs.Name, ucs);
            }
        }

        public void Add(DwgUCS ucs) => Add(ucs.Name, ucs);

        #region IDictionary<string, DwgUCS> implementation

        public ICollection<string> Keys => ((IDictionary<string, DwgUCS>)_ucs).Keys;

        public ICollection<DwgUCS> Values => ((IDictionary<string, DwgUCS>)_ucs).Values;

        public int Count => ((IDictionary<string, DwgUCS>)_ucs).Count;

        public bool IsReadOnly => ((IDictionary<string, DwgUCS>)_ucs).IsReadOnly;

        public DwgUCS this[string key] { get => ((IDictionary<string, DwgUCS>)_ucs)[key]; set => ((IDictionary<string, DwgUCS>)_ucs)[key] = value; }

        public void Add(string key, DwgUCS value) => ((IDictionary<string, DwgUCS>)_ucs).Add(key, value);

        public bool ContainsKey(string key) => ((IDictionary<string, DwgUCS>)_ucs).ContainsKey(key);

        public bool Remove(string key) => ((IDictionary<string, DwgUCS>)_ucs).Remove(key);

        public bool TryGetValue(string key, out DwgUCS value) => ((IDictionary<string, DwgUCS>)_ucs).TryGetValue(key, out value);

        public void Add(KeyValuePair<string, DwgUCS> item) => ((IDictionary<string, DwgUCS>)_ucs).Add(item);

        public void Clear() => ((IDictionary<string, DwgUCS>)_ucs).Clear();

        public bool Contains(KeyValuePair<string, DwgUCS> item) => ((IDictionary<string, DwgUCS>)_ucs).Contains(item);

        public void CopyTo(KeyValuePair<string, DwgUCS>[] array, int arrayIndex) => ((IDictionary<string, DwgUCS>)_ucs).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, DwgUCS> item) => ((IDictionary<string, DwgUCS>)_ucs).Remove(item);

        public IEnumerator<KeyValuePair<string, DwgUCS>> GetEnumerator() => ((IDictionary<string, DwgUCS>)_ucs).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IDictionary<string, DwgUCS>)_ucs).GetEnumerator();

        #endregion

    }
}
