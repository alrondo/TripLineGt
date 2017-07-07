using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripLine.Toolbox.UI.Dialog
{
    public abstract class CommonDialogManager
    {
        private static CommonDialogManager _current = new DefaultCommonDialogManager();

        public static CommonDialogManager Current
        {
            get 
            { 
                return _current; 
            }
            set
            {
                if (value != null)
                {
                    _current = value;
                }
            }
        }

        public abstract OpenFileDialogResult ShowOpenFileDialog(string filter);        
    }
}
