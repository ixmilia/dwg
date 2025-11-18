namespace IxMilia.Dwg.Objects
{
    public partial class DwgSpatialFilter
    {
        public DwgObject? Parent { get; set; }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            if (InverseBlockTransformationMatrix.Count != 12)
            {
                throw new DwgReadException($"Expected exactly 12 values in {nameof(InverseBlockTransformationMatrix)}");
            }

            if (ClipBoundaryTransformationMatrix.Count != 12)
            {
                throw new DwgReadException($"Expected exactly 12 values in {nameof(ClipBoundaryTransformationMatrix)}");
            }

            Parent = objectCache.GetObject(reader, ResolveHandleReference(_parentHandleReference), allowNull: true);
        }

        internal override void OnBeforeObjectWrite(DwgVersionId version)
        {
            if (InverseBlockTransformationMatrix.Count != 12)
            {
                throw new DwgReadException($"Expected exactly 12 values in {nameof(InverseBlockTransformationMatrix)}");
            }

            if (ClipBoundaryTransformationMatrix.Count != 12)
            {
                throw new DwgReadException($"Expected exactly 12 values in {nameof(ClipBoundaryTransformationMatrix)}");
            }

            _pointCount = ClippingPoints.Count;
            _parentHandleReference = Parent is { }
                ? Parent.MakeHandleReference(DwgHandleReferenceCode.HardPointer)
                : DwgHandleReference.Empty;
        }
    }
}
