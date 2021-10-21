using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgBlockControlObject : IDictionary<string, DwgBlockHeader>
    {
        private static bool IsHardCodedName(string name)
        {
            switch (name.ToUpperInvariant())
            {
                case DwgBlockHeader.ModelSpaceBlockName:
                case DwgBlockHeader.PaperSpaceBlockName:
                    return true;
                default:
                    return false;
            }
        }

        private Dictionary<string, DwgBlockHeader> _blockHeaders = new Dictionary<string, DwgBlockHeader>(StringComparer.OrdinalIgnoreCase);

        public DwgBlockHeader ModelSpace { get => _blockHeaders[DwgBlockHeader.ModelSpaceBlockName]; set => _blockHeaders[DwgBlockHeader.ModelSpaceBlockName] = value; }

        public DwgBlockHeader PaperSpace { get => _blockHeaders[DwgBlockHeader.PaperSpaceBlockName]; set => _blockHeaders[DwgBlockHeader.PaperSpaceBlockName] = value; }

        public static DwgBlockControlObject Create(DwgLayer layer)
        {
            var control = new DwgBlockControlObject();
            control.ModelSpace = DwgBlockHeader.CreateBlockRecordWithName(DwgBlockHeader.ModelSpaceBlockName, layer);
            control.PaperSpace = DwgBlockHeader.CreateBlockRecordWithName(DwgBlockHeader.PaperSpaceBlockName, layer);
            return control;
        }

        internal override IEnumerable<DwgObject> ChildItems => _blockHeaders.Values;

        internal override void AssignHandles(DwgObjectMap objectMap)
        {
            base.AssignHandles(objectMap);
            foreach (var blockHeader in _blockHeaders.Values)
            {
                blockHeader.BlockControlHandleReference = MakeHandleReference(DwgHandleReferenceCode.HardPointer);
            }
        }

        internal override void OnBeforeObjectWrite(DwgVersionId version)
        {
            base.OnBeforeObjectWrite(version);
            _entityHandleReferences.Clear();
            foreach (var blockHeader in _blockHeaders.Values.Where(b => !IsHardCodedName(b.Name)))
            {
                _entityHandleReferences.Add(blockHeader.MakeHandleReference(DwgHandleReferenceCode.None));
                blockHeader.BlockControlHandleReference = MakeHandleReference(DwgHandleReferenceCode.HardPointer);
            }

            _modelSpaceBlockHeaderHandleReference = ModelSpace.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);
            _paperSpaceBlockHeaderHandleReference = PaperSpace.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            _blockHeaders.Clear();
            foreach (var blockHeaderHandle in _entityHandleReferences)
            {
                if (blockHeaderHandle.Code != DwgHandleReferenceCode.None)
                {
                    throw new DwgReadException("Incorrect child block header handle code.");
                }

                var resolvedHandle = ResolveHandleReference(blockHeaderHandle);
                if (!resolvedHandle.IsNull)
                {
                    var blockHeader = objectCache.GetObject<DwgBlockHeader>(reader, resolvedHandle);
                    if (!blockHeader.BlockControlHandleReference.IsEmpty && ResolveHandleReference(blockHeader.BlockControlHandleReference) != Handle)
                    {
                        throw new DwgReadException("Incorrect block header control object parent handle reference.");
                    }

                    _blockHeaders.Add(blockHeader.Name, blockHeader);
                }
            }

            if (_modelSpaceBlockHeaderHandleReference.Code != DwgHandleReferenceCode.SoftPointer)
            {
                throw new DwgReadException("Incorrect model space block header handle code.");
            }

            if (_paperSpaceBlockHeaderHandleReference.Code != DwgHandleReferenceCode.SoftPointer)
            {
                throw new DwgReadException("Incorrect paper space block header handle code.");
            }

            ModelSpace = objectCache.GetObject<DwgBlockHeader>(reader, ResolveHandleReference(_modelSpaceBlockHeaderHandleReference));
            PaperSpace = objectCache.GetObject<DwgBlockHeader>(reader, ResolveHandleReference(_paperSpaceBlockHeaderHandleReference));
        }

        public void Add(DwgBlockHeader blockHeader) => Add(blockHeader.Name, blockHeader);

        #region IDictionary<string, DwgBlockHeader> implementation

        public ICollection<string> Keys => ((IDictionary<string, DwgBlockHeader>)_blockHeaders).Keys;

        public ICollection<DwgBlockHeader> Values => ((IDictionary<string, DwgBlockHeader>)_blockHeaders).Values;

        public int Count => ((IDictionary<string, DwgBlockHeader>)_blockHeaders).Count;

        public bool IsReadOnly => ((IDictionary<string, DwgBlockHeader>)_blockHeaders).IsReadOnly;

        public DwgBlockHeader this[string key] { get => ((IDictionary<string, DwgBlockHeader>)_blockHeaders)[key]; set => ((IDictionary<string, DwgBlockHeader>)_blockHeaders)[key] = value; }

        public void Add(string key, DwgBlockHeader value) => ((IDictionary<string, DwgBlockHeader>)_blockHeaders).Add(key, value);

        public bool ContainsKey(string key) => ((IDictionary<string, DwgBlockHeader>)_blockHeaders).ContainsKey(key);

        public bool Remove(string key) => ((IDictionary<string, DwgBlockHeader>)_blockHeaders).Remove(key);

        public bool TryGetValue(string key, out DwgBlockHeader value) => ((IDictionary<string, DwgBlockHeader>)_blockHeaders).TryGetValue(key, out value);

        public void Add(KeyValuePair<string, DwgBlockHeader> item) => ((IDictionary<string, DwgBlockHeader>)_blockHeaders).Add(item);

        public void Clear() => ((IDictionary<string, DwgBlockHeader>)_blockHeaders).Clear();

        public bool Contains(KeyValuePair<string, DwgBlockHeader> item) => ((IDictionary<string, DwgBlockHeader>)_blockHeaders).Contains(item);

        public void CopyTo(KeyValuePair<string, DwgBlockHeader>[] array, int arrayIndex) => ((IDictionary<string, DwgBlockHeader>)_blockHeaders).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, DwgBlockHeader> item) => ((IDictionary<string, DwgBlockHeader>)_blockHeaders).Remove(item);

        public IEnumerator<KeyValuePair<string, DwgBlockHeader>> GetEnumerator() => ((IDictionary<string, DwgBlockHeader>)_blockHeaders).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IDictionary<string, DwgBlockHeader>)_blockHeaders).GetEnumerator();

        #endregion

    }
}
