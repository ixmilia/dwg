using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgSortEntsTable
    {
        public DwgObject Owner { get; set; }

        public List<DwgObject> SortedEntities { get; set; } = new List<DwgObject>();

        // only exists for object creation during parsing
        internal DwgSortEntsTable()
            : this(null!)
        {
        }

        public DwgSortEntsTable(DwgObject owner)
        {
            SetDefaults();

            Owner = owner;
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            if (_ownerHandleReference.Code != DwgHandleReferenceCode.HardPointer)
            {
                throw new DwgReadException("Incorrect sortents table owner handle code");
            }

            Owner = objectCache.GetObject(reader, ResolveHandleReference(_ownerHandleReference));

            SortedEntities.Clear();
            foreach (var entityHandleReference in _sortHandleReferences)
            {
                if (entityHandleReference.Code != DwgHandleReferenceCode.Declaration)
                {
                    throw new DwgReadException("Incorrect sortents table entity handle code");
                }

                var entity = objectCache.GetObject(reader, ResolveHandleReference(entityHandleReference));
                SortedEntities.Add(entity);
            }
        }

        internal override void OnBeforeObjectWrite(DwgVersionId version)
        {
            _ownerHandleReference = Owner.MakeHandleReference(DwgHandleReferenceCode.HardPointer);
            _sortHandleReferences.Clear();
            _objectHandleReferences.Clear();
            foreach (var entity in SortedEntities)
            {
                _sortHandleReferences.Add(entity.MakeHandleReference(DwgHandleReferenceCode.Declaration));
                _sortHandleReferences.Add(entity.MakeHandleReference(DwgHandleReferenceCode.HardPointer));
            }
        }
    }
}
