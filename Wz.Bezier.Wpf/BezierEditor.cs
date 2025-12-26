using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Wz.Controls.Wpf.Commands;
using Wz.Controls.Wpf.Models;

namespace Wz.Controls.Wpf
{
    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    public class BezierEditor : Control
    {
        private readonly BezierPathModel _model = new BezierPathModel();
        private readonly EditCommandManager _cmd = new EditCommandManager();

        private const double HANDLE_RADIUS = 5.0;
        private const double POINT_RADIUS = 6.0;

        private enum DragMode { None, MovePoint, MoveHandleIn, MoveHandleOut, AddOnSegment }
        private BezierPoint _dragPoint;
        private Point _dragStart;
        private DragMode _dragMode = DragMode.None;
        // creating new point by click+drag
        private bool _isCreatingPoint;
        private Point _createStart;
        private Vector _createHandle;
        // store original values for command creation
        private Vector _originalHandle;
        private Point _originalPoint;

        private Canvas PART_Canvas;

        static BezierEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BezierEditor), new FrameworkPropertyMetadata(typeof(BezierEditor)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PART_Canvas = GetTemplateChild("PART_Canvas") as Canvas;
            if (PART_Canvas == null) throw new InvalidOperationException("BezierEditor template must contain a Canvas named PART_Canvas");

            _model.Changed += Redraw;
            // use preview mouse down/up so clicks on child visuals (ellipses/lines) don't prevent the canvas from receiving the event
            PART_Canvas.PreviewMouseLeftButtonDown += Canvas_MouseLeftButtonDown;
            PART_Canvas.MouseMove += Canvas_MouseMove;
            PART_Canvas.PreviewMouseLeftButtonUp += Canvas_MouseLeftButtonUp;
            KeyDown += BezierEditor_KeyDown;
            // ensure we can receive keyboard focus for shortcuts
            Focusable = true;
            PART_Canvas.Focusable = true;

            Redraw();
        }

