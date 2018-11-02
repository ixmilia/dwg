using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    internal partial class DwgLayerControlObject
    {
        public List<DwgLayer> Layers { get; private set; } = new List<DwgLayer>();

        internal override IEnumerable<DwgObject> ChildItems => Layers;

        internal override void PreWrite()
        {
            _layerCount = (short)Layers.Count;
        }

        internal override void PoseParse(BitReader reader, DwgObjectMap objectMap)
        {
            Layers.Clear();
            if (_layerHandles.Count != _layerCount)
            {
                throw new DwgReadException("Mismatch between reported layer count and layer handles read.");
            }

            foreach (var layerHandle in _layerHandles)
            {
                // TODO: read the layer, something like this
                //var layer = ParseSpecific<DwgLayer>(reader.FromOffset(objectMap.GetOffset(layerHandle.HandleOrOffset)), objectMap);
                //Layers.Add(layer);
                Layers.Add(new DwgLayer());
            }
        }
    }
}
