﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgLayerControlObject : IDictionary<string, DwgLayer>
    {
        private Dictionary<string, DwgLayer> _layers = new Dictionary<string, DwgLayer>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<DwgHandle, DwgLayer> _layersFromHandle = new Dictionary<DwgHandle, DwgLayer>();

        internal override IEnumerable<DwgObject> ChildItems => _layers.Values;

        internal DwgLayer LayerFromHandle(DwgHandle handle)
        {
            if (_layersFromHandle.TryGetValue(handle, out var layer))
            {
                return layer;
            }

            return null;
        }

        internal override void OnBeforeObjectWrite(DwgVersionId version)
        {
            base.OnBeforeObjectWrite(version);
            foreach (var layer in _layers.Values)
            {
                _entityHandleReferences.Add(layer.MakeHandleReference(DwgHandleReferenceCode.None));
                layer.LayerControlHandleReference = MakeHandleReference(DwgHandleReferenceCode.HardPointer);
            }
        }

        internal override void OnAfterObjectRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            _layers.Clear();
            _layersFromHandle.Clear();
            foreach (var layerHandleReference in _entityHandleReferences)
            {
                if (layerHandleReference.Code != DwgHandleReferenceCode.None)
                {
                    throw new DwgReadException("Incorrect child layer handle code.");
                }

                var resolvedHandle = ResolveHandleReference(layerHandleReference);
                if (!resolvedHandle.IsNull)
                {
                    var layer = objectCache.GetObject<DwgLayer>(reader, resolvedHandle);
                    if (layer.ResolveHandleReference(layer.LayerControlHandleReference) != Handle)
                    {
                        throw new DwgReadException("Incorrect layer control object parent handle reference.");
                    }

                    _layers.Add(layer.Name, layer);
                    _layersFromHandle.Add(layer.Handle, layer);
                }
            }
        }

        public void Add(DwgLayer layer) => Add(layer.Name, layer);

        #region IDictionary<string, DwgLayer> implementation

        public ICollection<string> Keys => ((IDictionary<string, DwgLayer>)_layers).Keys;

        public ICollection<DwgLayer> Values => ((IDictionary<string, DwgLayer>)_layers).Values;

        public int Count => ((IDictionary<string, DwgLayer>)_layers).Count;

        public bool IsReadOnly => ((IDictionary<string, DwgLayer>)_layers).IsReadOnly;

        public DwgLayer this[string key]
        {
            get => ((IDictionary<string, DwgLayer>)_layers)[key];
            set
            {
                ((IDictionary<string, DwgLayer>)_layers)[key] = value;
                _layersFromHandle[value.Handle] = value;
            }
        }

        public void Add(string key, DwgLayer value)
        {
            ((IDictionary<string, DwgLayer>)_layers).Add(key, value);
            _layersFromHandle.Add(value.Handle, value);
        }

        public bool ContainsKey(string key) => ((IDictionary<string, DwgLayer>)_layers).ContainsKey(key);

        public bool Remove(string key)
        {
            if (_layers.TryGetValue(key, out var layer))
            {
                _layersFromHandle.Remove(layer.Handle);
            }

            return ((IDictionary<string, DwgLayer>)_layers).Remove(key);
        }

        public bool TryGetValue(string key, out DwgLayer value) => ((IDictionary<string, DwgLayer>)_layers).TryGetValue(key, out value);

        public void Add(KeyValuePair<string, DwgLayer> item)
        {
            ((IDictionary<string, DwgLayer>)_layers).Add(item);
            _layersFromHandle.Add(item.Value.Handle, item.Value);
        }

        public void Clear()
        {
            ((IDictionary<string, DwgLayer>)_layers).Clear();
            _layersFromHandle.Clear();
        }

        public bool Contains(KeyValuePair<string, DwgLayer> item) => ((IDictionary<string, DwgLayer>)_layers).Contains(item);

        public void CopyTo(KeyValuePair<string, DwgLayer>[] array, int arrayIndex) => ((IDictionary<string, DwgLayer>)_layers).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, DwgLayer> item)
        {
            _layersFromHandle.Remove(item.Value.Handle);
            return ((IDictionary<string, DwgLayer>)_layers).Remove(item);
        }

        public IEnumerator<KeyValuePair<string, DwgLayer>> GetEnumerator() => ((IDictionary<string, DwgLayer>)_layers).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IDictionary<string, DwgLayer>)_layers).GetEnumerator();

        #endregion

    }
}
