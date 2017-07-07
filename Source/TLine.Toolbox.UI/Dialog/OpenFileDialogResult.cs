using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripLine.Toolbox.UI.Dialog
{
    public class OpenFileDialogResult
    {
        public bool IsFileSelected { get; set; }

        public string PathOfSelectedFile { get; set; }
    }
}
