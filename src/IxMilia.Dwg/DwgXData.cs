using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using IxMilia.Dwg.Objects;

namespace IxMilia.Dwg
{
    public class DwgXData : IDictionary<string, IList<DwgXDataItem>>, IEquatable<DwgXData>
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

        // equality ----------------------------------------------------------
        public bool Equals(DwgXData? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (Count != other.Count)
            {
                return false;
            }

            // compare key sets
            if (!Keys.OrderBy(k => k).SequenceEqual(other.Keys.OrderBy(k => k), StringComparer.Ordinal))
            {
                return false;
            }

            // deep compare item lists
            foreach (var key in Keys)
            {
                var a = this[key];
                var b = other[key];
                if (a.Count != b.Count)
                {
                    return false;
                }

                for (int i = 0; i < a.Count; i++)
                {
                    if (!XDataItemsEqual(a[i], b[i]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override bool Equals(object? obj) => Equals(obj as DwgXData);

        public override int GetHashCode()
        {
            // combine hashes of keys and contained items; order independent for keys but order dependent for item lists
            unchecked
            {
                int hash = 17;
                foreach (var key in Keys.OrderBy(k => k))
                {
                    hash = hash * 23 + key.GetHashCode();
                    var list = this[key];
                    foreach (var item in list)
                    {
                        hash = hash * 23 + XDataItemHash(item);
                    }
                }

                return hash;
            }
        }

        public static bool operator ==(DwgXData? left, DwgXData? right) => left?.Equals(right) ?? right is null;
        public static bool operator !=(DwgXData? left, DwgXData? right) => !(left == right);

        private static bool XDataItemsEqual(DwgXDataItem a, DwgXDataItem b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a is null || b is null || a.GetType() != b.GetType())
            {
                return false;
            }

            return a switch
            {
                DwgXDataString s1 when b is DwgXDataString s2 => s1.CodePage == s2.CodePage && s1.Value == s2.Value,
                DwgXDataItemList l1 when b is DwgXDataItemList l2 => l1.Count == l2.Count && Enumerable.Range(0, l1.Count).All(i => XDataItemsEqual(l1[i], l2[i])),
                DwgXDataLayerReference lr1 when b is DwgXDataLayerReference lr2 => lr1.Handle == lr2.Handle,
                DwgXDataBinaryChunk bc1 when b is DwgXDataBinaryChunk bc2 => bc1.Data.Length == bc2.Data.Length && bc1.Data.SequenceEqual(bc2.Data),
                DwgXDataEntityReference er1 when b is DwgXDataEntityReference er2 => er1.Handle == er2.Handle,
                DwgXDataRealTriple rt1 when b is DwgXDataRealTriple rt2 => rt1.Value == rt2.Value,
                DwgXDataWorldSpacePosition p1 when b is DwgXDataWorldSpacePosition p2 => p1.Value == p2.Value,
                DwgXDataWorldSpaceDisplacement d1 when b is DwgXDataWorldSpaceDisplacement d2 => d1.Value == d2.Value,
                DwgXDataWorldDirection wd1 when b is DwgXDataWorldDirection wd2 => wd1.Value == wd2.Value,
                DwgXDataReal r1 when b is DwgXDataReal r2 => r1.Value == r2.Value,
                DwgXDataDistance dist1 when b is DwgXDataDistance dist2 => dist1.Value == dist2.Value,
                DwgXDataScaleFactor sf1 when b is DwgXDataScaleFactor sf2 => sf1.Value == sf2.Value,
                DwgXDataShort sh1 when b is DwgXDataShort sh2 => sh1.Value == sh2.Value,
                DwgXDataLong lo1 when b is DwgXDataLong lo2 => lo1.Value == lo2.Value,
                _ => throw new NotSupportedException($"Unexpected item type {a.GetType().Name}"),
            };
        }

        private static int XDataItemHash(DwgXDataItem item)
        {
            unchecked
            {
                int h = 17;
                switch (item)
                {
                    case DwgXDataString s:
                        h = h * 23 + s.CodePage.GetHashCode();
                        h = h * 23 + (s.Value?.GetHashCode() ?? 0);
                        break;
                    case DwgXDataItemList l:
                        foreach (var child in l)
                        {
                            h = h * 23 + XDataItemHash(child);
                        }
                        break;
                    case DwgXDataLayerReference lr:
                        h = h * 23 + lr.Handle.GetHashCode();
                        break;
                    case DwgXDataBinaryChunk bc:
                        foreach (var b in bc.Data)
                        {
                            h = h * 23 + b.GetHashCode();
                        }
                        break;
                    case DwgXDataEntityReference er:
                        h = h * 23 + er.Handle.GetHashCode();
                        break;
                    case DwgXDataRealTriple rt:
                        h = h * 23 + rt.Value.GetHashCode();
                        break;
                    case DwgXDataWorldSpacePosition p:
                        h = h * 23 + p.Value.GetHashCode();
                        break;
                    case DwgXDataWorldSpaceDisplacement d:
                        h = h * 23 + d.Value.GetHashCode();
                        break;
                    case DwgXDataWorldDirection wd:
                        h = h * 23 + wd.Value.GetHashCode();
                        break;
                    case DwgXDataReal r:
                        h = h * 23 + r.Value.GetHashCode();
                        break;
                    case DwgXDataDistance dist:
                        h = h * 23 + dist.Value.GetHashCode();
                        break;
                    case DwgXDataScaleFactor sf:
                        h = h * 23 + sf.Value.GetHashCode();
                        break;
                    case DwgXDataShort sh:
                        h = h * 23 + sh.Value.GetHashCode();
                        break;
                    case DwgXDataLong lo:
                        h = h * 23 + lo.Value.GetHashCode();
                        break;
                }

                return h;
            }
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

        public bool TryGetValue(string key, [NotNullWhen(returnValue: true)]out IList<DwgXDataItem>? value)
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
