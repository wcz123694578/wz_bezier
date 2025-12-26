using System.Windows;
using Wz.Controls.Wpf.Models;

namespace Wz.Controls.Wpf.Commands
{
    public class MovePointCommand : IEditCommand
    {
        private readonly BezierPoint _point;
        private readonly Point _oldPos;
        private readonly Point _newPos;

        public MovePointCommand(BezierPoint point, Point oldPos, Point newPos)
        {
            _point = point;
            _oldPos = oldPos;
            _newPos = newPos;
        }

        public void Execute()
        {
            _point.Position = _newPos;
        }

        public void Unexecute()
        {
            _point.Position = _oldPos;
        }
    }
}
