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

        public DwgBlockHeader(string name, DwgBlock block, DwgEndBlock endBlock)
            : this()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "Name cannot be null.");
            }

            Name = name;
            Block = block;
            EndBlock = endBlock;
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

        internal override void OnBeforeObjectWrite()
        {
            BlockEntityHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftPointer, Block.Handle.HandleOrOffset);
            EndBlockEntityHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftPointer, EndBlock.Handle.HandleOrOffset);
            if (Entities.Count == 0)
            {
                _firstEntityHandle = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, 0);
                _lastEntityHandle = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, 0);
            }
            else
            {
                var flatList = DwgEntityHelpers.FlattenAndAssignPointersForWrite(Entities);
                foreach (var entity in Entities)
                {
                    AssignEntityMode(entity);
                }

                _firstEntityHandle = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, flatList.First().Handle.HandleOrOffset);
                _lastEntityHandle = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, flatList.Last().Handle.HandleOrOffset);
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

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache)
        {
            if (BlockControlHandle.Code != DwgHandleReferenceCode.HardPointer)
            {
                throw new DwgReadException("Incorrect block header control object parent handle code.");
            }

            if (BlockEntityHandle.Code != DwgHandleReferenceCode.SoftPointer)
            {
                throw new DwgReadException("Incorrect block entity handle code.");
            }

            if (!IsXRef && !IsOverlaidXref)
            {
                if (_firstEntityHandle.Code != DwgHandleReferenceCode.HardPointer)
                {
                    throw new DwgReadException("Incorrect first entity handle code.");
                }

                if (_lastEntityHandle.Code != DwgHandleReferenceCode.HardPointer)
                {
                    throw new DwgReadException("Incorrect last entity handle code.");
                }
            }

            if (EndBlockEntityHandle.Code != DwgHandleReferenceCode.SoftPointer)
            {
                throw new DwgReadException("Incorrect end block entity handle code.");
            }

            Block = objectCache.GetObject<DwgBlock>(reader, BlockEntityHandle.HandleOrOffset);
            EndBlock = objectCache.GetObject<DwgEndBlock>(reader, EndBlockEntityHandle.HandleOrOffset);
            LoadEntities(reader, objectCache);
        }

        private void LoadEntities(BitReader reader, DwgObjectCache objectCache)
        {
            var isModelSpaceBlock = IsModelSpaceBlock;
            var isPaperSpaceBlock = IsPaperSpaceBlock;

            Entities.Clear();
            var currentEntityHandle = _firstEntityHandle;
            while (currentEntityHandle.HandleOrOffset != 0)
            {
                var obj = objectCache.GetObject(reader, currentEntityHandle.HandleOrOffset, allowNull: true);
                if (obj is DwgEntity entity)
                {
                    Entities.Add(entity);
                    currentEntityHandle = currentEntityHandle.GetNextHandle(entity.NextEntityHandle);
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
                    currentEntityHandle = default(DwgHandleReference);
                }
            }
        }

        internal static DwgBlockHeader GetBlockRecordWithName(string name, DwgLayer layer)
        {
            var block = new DwgBlock(name) { Layer = layer };
            var endBlock = new DwgEndBlock() { Layer = layer };
            return new DwgBlockHeader(name, block, endBlock);
        }
    }
}
