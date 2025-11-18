using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgGroup
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
            foreach (var handle in _handles)
            {
                if (handle.Code != DwgHandleReferenceCode.SoftOwner)
                {
                    throw new DwgReadException("Incorrect group object handle code");
                }

                var obj = objectCache.GetObject(reader, ResolveHandleReference(handle));
                Objects.Add(obj);
            }
        }

        internal override void OnBeforeObjectWrite(DwgVersionId version)
        {
            _handles.Clear();
            foreach (var obj in Objects)
            {
                var handleReference = obj.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
                _handles.Add(handleReference);
            }

            _handleCount = _handles.Count;
        }
    }
}
