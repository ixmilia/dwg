using IxMilia.Dwg.Objects;
using System;
using System.Collections.Generic;

namespace IxMilia.Dwg
{
    internal class DwgObjectCache
    {
        private IDictionary<DwgHandle, int> _handleToOffset = new Dictionary<DwgHandle, int>();
        private IDictionary<DwgHandle, DwgObject> _handleToObject = new Dictionary<DwgHandle, DwgObject>();
        private List<Tuple<DwgHandle, bool, Action<DwgObject?>>> _lazyResolvers = new List<Tuple<DwgHandle, bool, Action<DwgObject?>>>();
        private DwgVersionId _version;

        public int ObjectCount => _handleToOffset.Count;

        public IList<DwgClassDefinition?> Classes { get; }

        private DwgObjectCache(DwgVersionId version, IList<DwgClassDefinition?> classes)
        {
            _version = version;
            Classes = classes;
        }

        public int GetOffsetFromHandle(DwgHandle handle)
        {
            if (_handleToOffset.TryGetValue(handle, out var offset))
            {
                return offset;
            }

            throw new DwgReadException($"Unable to get offset for object handle {handle}");
        }

        public DwgObject GetObject(BitReader reader, DwgHandle handle)
        {
            var result = GetObject(reader, handle, allowNull: false);
            return result!;
        }

        public DwgObject? GetObject(BitReader reader, DwgHandle handle, bool allowNull = false)
        {
            if (_handleToObject.TryGetValue(handle, out var obj))
            {
                return obj;
            }

            if (_handleToOffset.TryGetValue(handle, out var offset))
            {
                obj = DwgObject.Parse(reader.FromOffset(offset), this, _version);
                if (obj == null && !allowNull)
                {
                    throw new DwgReadException($"Unsupported object from handle {handle} at offset {offset}.");
                }

                if (obj is not null)
                {
                    _handleToObject.Add(handle, obj);
                }

                return obj;
            }

            if (allowNull)
            {
                // handle didn't resolve to anything, but it doesn't matter
                return null;
            }

            throw new DwgReadException($"Object with handle {handle} not found in object map.");
        }

        // TODO: this should be the default
        public void GetObjectLazy(DwgHandle handle, Action<DwgObject?> onObjectResolved, bool allowNull = false)
        {
            _lazyResolvers.Add(Tuple.Create(handle, allowNull, onObjectResolved));
        }

        public T GetObject<T>(BitReader reader, DwgHandle handle) where T: DwgObject
        {
            var result = GetObject<T>(reader, handle, allowNull: false);
            return result!;
        }

        public T? GetObject<T>(BitReader reader, DwgHandle handle, bool allowNull) where T: DwgObject
        {
            var obj = GetObject(reader, handle, allowNull);
            if (obj is null && allowNull)
            {
                return null;
            }

            if (obj is T specific)
            {
                return specific;
            }

            throw new DwgReadException($"Expected object of type {typeof(T)} with handle {handle} but instead found {obj?.GetType().Name ?? "<null>"}.");
        }

        public T? GetObjectOrDefault<T>(BitReader reader, DwgHandle handle) where T: DwgObject
        {
            if (handle.IsNull)
            {
                return null;
            }

            var obj = GetObject(reader, handle, allowNull: true);
            if (obj is T specific)
            {
                return specific;
            }

            return null;
        }

        public void ResolveLazyObjects()
        {
            foreach (var resolverPair in _lazyResolvers)
            {
                var handle = resolverPair.Item1;
                var allowNull = resolverPair.Item2;
                var resolverAction = resolverPair.Item3;
                DwgObject? resolved;
                if (!_handleToObject.TryGetValue(handle, out resolved))
                {
                    if (allowNull)
                    {
                        resolved = null;
                    }
                    else
                    {
                        throw new DwgReadException($"Unable to resolve object with handle {handle}.");
                    }
                }

                resolverAction(resolved);
            }
        }

        public static DwgObjectCache Parse(BitReader reader, DwgVersionId version, IList<DwgClassDefinition?> classes)
        {
            var objectCache = new DwgObjectCache(version, classes);
            var lastHandle = 0u;
            var lastLocation = 0;
            reader.StartCrcCheck();
            var sectionSize = reader.ReadShortBigEndian();
            while (sectionSize != 2)
            {
                var sectionStart = reader.Offset;
                var sectionEnd = sectionStart + sectionSize - 2;
                while (reader.Offset < sectionEnd)
                {
                    // read data
                    var handleOffset = reader.Read_MC(allowNegation: false);
                    var locationOffset = reader.Read_MC();
                    var handle = (uint)((long)lastHandle + handleOffset);
                    var location = lastLocation + locationOffset;
                    objectCache._handleToOffset.Add(new DwgHandle((uint)handle), location);
                    lastHandle = handle;
                    lastLocation = location;
                }

                reader.ValidateCrc(initialValue: DwgHeaderVariables.InitialCrcValue, readCrcAsMsb: true);
                reader.StartCrcCheck();
                sectionSize = reader.ReadShortBigEndian();
            }

            reader.ValidateCrc(initialValue: DwgHeaderVariables.InitialCrcValue, readCrcAsMsb: true);
            return objectCache;
        }
    }
}
