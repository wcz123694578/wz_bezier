using System;

namespace Wz.Controls.Wpf.Commands
{
    public interface IEditCommand
    {
        void Execute();
        void Unexecute();
    }
}
