using System.Collections.Generic;

namespace Wz.Controls.Wpf.Commands
{
    public class EditCommandManager
    {
        private readonly Stack<IEditCommand> _undo = new Stack<IEditCommand>();
        private readonly Stack<IEditCommand> _redo = new Stack<IEditCommand>();

        public bool CanUndo => _undo.Count > 0;
        public bool CanRedo => _redo.Count > 0;

        public void Execute(IEditCommand cmd)
        {
            cmd.Execute();
            _undo.Push(cmd);
            _redo.Clear();
        }

        public void Undo()
        {
            if (!CanUndo) return;
            var cmd = _undo.Pop();
            cmd.Unexecute();
            _redo.Push(cmd);
        }

        public void Redo()
        {
            if (!CanRedo) return;
            var cmd = _redo.Pop();
            cmd.Execute();
            _undo.Push(cmd);
        }
    }
}
