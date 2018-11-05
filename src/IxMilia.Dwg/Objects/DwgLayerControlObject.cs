using System.Collections;
using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgLayerControlObject : IList<DwgLayer>
    {
        private List<DwgLayer> _layers = new List<DwgLayer>();

        internal override IEnumerable<DwgObject> ChildItems => _layers;

        internal override void PreWrite()
        {
            _layerCount = (short)_layers.Count;
            _layerHandles.Clear();
            for (int i = 0; i < _layers.Count; i++)
            {
                _layerHandles.Add(new DwgHandleReference(DwgHandleReferenceCode.None, _layers[i].Handle.HandleOrOffset));
                _layers[i].LayerControlHandle = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, Handle.HandleOrOffset);
            }
        }

        internal override void PoseParse(BitReader reader, DwgObjectCache objectCache)
        {
            _layers.Clear();
            if (_layerHandles.Count != _layerCount)
            {
                throw new DwgReadException("Mismatch between reported layer count and layer handles read.");
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

            foreach (var layerHandle in _layerHandles)
            {
                if (layerHandle.Code != DwgHandleReferenceCode.None)
                {
                    throw new DwgReadException("Incorrect child layer handle code.");
                }

                var layer = objectCache.GetObject<DwgLayer>(reader, layerHandle.HandleOrOffset);
                if (layer.LayerControlHandle.HandleOrOffset != Handle.HandleOrOffset)
                {
                    throw new DwgReadException("Incorrect layer control object parent handle reference.");
                }

                _layers.Add(layer);
            }
        }

        #region IList<DwgLayer> implementation

        public int Count => _layers.Count;

        public bool IsReadOnly => false;

        public DwgLayer this[int index] { get => _layers[index]; set => _layers[index] = value; }

        public int IndexOf(DwgLayer item) => _layers.IndexOf(item);

        public void Insert(int index, DwgLayer item) => _layers.Insert(index, item);

        public void RemoveAt(int index) => _layers.RemoveAt(index);

        public void Add(DwgLayer item) => _layers.Add(item);

        public void Clear() => _layers.Clear();

        public bool Contains(DwgLayer item) => _layers.Contains(item);

        public void CopyTo(DwgLayer[] array, int arrayIndex) => _layers.CopyTo(array, arrayIndex);

        public bool Remove(DwgLayer item) => _layers.Remove(item);

        public IEnumerator<DwgLayer> GetEnumerator() => _layers.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_layers).GetEnumerator();

        #endregion

    }
}
