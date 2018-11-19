using System;
using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgBlockHeader : DwgObject
    {
        public DwgBlock Block { get; set; }
        public DwgEndBlock EndBlock { get; set; }
        public List<DwgEntity> Entities { get; } = new List<DwgEntity>();

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
                _firstEntityHandle = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, Entities[0].Handle.HandleOrOffset);
                _lastEntityHandle = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, Entities[Entities.Count - 1].Handle.HandleOrOffset);
                for (int i = 0; i < Entities.Count; i++)
                {
                    var currentEntity = Entities[i];
                    var previousEntity = i == 0
                        ? null
                        : Entities[i - 1];
                    var nextEntity = i == Entities.Count - 1
                        ? null
                        : Entities[i + 1];
                    currentEntity.PreviousEntityHandle = currentEntity.GetHandleToObject(previousEntity, DwgHandleReferenceCode.HardPointer);
                    currentEntity.NextEntityHandle = currentEntity.GetHandleToObject(nextEntity, DwgHandleReferenceCode.HardPointer);
                }
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
            Entities.Clear();
            var currentEntityHandle = _firstEntityHandle;
            while (currentEntityHandle.HandleOrOffset != 0)
            {
                var obj = objectCache.GetObject(reader, currentEntityHandle.HandleOrOffset, allowNull: true);
                if (obj is DwgEntity entity)
                {
                    Entities.Add(entity);
                    currentEntityHandle = currentEntityHandle.GetNextHandle(entity.NextEntityHandle);
                }
                else
                {
                    currentEntityHandle = default(DwgHandleReference);
                }
            }
        }

        private static DwgBlockHeader GetBlockRecordWithName(string name, DwgLayer layer)
        {
            var block = new DwgBlock(name) { Layer = layer };
            var endBlock = new DwgEndBlock() { Layer = layer };
            return new DwgBlockHeader(name, block, endBlock);
        }

        internal static DwgBlockHeader GetPaperSpaceBlockRecord(DwgLayer layer)
        {
            return GetBlockRecordWithName("*PAPER_SPACE", layer);
        }

        internal static DwgBlockHeader GetModelSpaceBlockRecord(DwgLayer layer)
        {
            return GetBlockRecordWithName("*MODEL_SPACE", layer);
        }
    }
}
