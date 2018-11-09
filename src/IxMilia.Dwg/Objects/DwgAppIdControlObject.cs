using System.Collections;
using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgAppIdControlObject : IDictionary<string, DwgAppId>
    {
        private Dictionary<string, DwgAppId> _appIds = new Dictionary<string, DwgAppId>();

        internal override IEnumerable<DwgObject> ChildItems => _appIds.Values;

        internal override void PreWrite()
        {
            _appIdCount = (short)_appIds.Count;
            _appIdHandles.Clear();
            foreach (var appId in _appIds.Values)
            {
                _appIdHandles.Add(new DwgHandleReference(DwgHandleReferenceCode.None, appId.Handle.HandleOrOffset));
                appId.AppIdControlHandle = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, Handle.HandleOrOffset);
            }
        }

        internal override void PoseParse(BitReader reader, DwgObjectCache objectCache)
        {
            base.PoseParse(reader, objectCache);
            _appIds.Clear();
            if (_appIdHandles.Count != _appIdCount)
            {
                throw new DwgReadException("Mismatch between reported app id count and app id handles read.");
            }

            if (_nullHandle.Code != DwgHandleReferenceCode.HardPointer)
            {
                throw new DwgReadException("Incorrect object NULL handle code.");
            }

            if (_nullHandle.HandleOrOffset != 0)
            {
                throw new DwgReadException("Incorrect object NULL handle value.");
            }

            if (_xDictionaryObjectHandle.Code != DwgHandleReferenceCode.SoftPointer)
            {
                throw new DwgReadException("Incorrect XDictionary object handle code.");
            }

            foreach (var appIdHandle in _appIdHandles)
            {
                if (appIdHandle.Code != DwgHandleReferenceCode.None)
                {
                    throw new DwgReadException("Incorrect child app id handle code.");
                }

                var appId = objectCache.GetObject<DwgAppId>(reader, appIdHandle.HandleOrOffset);
                if (appId.AppIdControlHandle.HandleOrOffset != Handle.HandleOrOffset)
                {
                    throw new DwgReadException("Incorrect app id control object parent handle reference.");
                }

                _appIds.Add(appId.Name, appId);
            }
        }

        public void Add(DwgAppId appId) => Add(appId.Name, appId);

        #region IDictionary<string, DwgAppId> implementation

        public ICollection<string> Keys => ((IDictionary<string, DwgAppId>)_appIds).Keys;

        public ICollection<DwgAppId> Values => ((IDictionary<string, DwgAppId>)_appIds).Values;

        public int Count => ((IDictionary<string, DwgAppId>)_appIds).Count;

        public bool IsReadOnly => ((IDictionary<string, DwgAppId>)_appIds).IsReadOnly;

        public DwgAppId this[string key] { get => ((IDictionary<string, DwgAppId>)_appIds)[key]; set => ((IDictionary<string, DwgAppId>)_appIds)[key] = value; }

        public void Add(string key, DwgAppId value) => ((IDictionary<string, DwgAppId>)_appIds).Add(key, value);

        public bool ContainsKey(string key) => ((IDictionary<string, DwgAppId>)_appIds).ContainsKey(key);

        public bool Remove(string key) => ((IDictionary<string, DwgAppId>)_appIds).Remove(key);

        public bool TryGetValue(string key, out DwgAppId value) => ((IDictionary<string, DwgAppId>)_appIds).TryGetValue(key, out value);

        public void Add(KeyValuePair<string, DwgAppId> item) => ((IDictionary<string, DwgAppId>)_appIds).Add(item);

        public void Clear() => ((IDictionary<string, DwgAppId>)_appIds).Clear();

        public bool Contains(KeyValuePair<string, DwgAppId> item) => ((IDictionary<string, DwgAppId>)_appIds).Contains(item);

        public void CopyTo(KeyValuePair<string, DwgAppId>[] array, int arrayIndex) => ((IDictionary<string, DwgAppId>)_appIds).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, DwgAppId> item) => ((IDictionary<string, DwgAppId>)_appIds).Remove(item);

        public IEnumerator<KeyValuePair<string, DwgAppId>> GetEnumerator() => ((IDictionary<string, DwgAppId>)_appIds).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IDictionary<string, DwgAppId>)_appIds).GetEnumerator();

        #endregion

    }
}
