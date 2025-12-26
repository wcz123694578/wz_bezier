using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Wz.Controls.Wpf.Models
{
    // Represents a single node (anchor) on a bezier path
    public class BezierPoint
    {
        public Point Position { get; set; }
        // Handle points are stored as offsets from Position
        public Vector HandleIn { get; set; }
        public Vector HandleOut { get; set; }
        public bool IsSelected { get; set; }

        public BezierPoint() { }
        public BezierPoint(Point pos)
        {
            Position = pos;
            HandleIn = new Vector(-30, 0);
            HandleOut = new Vector(30, 0);
        }

        public Point HandleInAbsolute => Position + HandleIn;
        public Point HandleOutAbsolute => Position + HandleOut;

        public BezierPoint Clone()
        {
            return new BezierPoint
            {
                Position = this.Position,
                HandleIn = this.HandleIn,
                HandleOut = this.HandleOut,
                IsSelected = this.IsSelected
            };
        }
    }

    public class BezierPathModel
    {
        private readonly List<BezierPoint> _points = new List<BezierPoint>();

        public IReadOnlyList<BezierPoint> Points => _points;

        public event Action Changed;

        public void Add(BezierPoint p)
        {
            _points.Add(p);
            Changed?.Invoke();
        }

        public void Insert(int index, BezierPoint p)
        {
            _points.Insert(index, p);
            Changed?.Invoke();
        }

        public void Remove(BezierPoint p)
        {
            _points.Remove(p);
            Changed?.Invoke();
        }

        public void Clear()
        {
            _points.Clear();
            Changed?.Invoke();
        }

        public void RaiseChanged() => Changed?.Invoke();

        public BezierPoint GetClosest(Point p, double tolerance)
        {
            BezierPoint best = null;
            double bestDist = tolerance;
            foreach (var pt in _points)
            {
                var d = (pt.Position - p).Length;
                if (d <= bestDist)
                {
                    best = pt;
                    bestDist = d;
                }
            }
            return best;
        }
    }
}
