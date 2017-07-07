using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TripLine.DesktopApp.ContentUserControl
{
    public interface IWindowCommands
    {
        void AddLeft(UIElement element);

        void AddRight(UIElement element);

        void RemoveLeft(UIElement element);

        void RemoveLeft(Func<UIElement, bool> removeAction);

        void RemoveRight(UIElement element);

        bool LeftExist(UIElement element);

        bool RightExist(UIElement element);

        void EnableButtons(bool enable);
    }

}
