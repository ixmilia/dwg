using System;
using System.Collections.Generic;
using System.Linq;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgBlockHeader : DwgObject
    {
        public DwgBlock Block { get; set; }
        public DwgEndBlock EndBlock { get; set; }
        public List<DwgEntity> Entities { get; } = new List<DwgEntity>();

        public const string ModelSpaceBlockName = "*MODEL_SPACE";
        public const string PaperSpaceBlockName = "*PAPER_SPACE";

        public bool IsModelSpaceBlock => string.Compare(Name, ModelSpaceBlockName, StringComparison.OrdinalIgnoreCase) == 0;
        public bool IsPaperSpaceBlock => string.Compare(Name, PaperSpaceBlockName, StringComparison.OrdinalIgnoreCase) == 0;

        public DwgBlockHeader(string name)
            : this()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "Name cannot be null.");
            }

            Name = name;
            Block = new DwgBlock(name);
            EndBlock = new DwgEndBlock();
        }

        public void SetLayer(DwgLayer layer)
        {
            Block.Layer = layer;
            EndBlock.Layer = layer;
        }

        internal override DwgHandleReferenceCode ExpectedNullHandleCode => DwgHandleReferenceCode.SoftOwner;

        internal override IEnumerable<DwgObject> ChildItems
        {
            get
            {
                yield return Block;
                yield return EndBlock;
                foreach (var entity in Entities)
                {
                    yield return entity;
                }
            }
        }

        internal override void OnBeforeObjectWrite(DwgVersionId version)
        {
            base.OnBeforeObjectWrite(version);
            BlockEntityHandleReference = Block.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);
            EndBlockEntityHandleReference = EndBlock.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);
            if (Entities.Count == 0)
            {
                _firstEntityHandleReference = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, 0);
                _lastEntityHandleReference = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, 0);
            }
            else
            {
                var flatList = DwgEntityHelpers.FlattenAndAssignPointersForWrite(Entities);
                foreach (var entity in Entities)
                {
                    AssignEntityMode(entity);
                }

                _firstEntityHandleReference = flatList.First().MakeHandleReference(DwgHandleReferenceCode.HardPointer);
                _lastEntityHandleReference = flatList.Last().MakeHandleReference(DwgHandleReferenceCode.HardPointer);
            }

            // assign subentity references
            AssignEntityMode(Block);
            AssignEntityMode(EndBlock);
            Block.AssignSubentityReference(Handle);
            EndBlock.AssignSubentityReference(Handle);
            foreach (var entity in Entities)
            {
                entity.AssignSubentityReference(Handle);
            }
        }

        private void AssignEntityMode(DwgEntity entity)
        {
            switch (Name.ToUpperInvariant())
            {
                case ModelSpaceBlockName:
                    entity._entityMode = 0b10;
                    break;
                case PaperSpaceBlockName:
                    entity._entityMode = 0b01;
                    break;
                default:
                    entity._entityMode = 0b00;
                    break;
            }
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            // according to the spec, BlockControlHandleReference.Code should be HardPointer (4), but AutoCAD sometimes produces HandleMinus1 (8)

            if (BlockEntityHandleReference.Code != DwgHandleReferenceCode.SoftPointer)
            {
                throw new DwgReadException("Incorrect block entity handle code.");
            }

            if (!IsXRef && !IsOverlaidXref)
            {
                if (_firstEntityHandleReference.Code != DwgHandleReferenceCode.HardPointer)
                {
                    throw new DwgReadException("Incorrect first entity handle code.");
                }

                if (_lastEntityHandleReference.Code != DwgHandleReferenceCode.HardPointer)
                {
                    throw new DwgReadException("Incorrect last entity handle code.");
                }
            }

            if (EndBlockEntityHandleReference.Code != DwgHandleReferenceCode.SoftPointer)
            {
                throw new DwgReadException("Incorrect end block entity handle code.");
            }

            Block = objectCache.GetObject<DwgBlock>(reader, ResolveHandleReference(BlockEntityHandleReference));
            EndBlock = objectCache.GetObject<DwgEndBlock>(reader, ResolveHandleReference(EndBlockEntityHandleReference));
            LoadEntities(reader, objectCache);
        }

        private void LoadEntities(BitReader reader, DwgObjectCache objectCache)
        {
            var isModelSpaceBlock = IsModelSpaceBlock;
            var isPaperSpaceBlock = IsPaperSpaceBlock;

            Entities.Clear();
            var currentEntityHandle = ResolveHandleReference(_firstEntityHandleReference);
            while (!currentEntityHandle.IsNull)
            {
                var obj = objectCache.GetObject(reader, currentEntityHandle, allowNull: true);
                if (obj is DwgEntity entity)
                {
                    Entities.Add(entity);
                    currentEntityHandle = currentEntityHandle.ResolveHandleReference(entity.NextEntityHandle);
                    if (isModelSpaceBlock && entity._entityMode != 0b10)
                    {
                        throw new DwgReadException("Expected entity mode 2 for children of model space block.");
                    }
                    if (isPaperSpaceBlock && entity._entityMode != 0b01)
                    {
                        throw new DwgReadException("Expected entity mode 1 for children of paper space block.");
                    }
                    if (!isModelSpaceBlock && !isPaperSpaceBlock && entity._entityMode != 0b00)
                    {
                        throw new DwgReadException("Expected entity mode 0 for children of regular blocks.");
                    }
                }
                else
                {
                    currentEntityHandle = default(DwgHandle);
                }
            }
        }

        public static DwgBlockHeader CreateBlockRecordWithName(string name, DwgLayer layer)
        {
            var blockHeader = new DwgBlockHeader(name);
            blockHeader.SetLayer(layer);
            return blockHeader;
        }
    }
}
