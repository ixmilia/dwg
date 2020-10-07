using System;
using System.Collections;
using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgDimStyleControlObject : IDictionary<string, DwgDimStyle>
    {
        private Dictionary<string, DwgDimStyle> _styles = new Dictionary<string, DwgDimStyle>(StringComparer.OrdinalIgnoreCase);

        internal override IEnumerable<DwgObject> ChildItems => _styles.Values;

        internal override void OnBeforeObjectWrite()
        {
            base.OnBeforeObjectWrite();
            foreach (var style in _styles.Values)
            {
                _entityHandleReferences.Add(style.MakeHandleReference(DwgHandleReferenceCode.None));
                style.DimStyleControlHandleReference = MakeHandleReference(DwgHandleReferenceCode.HardPointer);
            }
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache)
        {
            _styles.Clear();
            foreach (var styleHandleReference in _entityHandleReferences)
            {
                if (styleHandleReference.Code != DwgHandleReferenceCode.None)
                {
                    throw new DwgReadException("Incorrect child style handle code.");
                }

                var style = objectCache.GetObject<DwgDimStyle>(reader, ResolveHandleReference(styleHandleReference));
                if (style.ResolveHandleReference(style.DimStyleControlHandleReference) != Handle)
                {
                    throw new DwgReadException("Incorrect style control object parent handle reference.");
                }

                _styles.Add(style.Name, style);
            }
        }

        public void Add(DwgDimStyle style) => Add(style.Name, style);

        #region IDictionary<string, DwgDimStyle> implementation

        public ICollection<string> Keys => ((IDictionary<string, DwgDimStyle>)_styles).Keys;

        public ICollection<DwgDimStyle> Values => ((IDictionary<string, DwgDimStyle>)_styles).Values;

        public int Count => ((IDictionary<string, DwgDimStyle>)_styles).Count;

        public bool IsReadOnly => ((IDictionary<string, DwgDimStyle>)_styles).IsReadOnly;

        public DwgDimStyle this[string key] { get => ((IDictionary<string, DwgDimStyle>)_styles)[key]; set => ((IDictionary<string, DwgDimStyle>)_styles)[key] = value; }

        public void Add(string key, DwgDimStyle value) => ((IDictionary<string, DwgDimStyle>)_styles).Add(key, value);

        public bool ContainsKey(string key) => ((IDictionary<string, DwgDimStyle>)_styles).ContainsKey(key);

        public bool Remove(string key) => ((IDictionary<string, DwgDimStyle>)_styles).Remove(key);

        public bool TryGetValue(string key, out DwgDimStyle value) => ((IDictionary<string, DwgDimStyle>)_styles).TryGetValue(key, out value);

        public void Add(KeyValuePair<string, DwgDimStyle> item) => ((IDictionary<string, DwgDimStyle>)_styles).Add(item);

        public void Clear() => ((IDictionary<string, DwgDimStyle>)_styles).Clear();

        public bool Contains(KeyValuePair<string, DwgDimStyle> item) => ((IDictionary<string, DwgDimStyle>)_styles).Contains(item);

        public void CopyTo(KeyValuePair<string, DwgDimStyle>[] array, int arrayIndex) => ((IDictionary<string, DwgDimStyle>)_styles).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, DwgDimStyle> item) => ((IDictionary<string, DwgDimStyle>)_styles).Remove(item);

        public IEnumerator<KeyValuePair<string, DwgDimStyle>> GetEnumerator() => ((IDictionary<string, DwgDimStyle>)_styles).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IDictionary<string, DwgDimStyle>)_styles).GetEnumerator();

        #endregion

    }
}
