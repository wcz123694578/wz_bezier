using Wz.Controls.Wpf.Models;

namespace Wz.Controls.Wpf.Commands
{
    public class AddPointCommand : IEditCommand
    {
        private readonly BezierPathModel _model;
        private readonly BezierPoint _point;

        public AddPointCommand(BezierPathModel model, BezierPoint point)
        {
            _model = model;
            _point = point;
        }

        public void Execute()
        {
            _model.Add(_point);
        }

        public void Unexecute()
        {
            _model.Remove(_point);
        }
    }
}
