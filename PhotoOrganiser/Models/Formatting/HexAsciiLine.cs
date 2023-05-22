using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Formatting
{
    public class HexAsciiLine
    {
        public int LineNumber { get; set; }
        public ObservableCollection<string> HexBytes { get; set; }
        public string Ascii { get; set; }
    }

}
