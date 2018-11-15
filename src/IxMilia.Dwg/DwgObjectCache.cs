using IxMilia.Dwg.Objects;
using System.Collections.Generic;

namespace IxMilia.Dwg
{
    internal class DwgObjectCache
    {
        private IDictionary<int, int> _handleToOffset = new Dictionary<int, int>();
        private IDictionary<int, DwgObject> _handleToObject = new Dictionary<int, DwgObject>();
        private DwgVersionId _version;

        private DwgObjectCache(DwgVersionId version)
        {
            _version = version;
        }

        public DwgObject GetObject(BitReader reader, int handle, bool allowNull = false)
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

                _handleToObject.Add(handle, obj);
                return obj;
            }

            throw new DwgReadException($"Object with handle {handle} not found in object map.");
        }

        public T GetObject<T>(BitReader reader, int handle) where T: DwgObject
        {
            var obj = GetObject(reader, handle);
            if (obj is T specific)
            {
                return specific;
            }

            throw new DwgReadException($"Expected object of type {typeof(T)} with handle {handle} but instead found {obj.GetType().Name}.");
        }

        public T GetObjectOrDefault<T>(BitReader reader, int handle) where T: DwgObject
        {
            if (handle == 0)
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

        public static DwgObjectCache Parse(BitReader reader, DwgVersionId version)
        {
            var objectCache = new DwgObjectCache(version);
            var lastHandle = 0;
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
                    var handle = lastHandle + handleOffset;
                    var location = lastLocation + locationOffset;
                    objectCache._handleToOffset.Add(handle, location);
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
