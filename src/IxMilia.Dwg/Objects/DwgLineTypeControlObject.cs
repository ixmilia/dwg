using System;
using System.Collections;
using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgLineTypeControlObject : IDictionary<string, DwgLineType>
    {
        private Dictionary<string, DwgLineType> _lineTypes = new Dictionary<string, DwgLineType>(StringComparer.OrdinalIgnoreCase);

        internal override IEnumerable<DwgObject> ChildItems => _lineTypes.Values;

        internal override void OnBeforeObjectWrite()
        {
            foreach (var lineType in _lineTypes.Values)
            {
                _entityHandles.Add(new DwgHandleReference(DwgHandleReferenceCode.None, lineType.Handle.HandleOrOffset));
                lineType.LineTypeControlHandle = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, Handle.HandleOrOffset);
            }
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache)
        {
            _lineTypes.Clear();
            foreach (var lineTypeHandle in _entityHandles)
            {
                if (lineTypeHandle.Code != DwgHandleReferenceCode.None)
                {
                    throw new DwgReadException("Incorrect child line type handle code.");
                }

                var lineType = objectCache.GetObject<DwgLineType>(reader, lineTypeHandle.HandleOrOffset);
                if (!lineType.LineTypeControlHandle.IsEmpty && lineType.LineTypeControlHandle.HandleOrOffset != Handle.HandleOrOffset)
                {
                    throw new DwgReadException("Incorrect line type control object parent handle reference.");
                }

                _lineTypes.Add(lineType.Name, lineType);
            }
        }

        public void Add(DwgLineType lineType) => Add(lineType.Name, lineType);

        #region IDictionary<string, DwgLineType> implementation

        public ICollection<string> Keys => ((IDictionary<string, DwgLineType>)_lineTypes).Keys;

        public ICollection<DwgLineType> Values => ((IDictionary<string, DwgLineType>)_lineTypes).Values;

        public int Count => ((IDictionary<string, DwgLineType>)_lineTypes).Count;

        public bool IsReadOnly => ((IDictionary<string, DwgLineType>)_lineTypes).IsReadOnly;

        public DwgLineType this[string key] { get => ((IDictionary<string, DwgLineType>)_lineTypes)[key]; set => ((IDictionary<string, DwgLineType>)_lineTypes)[key] = value; }

        public void Add(string key, DwgLineType value) => ((IDictionary<string, DwgLineType>)_lineTypes).Add(key, value);

        public bool ContainsKey(string key) => ((IDictionary<string, DwgLineType>)_lineTypes).ContainsKey(key);

        public bool Remove(string key) => ((IDictionary<string, DwgLineType>)_lineTypes).Remove(key);

        public bool TryGetValue(string key, out DwgLineType value) => ((IDictionary<string, DwgLineType>)_lineTypes).TryGetValue(key, out value);

        public void Add(KeyValuePair<string, DwgLineType> item) => ((IDictionary<string, DwgLineType>)_lineTypes).Add(item);

        public void Clear() => ((IDictionary<string, DwgLineType>)_lineTypes).Clear();

        public bool Contains(KeyValuePair<string, DwgLineType> item) => ((IDictionary<string, DwgLineType>)_lineTypes).Contains(item);

        public void CopyTo(KeyValuePair<string, DwgLineType>[] array, int arrayIndex) => ((IDictionary<string, DwgLineType>)_lineTypes).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, DwgLineType> item) => ((IDictionary<string, DwgLineType>)_lineTypes).Remove(item);

        public IEnumerator<KeyValuePair<string, DwgLineType>> GetEnumerator() => ((IDictionary<string, DwgLineType>)_lineTypes).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IDictionary<string, DwgLineType>)_lineTypes).GetEnumerator();

        #endregion

    }
}
