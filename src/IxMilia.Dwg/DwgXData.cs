using System;
using System.Collections.Generic;
using System.IO;
using IxMilia.Dwg.Objects;

namespace IxMilia.Dwg
{
    public class DwgXData : IDictionary<string, IList<DwgXDataItem>>
    {
        private IDictionary<string, IList<DwgXDataItem>> _items = new Dictionary<string, IList<DwgXDataItem>>();

        internal static IDictionary<DwgHandle, IList<DwgXDataItem>> Parse(BitReader reader)
        {
            var xdata = new Dictionary<DwgHandle, IList<DwgXDataItem>>();
            var xdataLength = reader.Read_BS();
            while (xdataLength > 0)
            {
                var applicationHandle = reader.Read_H();
                if (applicationHandle.Code != DwgHandleReferenceCode.SoftOwner)
                {
                    throw new DwgReadException("Incorrect XData application pointer type.");
                }

                var xdataReader = reader.FromOffsetWithBitOffset(reader.Offset, reader.Offset + xdataLength);
                var xdataItems = DwgXDataItem.ParseItems(xdataReader);
                xdata.Add(applicationHandle.AsAbsoluteHandle(), xdataItems);

                var _ = reader.ReadBytes(xdataLength); // fast-forward the main reader
                xdataLength = reader.Read_BS();
            }

            return xdata;
        }

        internal static DwgXData FromMap(BitReader reader, DwgObjectCache objectCache, IDictionary<DwgHandle, IList<DwgXDataItem>> xdataMap)
        {
            var xdata = new DwgXData();
            foreach (var pair in xdataMap)
            {
                var appIdObj = objectCache.GetObject(reader, pair.Key, allowNull: true);
                if (appIdObj == null)
                {
                    throw new DwgReadException($"XData application ID with handle 0x{pair.Key:X2} not found.");
                }

                if (!(appIdObj is DwgAppId appId))
                {
                    throw new DwgReadException($"XData handle expected to point to an application ID, but instead points to {appIdObj.Type}.");
                }

                xdata.Add(appId.Name, pair.Value);
            }

            return xdata;
        }

        internal void Write(BitWriter writer, IDictionary<string, DwgHandle> appIdMap)
        {
            foreach (var pair in _items)
            {
                using (var ms = new MemoryStream())
                {
                    var tempWriter = new BitWriter(ms);
                    foreach (var item in pair.Value)
                    {
                        item.Write(tempWriter);
                    }

                    if (!appIdMap.TryGetValue(pair.Key, out var handle))
                    {
                        throw new InvalidOperationException($"The application with name '{pair.Key}' was not found in the drawing.");
                    }

                    var bytes = tempWriter.AsBytes();
                    writer.Write_BS((short)bytes.Length);
                    writer.Write_H(handle.MakeHandleReference(DwgHandleReferenceCode.SoftOwner));
                    writer.WriteBytes(bytes);
                }
            }

            writer.Write_BS(0);
        }

        #region dictionary implementation

        public ICollection<string> Keys => _items.Keys;

        public ICollection<IList<DwgXDataItem>> Values => _items.Values;

        public int Count => _items.Count;

        public bool IsReadOnly => _items.IsReadOnly;

        public IList<DwgXDataItem> this[string key] { get => _items[key]; set => _items[key] = value; }

        public void Add(string key, IList<DwgXDataItem> value)
        {
            _items.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _items.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return _items.Remove(key);
        }

        public bool TryGetValue(string key, out IList<DwgXDataItem> value)
        {
            return _items.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, IList<DwgXDataItem>> item)
        {
            _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(KeyValuePair<string, IList<DwgXDataItem>> item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, IList<DwgXDataItem>>[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, IList<DwgXDataItem>> item)
        {
            return _items.Remove(item);
        }

        public IEnumerator<KeyValuePair<string, IList<DwgXDataItem>>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)_items).GetEnumerator();
        }

        #endregion
    }
}
