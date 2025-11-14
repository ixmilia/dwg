#nullable enable

using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgIdBuffer
    {
        public List<DwgObject> Objects { get; } = new List<DwgObject>();

        internal override IEnumerable<DwgObject> ChildItems
        {
            get
            {
                return Objects;
            }
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            foreach (var handle in _objectHandleReferences)
            {
                if (handle.Code != DwgHandleReferenceCode.HardPointer)
                {
                    throw new DwgReadException("Incorrect group object handle code");
                }

                var obj = objectCache.GetObject(reader, ResolveHandleReference(handle));
                Objects.Add(obj);
            }
        }

        internal override void OnBeforeObjectWrite(DwgVersionId version)
        {
            _objectHandleReferences.Clear();
            foreach (var obj in Objects)
            {
                var handleReference = obj.MakeHandleReference(DwgHandleReferenceCode.HardPointer);
                _objectHandleReferences.Add(handleReference);
            }

            _objectIdCount = _objectHandleReferences.Count;
        }
    }
}
