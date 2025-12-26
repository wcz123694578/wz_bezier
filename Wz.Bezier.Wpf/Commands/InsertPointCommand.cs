using Wz.Controls.Wpf.Models;

namespace Wz.Controls.Wpf.Commands
{
    public class InsertPointCommand : IEditCommand
    {
        private readonly BezierPathModel _model;
        private readonly BezierPoint _point;
        private readonly int _index;

        public InsertPointCommand(BezierPathModel model, int index, BezierPoint point)
        {
            _model = model;
            _index = index;
            _point = point;
        }

        public void Execute()
        {
            _model.Insert(_index, _point);
        }

        public void Unexecute()
        {
            _model.Remove(_point);
        }
    }
}
