using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgViewPortEntityHeaderControlObject : IDictionary<string, DwgViewPortEntityHeader>
    {
        private Dictionary<string, DwgViewPortEntityHeader> _viewPortEntityHeaders = new Dictionary<string, DwgViewPortEntityHeader>(StringComparer.OrdinalIgnoreCase);

        internal override IEnumerable<DwgObject> ChildItems => _viewPortEntityHeaders.Values;

        internal override void OnBeforeObjectWrite(DwgVersionId version)
        {
            base.OnBeforeObjectWrite(version);
            foreach (var viewPortEntityHeader in _viewPortEntityHeaders.Values)
            {
                _entityHandleReferences.Add(viewPortEntityHeader.MakeHandleReference(DwgHandleReferenceCode.None));
                viewPortEntityHeader.ViewPortEntityHeaderControlHandleReference = MakeHandleReference(DwgHandleReferenceCode.HardPointer);
            }
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            _viewPortEntityHeaders.Clear();
            foreach (var viewPortEntityHeaderHandle in _entityHandleReferences)
            {
                if (viewPortEntityHeaderHandle.Code != DwgHandleReferenceCode.None)
                {
                    throw new DwgReadException("Incorrect child view port entity header handle code.");
                }

                var resolvedHandle = ResolveHandleReference(viewPortEntityHeaderHandle);
                if (!resolvedHandle.IsNull)
                {
                    var viewPortEntityHeader = objectCache.GetObject<DwgViewPortEntityHeader>(reader, resolvedHandle);
                    if (ResolveHandleReference(viewPortEntityHeader.ViewPortEntityHeaderControlHandleReference) != Handle)
                    {
                        throw new DwgReadException("Incorrect view port entity header control object parent handle reference.");
                    }

                    _viewPortEntityHeaders.Add(viewPortEntityHeader.Name, viewPortEntityHeader);
                }
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

        public bool TryGetValue(string key, [NotNullWhen(returnValue: true)] out DwgViewPortEntityHeader? value) => ((IDictionary<string, DwgViewPortEntityHeader>)_viewPortEntityHeaders).TryGetValue(key, out value);

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
