using System.Collections;
using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgStyleControlObject : IList<DwgStyle>
    {
        private List<DwgStyle> _styles = new List<DwgStyle>();

        internal override IEnumerable<DwgObject> ChildItems => _styles;

        internal override void PreWrite()
        {
            _styleCount = (short)_styles.Count;
            _styleHandles.Clear();
            for (int i = 0; i < _styles.Count; i++)
            {
                _styleHandles.Add(new DwgHandleReference(DwgHandleReferenceCode.None, _styles[i].Handle.HandleOrOffset));
                _styles[i].StyleControlHandle = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, Handle.HandleOrOffset);
            }
        }

        internal override void PoseParse(BitReader reader, DwgObjectCache objectCache)
        {
            _styles.Clear();
            if (_styleHandles.Count != _styleCount)
            {
                throw new DwgReadException("Mismatch between reported style count and style handles read.");
            }

            if (_nullHandle.Code != DwgHandleReferenceCode.HardPointer)
            {
                throw new DwgReadException("Incorrect object NULL handle code.");
            }

            if (_nullHandle.HandleOrOffset != 0)
            {
                throw new DwgReadException("Incorrect object NULL handle value.");
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

                _styles.Add(style);
            }
        }

        #region IList<DwgStyle> implementation

        public int Count => _styles.Count;

        public bool IsReadOnly => false;

        public DwgStyle this[int index] { get => _styles[index]; set => _styles[index] = value; }

        public int IndexOf(DwgStyle item) => _styles.IndexOf(item);

        public void Insert(int index, DwgStyle item) => _styles.Insert(index, item);

        public void RemoveAt(int index) => _styles.RemoveAt(index);

        public void Add(DwgStyle item) => _styles.Add(item);

        public void Clear() => _styles.Clear();

        public bool Contains(DwgStyle item) => _styles.Contains(item);

        public void CopyTo(DwgStyle[] array, int arrayIndex) => _styles.CopyTo(array, arrayIndex);

        public bool Remove(DwgStyle item) => _styles.Remove(item);

        public IEnumerator<DwgStyle> GetEnumerator() => _styles.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_styles).GetEnumerator();

        #endregion

    }
}