        private void BezierEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Z && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                _cmd.Undo();
                _model.RaiseChanged();
            }
            else if (e.Key == Key.Y && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                _cmd.Redo();
                _model.RaiseChanged();
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(PART_Canvas);
            // give focus so KeyDown works
            Focus();
            PART_Canvas.Focus();

            var hitResult = HitTest(p);
            if (hitResult.point != null)
            {
                _dragPoint = hitResult.point;
                _dragStart = _dragPoint.Position;
                _originalPoint = _dragPoint.Position;
                _dragMode = hitResult.mode;
                _originalHandle = (_dragMode == DragMode.MoveHandleOut) ? _dragPoint.HandleOut : _dragPoint.HandleIn;
                // mark handled so template children (ellipses) don't also act on the event
                e.Handled = true;
                PART_Canvas.CaptureMouse();
            }
            else if (hitResult.mode == DragMode.AddOnSegment)
            {
                // insert new anchor into the segment at the clicked index
                var pt = new BezierPoint(p);
                var insIndex = Math.Max(0, Math.Min(_model.Points.Count, hitResult.segmentIndex + 1));
                _cmd.Execute(new InsertPointCommand(_model, insIndex, pt));
                e.Handled = true;
            }
            else
            {
                // start creating a new point; finalize on mouse up (click+drag to set handles)
                _isCreatingPoint = true;
                _createStart = p;
                _createHandle = new Vector(0, 0);
                e.Handled = true;
                PART_Canvas.CaptureMouse();
            }
        }

        // Hit test points and handles; returns the hit point and detected drag mode
        // Hit test points, handles and segments; returns the hit point (if any), detected drag mode, and segment index for insertion
        private (BezierPoint point, DragMode mode, int segmentIndex) HitTest(Point p)
        {
            const double handleThreshold = 8.0;
            const double pointThreshold = 10.0;

            BezierPoint bestPoint = null;
            DragMode bestMode = DragMode.None;
            double bestDist = double.MaxValue;
            int bestSegment = -1;

            foreach (var pt in _model.Points)
            {
                var dIn = (pt.HandleInAbsolute - p).Length;
                if (dIn <= handleThreshold && dIn < bestDist)
                {
                    bestDist = dIn;
                    bestPoint = pt;
                    bestMode = DragMode.MoveHandleIn;
                }

                var dOut = (pt.HandleOutAbsolute - p).Length;
                if (dOut <= handleThreshold && dOut < bestDist)
                {
                    bestDist = dOut;
                    bestPoint = pt;
                    bestMode = DragMode.MoveHandleOut;
                }

                var dPoint = (pt.Position - p).Length;
                if (dPoint <= pointThreshold && dPoint < bestDist)
                {
                    bestDist = dPoint;
                    bestPoint = pt;
                    bestMode = DragMode.MovePoint;
                }
            }

            // Also check segments (simple nearest point to bezier curve not implemented; check midpoints between anchors)
            for (int i = 0; i + 1 < _model.Points.Count; i++)
            {
                var a = _model.Points[i];
                var b = _model.Points[i + 1];
                var mid = new Point((a.Position.X + b.Position.X) / 2, (a.Position.Y + b.Position.Y) / 2);
                var d = (mid - p).Length;
                if (d < 12 && d < bestDist)
                {
                    bestDist = d;
                    bestPoint = null;
                    bestMode = DragMode.AddOnSegment;
                    bestSegment = i;
                }
            }

            return (bestPoint, bestMode, bestSegment);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(PART_Canvas);
            if (_isCreatingPoint)
            {
                // update creation handle
                _createHandle = p - _createStart;
                Redraw();
                return;
            }

            if (_dragPoint == null || e.LeftButton != MouseButtonState.Pressed) return;
            switch (_dragMode)
            {
                case DragMode.MoveHandleOut:
                    var newOut = p - _dragPoint.Position;
                    _dragPoint.HandleOut = newOut;
                    // mirror opposite handle unless Alt is pressed
                    if ((Keyboard.Modifiers & ModifierKeys.Alt) == 0)
                    {
                        _dragPoint.HandleIn = -newOut;
                    }
                    _model.RaiseChanged();
                    break;
                case DragMode.MoveHandleIn:
                    var newIn = p - _dragPoint.Position;
                    _dragPoint.HandleIn = newIn;
                    if ((Keyboard.Modifiers & ModifierKeys.Alt) == 0)
                    {
                        _dragPoint.HandleOut = -newIn;
                    }
                    _model.RaiseChanged();
                    break;
                case DragMode.MovePoint:
                    _dragPoint.Position = p;
                    _model.RaiseChanged();
                    break;
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isCreatingPoint)
            {
                // finalize creation
                var pt = new BezierPoint(_createStart);
                pt.HandleOut = _createHandle;
                pt.HandleIn = -_createHandle;
                _cmd.Execute(new AddPointCommand(_model, pt));
                _isCreatingPoint = false;
                Redraw();
            }

            if (_dragPoint != null)
            {
                // push appropriate command depending on drag mode
                var p = e.GetPosition(PART_Canvas);
                switch (_dragMode)
                {
                    case DragMode.MoveHandleOut:
                        var newOut = _dragPoint.HandleOut; // already updated
                        _cmd.Execute(new MoveHandleCommand(_dragPoint, _originalHandle, newOut, true));
                        break;
                    case DragMode.MoveHandleIn:
                        var newIn = _dragPoint.HandleIn;
                        _cmd.Execute(new MoveHandleCommand(_dragPoint, _originalHandle, newIn, false));
                        break;
                    case DragMode.MovePoint:
                        _cmd.Execute(new MovePointCommand(_dragPoint, _originalPoint, _dragPoint.Position));
                        break;
                }
            }

            _dragPoint = null;
            _dragMode = DragMode.None;
            PART_Canvas.ReleaseMouseCapture();
        }

        private void Redraw()
        {
            if (PART_Canvas == null) return;

            PART_Canvas.Children.Clear();

            // draw path
            if (_model.Points.Count > 0)
            {
                var sg = new StreamGeometry();
                using (var ctx = sg.Open())
                {
                    ctx.BeginFigure(_model.Points[0].Position, false, false);
                    for (int i = 1; i < _model.Points.Count; i++)
                    {
                        var a = _model.Points[i - 1];
                        var b = _model.Points[i];
                        ctx.BezierTo(a.HandleOutAbsolute, b.HandleInAbsolute, b.Position, true, false);
                    }
                }
                var path = new Path { Stroke = Brushes.Black, StrokeThickness = 1, Data = sg };
                PART_Canvas.Children.Add(path);
            }

            // draw points and handles
            foreach (var pt in _model.Points)
            {
                // handles
                var l1 = new Line { X1 = pt.Position.X, Y1 = pt.Position.Y, X2 = pt.HandleInAbsolute.X, Y2 = pt.HandleInAbsolute.Y, Stroke = Brushes.Gray, StrokeDashArray = new DoubleCollection { 2, 2 } };
                var l2 = new Line { X1 = pt.Position.X, Y1 = pt.Position.Y, X2 = pt.HandleOutAbsolute.X, Y2 = pt.HandleOutAbsolute.Y, Stroke = Brushes.Gray, StrokeDashArray = new DoubleCollection { 2, 2 } };
                PART_Canvas.Children.Add(l1);
                PART_Canvas.Children.Add(l2);

                DrawPoint(pt.HandleInAbsolute);

                DrawPoint(pt.HandleOutAbsolute);

                DrawPoint(pt.Position);
            }

            // draw creation preview when user is dragging to create a new point
            if (_isCreatingPoint)
            {
                // draw handle lines for the new point
                var inAbs = _createStart + (-_createHandle);
                var outAbs = _createStart + _createHandle;

                var l1 = new Line { X1 = _createStart.X, Y1 = _createStart.Y, X2 = inAbs.X, Y2 = inAbs.Y, Stroke = Brushes.Gray, StrokeDashArray = new DoubleCollection { 2, 2 } };
                var l2 = new Line { X1 = _createStart.X, Y1 = _createStart.Y, X2 = outAbs.X, Y2 = outAbs.Y, Stroke = Brushes.Gray, StrokeDashArray = new DoubleCollection { 2, 2 } };
                PART_Canvas.Children.Add(l1);
                PART_Canvas.Children.Add(l2);

                DrawPoint(inAbs);

                DrawPoint(outAbs);

                DrawPoint(_createStart);

                // preview curve from last point to this new point (if any points exist)
                if (_model.Points.Count > 0)
                {
                    var last = _model.Points[_model.Points.Count - 1];
                    var sg = new StreamGeometry();
                    using (var ctx = sg.Open())
                    {
                        ctx.BeginFigure(last.Position, false, false);
                        ctx.BezierTo(last.HandleOutAbsolute, inAbs, _createStart, true, false);
                    }
                    var path = new Path { Stroke = Brushes.DarkOrange, StrokeThickness = 1, Data = sg, Opacity = 0.7 };
                    PART_Canvas.Children.Add(path);
                }
            }
        }

        private void DrawPoint(Point pt)
        {
            var h1 = new Ellipse { Width = HANDLE_RADIUS * 2, Height = HANDLE_RADIUS * 2, Fill = Brushes.Black };
            Canvas.SetLeft(h1, pt.X - HANDLE_RADIUS);
            Canvas.SetTop(h1, pt.Y - HANDLE_RADIUS);
            PART_Canvas.Children.Add(h1);
        }
    }
}
