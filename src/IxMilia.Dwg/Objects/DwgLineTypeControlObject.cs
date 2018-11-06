using System.Collections;
using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgLineTypeControlObject : IList<DwgLineType>
    {
        private List<DwgLineType> _lineTypes = new List<DwgLineType>();

        internal override IEnumerable<DwgObject> ChildItems => _lineTypes;

        internal override void PreWrite()
        {
            _lineTypeCount = (short)_lineTypes.Count;
            _lineTypeHandles.Clear();
            for (int i = 0; i < _lineTypes.Count; i++)
            {
                _lineTypeHandles.Add(new DwgHandleReference(DwgHandleReferenceCode.None, _lineTypes[i].Handle.HandleOrOffset));
                _lineTypes[i].LineTypeControlHandle = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, Handle.HandleOrOffset);
            }
        }

        internal override void PoseParse(BitReader reader, DwgObjectCache objectCache)
        {
            _lineTypes.Clear();
            if (_lineTypeHandles.Count != _lineTypeCount)
            {
                throw new DwgReadException("Mismatch between reported line type count and line type handles read.");
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

            foreach (var lineTypeHandle in _lineTypeHandles)
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

                _lineTypes.Add(lineType);
            }
        }

        #region IList<DwgLineType> implementation

        public int Count => _lineTypes.Count;

        public bool IsReadOnly => false;

        public DwgLineType this[int index] { get => _lineTypes[index]; set => _lineTypes[index] = value; }

        public int IndexOf(DwgLineType item) => _lineTypes.IndexOf(item);

        public void Insert(int index, DwgLineType item) => _lineTypes.Insert(index, item);

        public void RemoveAt(int index) => _lineTypes.RemoveAt(index);

        public void Add(DwgLineType item) => _lineTypes.Add(item);

        public void Clear() => _lineTypes.Clear();

        public bool Contains(DwgLineType item) => _lineTypes.Contains(item);

        public void CopyTo(DwgLineType[] array, int arrayIndex) => _lineTypes.CopyTo(array, arrayIndex);

        public bool Remove(DwgLineType item) => _lineTypes.Remove(item);

        public IEnumerator<DwgLineType> GetEnumerator() => _lineTypes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_lineTypes).GetEnumerator();

        #endregion

    }
}
