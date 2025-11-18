using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgLayerIndex
    {
        internal List<DwgHandleReference> _layerHandleReferences = new List<DwgHandleReference>();

        public List<DwgLayer> Layers = new List<DwgLayer>();

        internal override void ParseSpecific(BitReader reader, int objectBitOffsetStart, DwgVersionId version)
        {
            _layerIndices.Clear();
            _layerNames.Clear();
            _reactorHandleReferences.Clear();
            _layerHandleReferences.Clear();

            TimeStamp = Converters.JulianDate(reader.Read_2BL());
            _entryCount = reader.Read_BL();
            for (int i = 0; i < _entryCount; i++)
            {
                _layerIndices.Add(reader.Read_BL());
                _layerNames.Add(reader.Read_T());
            }
            AssertObjectSize(reader, objectBitOffsetStart);

            _parentHandleReference = reader.Read_H();
            for (int i = 0; i < _reactorCount; i++)
            {
                _reactorHandleReferences.Add(reader.Read_H());
            }
            _xDictionaryObjectHandleReference = reader.Read_H();
            for (int i = 0; i < _entryCount; i++)
            {
                _layerHandleReferences.Add(reader.Read_H());
            }
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            Layers.Clear();
            for (int i = 0; i < _entryCount; i++)
            {
                var layerHandle = _layerHandleReferences[i];
                if (layerHandle.Code != DwgHandleReferenceCode.SoftPointer)
                {
                    throw new DwgReadException("Incorrect layer handle reference code");
                }

                var layer = objectCache.GetObject<DwgLayer>(reader, ResolveHandleReference(layerHandle));
                Layers.Add(layer);
            }
        }

        internal override void OnBeforeObjectWrite(DwgVersionId version)
        {
            _entryCount = Layers.Count;
            _layerIndices.Clear();
            _layerNames.Clear();
            _layerHandleReferences.Clear();
            for (int i = 0; i < _entryCount; i++)
            {
                var layer = Layers[i];
                var handleReference = layer.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);
                _layerIndices.Add(i);
                _layerNames.Add(layer.Name);
                _layerHandleReferences.Add(handleReference);
            }
        }

        internal override void WriteSpecific(BitWriter writer, DwgVersionId version)
        {
            writer.Write_2BL(Converters.JulianDate(TimeStamp));
            writer.Write_BL(_entryCount);
            for (int i = 0; i < _entryCount; i++)
            {
                writer.Write_BL(_layerIndices[i]);
                writer.Write_T(_layerNames[i]);
            }

            _objectSize = writer.BitCount;
            writer.Write_H(_parentHandleReference);
            for (int i = 0; i < _reactorCount; i++)
            {
                writer.Write_H(_reactorHandleReferences[i]);
            }
            writer.Write_H(_xDictionaryObjectHandleReference);
            for (int i = 0; i < _entryCount; i++)
            {
                writer.Write_H(_entityHandleReferences[i]);
            }
        }
    }
}
