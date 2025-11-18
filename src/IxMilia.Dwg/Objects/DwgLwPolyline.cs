using System;
using System.Collections.Generic;
using System.Linq;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgLwPolyline
    {
        public List<DwgLwPolylineVertex> Vertices = new List<DwgLwPolylineVertex>();

        public DwgLwPolyline(IEnumerable<DwgLwPolylineVertex> vertices)
            : this()
        {
            Vertices.AddRange(vertices);
        }

        internal override void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            BindVertices();
        }

        internal override void OnBeforeEntityWrite(DwgVersionId version)
        {
            DeconstructVertices();
        }

        internal void BindVertices()
        {
            Vertices.Clear();
            for (int i = 0; i < _pointCount; i++)
            {
                var startingWidth = HasWidths ? _widths[i].Item1 : 0.0;
                var endingWidth = HasWidths ? _widths[i].Item2 : 0.0;
                var bulge = HasBulges ? _bulges[i] : 0.0;
                var vertex = new DwgLwPolylineVertex(_points[i].Item1, _points[i].Item2, startingWidth, endingWidth, bulge);
                Vertices.Add(vertex);
            }
        }

        internal void DeconstructVertices()
        {
            _points = Vertices.Select(v => Tuple.Create(v.X, v.Y)).ToList();
            _bulges = Vertices.Select(v => v.Bulge).ToList();
            _widths = Vertices.Select(v => Tuple.Create(v.StartWidth, v.EndWidth)).ToList();

            HasNormal = Normal != DwgVector.Zero;
            HasThickness = Thickness != 0.0;
            HasElevation = Elevation != 0.0;

            HasBulges = _bulges.Any(b => b != 0.0);
            if (!HasBulges)
            {
                _bulges.Clear();
            }

            var hasCustomWidth = _widths.Any(w => w.Item1 != 0.0 || w.Item2 != 0.0);
            HasWidths = hasCustomWidth;
            HasConstantWidth = !hasCustomWidth;
            if (!hasCustomWidth)
            {
                _widths.Clear();
            }

            _pointCount = _points.Count;
            _bulgeCount = _bulges.Count;
            _widthCount = _widths.Count;
        }
    }
}
