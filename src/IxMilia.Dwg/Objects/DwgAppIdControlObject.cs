using System;
using System.Collections;
using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgAppIdControlObject : IDictionary<string, DwgAppId>
    {
        private Dictionary<string, DwgAppId> _appIds = new Dictionary<string, DwgAppId>(StringComparer.OrdinalIgnoreCase);

        internal override IEnumerable<DwgObject> ChildItems => _appIds.Values;

        internal override void OnBeforeObjectWrite(DwgVersionId version)
        {
            base.OnBeforeObjectWrite(version);
            _entityHandleReferences.Clear();
            foreach (var appId in _appIds.Values)
            {
                _entityHandleReferences.Add(appId.MakeHandleReference(DwgHandleReferenceCode.None));
                appId.AppIdControlHandleReference = MakeHandleReference(DwgHandleReferenceCode.HardPointer);
            }
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            _appIds.Clear();
            foreach (var appIdHandleReference in _entityHandleReferences)
            {
                if (appIdHandleReference.Code != DwgHandleReferenceCode.None)
                {
                    throw new DwgReadException("Incorrect child app id handle code.");
                }

                var resolvedHandle = ResolveHandleReference(appIdHandleReference);
                if (!resolvedHandle.IsNull)
                {
                    var appId = objectCache.GetObject<DwgAppId>(reader, resolvedHandle);
                    if (appId.ResolveHandleReference(appId.AppIdControlHandleReference) != Handle)
                    {
                        throw new DwgReadException("Incorrect app id control object parent handle reference.");
                    }

                    _appIds.Add(appId.Name, appId);
                }
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
