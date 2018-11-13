using System.Collections;
using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgViewPortEntityHeaderControlObject : IDictionary<string, DwgViewPortEntityHeader>
    {
        private Dictionary<string, DwgViewPortEntityHeader> _viewPortEntityHeaders = new Dictionary<string, DwgViewPortEntityHeader>();

        internal override IEnumerable<DwgObject> ChildItems => _viewPortEntityHeaders.Values;

        internal override void PreWrite()
        {
            foreach (var viewPortEntityHeader in _viewPortEntityHeaders.Values)
            {
                _entityHandles.Add(new DwgHandleReference(DwgHandleReferenceCode.None, viewPortEntityHeader.Handle.HandleOrOffset));
                viewPortEntityHeader.ViewPortEntityHeaderControlHandle = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, Handle.HandleOrOffset);
            }
        }

        internal override void PoseParse(BitReader reader, DwgObjectCache objectCache)
        {
            _viewPortEntityHeaders.Clear();
            foreach (var viewPortEntityHeaderHandle in _entityHandles)
            {
                if (viewPortEntityHeaderHandle.Code != DwgHandleReferenceCode.None)
                {
                    throw new DwgReadException("Incorrect child view port entity header handle code.");
                }

                var viewPortEntityHeader = objectCache.GetObject<DwgViewPortEntityHeader>(reader, viewPortEntityHeaderHandle.HandleOrOffset);
                if (viewPortEntityHeader.ViewPortEntityHeaderControlHandle.HandleOrOffset != Handle.HandleOrOffset)
                {
                    throw new DwgReadException("Incorrect view port entity header control object parent handle reference.");
                }

                _viewPortEntityHeaders.Add(viewPortEntityHeader.Name, viewPortEntityHeader);
            }
        }

        public void Add(DwgViewPortEntityHeader viewPortEntityHeader) => Add(viewPortEntityHeader.Name, viewPortEntityHeader);

        #region IDictionary<string, DwgViewPortEntityHeader> implementation

        public ICollection<string> Keys => ((IDictionary<string, DwgViewPortEntityHeader>)_viewPortEntityHeaders).Keys;

        public ICollection<DwgViewPortEntityHeader> Values => ((IDictionary<string, DwgViewPortEntityHeader>)_viewPortEntityHeaders).Values;

        public int Count => ((IDictionary<string, DwgViewPortEntityHeader>)_viewPortEntityHeaders).Count;

        public bool IsReadOnly => ((IDictionary<string, DwgViewPortEntityHeader>)_viewPortEntityHeaders).IsReadOnly;

        public DwgViewPortEntityHeader this[string key] { get => ((IDictionary<string, DwgViewPortEntityHeader>)_viewPortEntityHeaders)[key]; set => ((IDictionary<string, DwgViewPortEntityHeader>)_viewPortEntityHeaders)[key] = value; }

        public void Add(string key, DwgViewPortEntityHeader value) => ((IDictionary<string, DwgViewPortEntityHeader>)_viewPortEntityHeaders).Add(key, value);

        public bool ContainsKey(string key) => ((IDictionary<string, DwgViewPortEntityHeader>)_viewPortEntityHeaders).ContainsKey(key);

        public bool Remove(string key) => ((IDictionary<string, DwgViewPortEntityHeader>)_viewPortEntityHeaders).Remove(key);

        public bool TryGetValue(string key, out DwgViewPortEntityHeader value) => ((IDictionary<string, DwgViewPortEntityHeader>)_viewPortEntityHeaders).TryGetValue(key, out value);

        public void Add(KeyValuePair<string, DwgViewPortEntityHeader> item) => ((IDictionary<string, DwgViewPortEntityHeader>)_viewPortEntityHeaders).Add(item);

        public void Clear() => ((IDictionary<string, DwgViewPortEntityHeader>)_viewPortEntityHeaders).Clear();

        public bool Contains(KeyValuePair<string, DwgViewPortEntityHeader> item) => ((IDictionary<string, DwgViewPortEntityHeader>)_viewPortEntityHeaders).Contains(item);

        public void CopyTo(KeyValuePair<string, DwgViewPortEntityHeader>[] array, int arrayIndex) => ((IDictionary<string, DwgViewPortEntityHeader>)_viewPortEntityHeaders).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, DwgViewPortEntityHeader> item) => ((IDictionary<string, DwgViewPortEntityHeader>)_viewPortEntityHeaders).Remove(item);

        public IEnumerator<KeyValuePair<string, DwgViewPortEntityHeader>> GetEnumerator() => ((IDictionary<string, DwgViewPortEntityHeader>)_viewPortEntityHeaders).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IDictionary<string, DwgViewPortEntityHeader>)_viewPortEntityHeaders).GetEnumerator();

        #endregion

    }
}
