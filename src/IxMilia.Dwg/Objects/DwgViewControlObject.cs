﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgViewControlObject : IDictionary<string, DwgView>
    {
        private Dictionary<string, DwgView> _views = new Dictionary<string, DwgView>(StringComparer.OrdinalIgnoreCase);

        internal override IEnumerable<DwgObject> ChildItems => _views.Values;

        internal override void OnBeforeObjectWrite()
        {
            base.OnBeforeObjectWrite();
            foreach (var view in _views.Values)
            {
                _entityHandleReferences.Add(view.MakeHandleReference(DwgHandleReferenceCode.None));
                view.ViewControlHandleReference = MakeHandleReference(DwgHandleReferenceCode.HardPointer);
            }
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache)
        {
            _views.Clear();
            foreach (var viewHandleReference in _entityHandleReferences)
            {
                if (viewHandleReference.Code != DwgHandleReferenceCode.None)
                {
                    throw new DwgReadException("Incorrect child view handle code.");
                }

                var resolvedHandle = ResolveHandleReference(viewHandleReference);
                if (!resolvedHandle.IsNull)
                {
                    var view = objectCache.GetObject<DwgView>(reader, resolvedHandle);
                    if (view.ResolveHandleReference(view.ViewControlHandleReference) != Handle)
                    {
                        throw new DwgReadException("Incorrect view control object parent handle reference.");
                    }

                    _views.Add(view.Name, view);
                }
            }
        }

        public void Add(DwgView view) => Add(view.Name, view);

        #region IDictionary<string, DwgView> implementation

        public ICollection<string> Keys => ((IDictionary<string, DwgView>)_views).Keys;

        public ICollection<DwgView> Values => ((IDictionary<string, DwgView>)_views).Values;

        public int Count => ((IDictionary<string, DwgView>)_views).Count;

        public bool IsReadOnly => ((IDictionary<string, DwgView>)_views).IsReadOnly;

        public DwgView this[string key] { get => ((IDictionary<string, DwgView>)_views)[key]; set => ((IDictionary<string, DwgView>)_views)[key] = value; }

        public void Add(string key, DwgView value) => ((IDictionary<string, DwgView>)_views).Add(key, value);

        public bool ContainsKey(string key) => ((IDictionary<string, DwgView>)_views).ContainsKey(key);

        public bool Remove(string key) => ((IDictionary<string, DwgView>)_views).Remove(key);

        public bool TryGetValue(string key, out DwgView value) => ((IDictionary<string, DwgView>)_views).TryGetValue(key, out value);

        public void Add(KeyValuePair<string, DwgView> item) => ((IDictionary<string, DwgView>)_views).Add(item);

        public void Clear() => ((IDictionary<string, DwgView>)_views).Clear();

        public bool Contains(KeyValuePair<string, DwgView> item) => ((IDictionary<string, DwgView>)_views).Contains(item);

        public void CopyTo(KeyValuePair<string, DwgView>[] array, int arrayIndex) => ((IDictionary<string, DwgView>)_views).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, DwgView> item) => ((IDictionary<string, DwgView>)_views).Remove(item);

        public IEnumerator<KeyValuePair<string, DwgView>> GetEnumerator() => ((IDictionary<string, DwgView>)_views).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IDictionary<string, DwgView>)_views).GetEnumerator();

        #endregion

    }
}
