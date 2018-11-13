using System.Collections;
using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgStyleControlObject : IDictionary<string, DwgStyle>
    {
        private Dictionary<string, DwgStyle> _styles = new Dictionary<string, DwgStyle>();

        internal override IEnumerable<DwgObject> ChildItems => _styles.Values;

        internal override void PreWrite()
        {
            _styleCount = (short)_styles.Count;
            _styleHandles.Clear();
            foreach (var style in _styles.Values)
            {
                _styleHandles.Add(new DwgHandleReference(DwgHandleReferenceCode.None, style.Handle.HandleOrOffset));
                style.StyleControlHandle = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, Handle.HandleOrOffset);
            }
        }

        internal override void PoseParse(BitReader reader, DwgObjectCache objectCache)
        {
            base.PoseParse(reader, objectCache);
            _styles.Clear();
            if (_styleHandles.Count != _styleCount)
            {
                throw new DwgReadException("Mismatch between reported style count and style handles read.");
            }

            if (_xDictionaryObjectHandle.Code != DwgHandleReferenceCode.SoftPointer)
            {
                throw new DwgReadException("Incorrect XDictionary object handle code.");
            }

            foreach (var styleHandle in _styleHandles)
            {
                if (styleHandle.Code != DwgHandleReferenceCode.None)
                {
                    throw new DwgReadException("Incorrect child style handle code.");
                }

                var style = objectCache.GetObject<DwgStyle>(reader, styleHandle.HandleOrOffset);
                if (style.StyleControlHandle.HandleOrOffset != Handle.HandleOrOffset)
                {
                    throw new DwgReadException("Incorrect style control object parent handle reference.");
                }

                _styles.Add(style.Name, style);
            }
        }

        public void Add(DwgStyle style) => Add(style.Name, style);

        #region IDictionary<string, DwgStyle> implementation

        public ICollection<string> Keys => ((IDictionary<string, DwgStyle>)_styles).Keys;

        public ICollection<DwgStyle> Values => ((IDictionary<string, DwgStyle>)_styles).Values;

        public int Count => ((IDictionary<string, DwgStyle>)_styles).Count;

        public bool IsReadOnly => ((IDictionary<string, DwgStyle>)_styles).IsReadOnly;

        public DwgStyle this[string key] { get => ((IDictionary<string, DwgStyle>)_styles)[key]; set => ((IDictionary<string, DwgStyle>)_styles)[key] = value; }

        public void Add(string key, DwgStyle value) => ((IDictionary<string, DwgStyle>)_styles).Add(key, value);

        public bool ContainsKey(string key) => ((IDictionary<string, DwgStyle>)_styles).ContainsKey(key);

        public bool Remove(string key) => ((IDictionary<string, DwgStyle>)_styles).Remove(key);

        public bool TryGetValue(string key, out DwgStyle value) => ((IDictionary<string, DwgStyle>)_styles).TryGetValue(key, out value);

        public void Add(KeyValuePair<string, DwgStyle> item) => ((IDictionary<string, DwgStyle>)_styles).Add(item);

        public void Clear() => ((IDictionary<string, DwgStyle>)_styles).Clear();

        public bool Contains(KeyValuePair<string, DwgStyle> item) => ((IDictionary<string, DwgStyle>)_styles).Contains(item);

        public void CopyTo(KeyValuePair<string, DwgStyle>[] array, int arrayIndex) => ((IDictionary<string, DwgStyle>)_styles).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, DwgStyle> item) => ((IDictionary<string, DwgStyle>)_styles).Remove(item);

        public IEnumerator<KeyValuePair<string, DwgStyle>> GetEnumerator() => ((IDictionary<string, DwgStyle>)_styles).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IDictionary<string, DwgStyle>)_styles).GetEnumerator();

        #endregion

    }
}
