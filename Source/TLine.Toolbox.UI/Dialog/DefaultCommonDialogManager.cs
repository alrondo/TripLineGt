using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripLine.Toolbox.UI.Dialog
{
    public class DefaultCommonDialogManager : CommonDialogManager
    {
        public override OpenFileDialogResult ShowOpenFileDialog(string filter)
        {
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = filter; // Filter files by extension

            // Multiselect
            //dialog.Multiselect = true;
            //dialog.FileNames;

            // Show open file dialog box
            var result = dialog.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                return new OpenFileDialogResult
                {
                    IsFileSelected = true,
                    PathOfSelectedFile = dialog.FileName,
                };
            }
            else
            {
                return new OpenFileDialogResult
                {
                    IsFileSelected = false,
                    PathOfSelectedFile = null,
                };
            }
        }
    }
}
