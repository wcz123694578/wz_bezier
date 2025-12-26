using Wz.Controls.Wpf.Models;
using System.Windows;

namespace Wz.Controls.Wpf.Commands
{
    public class MoveHandleCommand : IEditCommand
    {
        private readonly BezierPoint _point;
        private readonly Vector _oldHandle;
        private readonly Vector _newHandle;
        private readonly bool _isOut;

        public MoveHandleCommand(BezierPoint point, Vector oldHandle, Vector newHandle, bool isOut)
        {
            _point = point;
            _oldHandle = oldHandle;
            _newHandle = newHandle;
            _isOut = isOut;
        }

        public void Execute()
        {
            if (_isOut) _point.HandleOut = _newHandle;
            else _point.HandleIn = _newHandle;
        }

        public void Unexecute()
        {
            if (_isOut) _point.HandleOut = _oldHandle;
            else _point.HandleIn = _oldHandle;
        }
    }
}
