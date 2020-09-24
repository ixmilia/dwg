using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgLineTypeControlObject : IDictionary<string, DwgLineType>
    {
        public const string ByLayerName = "BYLAYER";
        public const string ByBlockName = "BYBLOCK";

        private static bool IsHardCodedname(string name)
        {
            switch (name.ToUpperInvariant())
            {
                case ByLayerName:
                case ByBlockName:
                    return true;
                default:
                    return false;
            }
        }

        private Dictionary<string, DwgLineType> _lineTypes = new Dictionary<string, DwgLineType>(StringComparer.OrdinalIgnoreCase);

        public DwgLineType ByLayer { get => _lineTypes[ByLayerName]; set => _lineTypes[ByLayerName] = value; }

        public DwgLineType ByBlock { get => _lineTypes[ByBlockName]; set => _lineTypes[ByBlockName] = value; }

        public static DwgLineTypeControlObject Create(params DwgLineType[] lineTypes)
        {
            var control = new DwgLineTypeControlObject();
            control.ByLayer = new DwgLineType(ByLayerName);
            control.ByBlock = new DwgLineType(ByBlockName);
            foreach (var lineType in lineTypes)
            {
                control.Add(lineType);
            }

            return control;
        }

        internal override IEnumerable<DwgObject> ChildItems => _lineTypes.Values;

        internal override void OnBeforeObjectWrite()
        {
            _entityHandles.Clear();
            foreach (var lineType in _lineTypes.Values.Where(lt => !IsHardCodedname(lt.Name)))
            {
                _entityHandles.Add(new DwgHandleReference(DwgHandleReferenceCode.None, lineType.Handle.HandleOrOffset));
                lineType.LineTypeControlHandle = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, Handle.HandleOrOffset);
            }

            _byLayerHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftPointer, ByLayer.Handle.HandleOrOffset);
            _byBlockHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftPointer, ByBlock.Handle.HandleOrOffset);
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache)
        {
            _lineTypes.Clear();
            foreach (var lineTypeHandle in _entityHandles)
            {
                if (lineTypeHandle.Code != DwgHandleReferenceCode.None)
                {
                    throw new DwgReadException("Incorrect child line type handle code.");
                }

                var lineType = objectCache.GetObject<DwgLineType>(reader, lineTypeHandle.HandleOrOffset);
                if (!lineType.LineTypeControlHandle.IsEmpty && lineType.LineTypeControlHandle.HandleOrOffset != Handle.HandleOrOffset)
                {
                    throw new DwgReadException("Incorrect line type control object parent handle reference.");
                }

                _lineTypes.Add(lineType.Name, lineType);
            }

            if (_byLayerHandle.Code != DwgHandleReferenceCode.SoftPointer)
            {
                throw new DwgReadException("Incorrect ByLayer line type handle code.");
            }

            if (_byBlockHandle.Code != DwgHandleReferenceCode.SoftPointer)
            {
                throw new DwgReadException("Incorrect ByBlock line type handle code.");
            }

            ByLayer = objectCache.GetObject<DwgLineType>(reader, _byLayerHandle.HandleOrOffset);
            ByBlock = objectCache.GetObject<DwgLineType>(reader, _byBlockHandle.HandleOrOffset);
        }

        public void Add(DwgLineType lineType) => Add(lineType.Name, lineType);

        #region IDictionary<string, DwgLineType> implementation

        public ICollection<string> Keys => ((IDictionary<string, DwgLineType>)_lineTypes).Keys;

        public ICollection<DwgLineType> Values => ((IDictionary<string, DwgLineType>)_lineTypes).Values;

        public int Count => ((IDictionary<string, DwgLineType>)_lineTypes).Count;

        public bool IsReadOnly => ((IDictionary<string, DwgLineType>)_lineTypes).IsReadOnly;

        public DwgLineType this[string key] { get => ((IDictionary<string, DwgLineType>)_lineTypes)[key]; set => ((IDictionary<string, DwgLineType>)_lineTypes)[key] = value; }

        public void Add(string key, DwgLineType value) => ((IDictionary<string, DwgLineType>)_lineTypes).Add(key, value);

        public bool ContainsKey(string key) => ((IDictionary<string, DwgLineType>)_lineTypes).ContainsKey(key);

        public bool Remove(string key) => ((IDictionary<string, DwgLineType>)_lineTypes).Remove(key);

        public bool TryGetValue(string key, out DwgLineType value) => ((IDictionary<string, DwgLineType>)_lineTypes).TryGetValue(key, out value);

        public void Add(KeyValuePair<string, DwgLineType> item) => ((IDictionary<string, DwgLineType>)_lineTypes).Add(item);

        public void Clear() => ((IDictionary<string, DwgLineType>)_lineTypes).Clear();

        public bool Contains(KeyValuePair<string, DwgLineType> item) => ((IDictionary<string, DwgLineType>)_lineTypes).Contains(item);

        public void CopyTo(KeyValuePair<string, DwgLineType>[] array, int arrayIndex) => ((IDictionary<string, DwgLineType>)_lineTypes).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, DwgLineType> item) => ((IDictionary<string, DwgLineType>)_lineTypes).Remove(item);

        public IEnumerator<KeyValuePair<string, DwgLineType>> GetEnumerator() => ((IDictionary<string, DwgLineType>)_lineTypes).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IDictionary<string, DwgLineType>)_lineTypes).GetEnumerator();

        #endregion

    }
}
