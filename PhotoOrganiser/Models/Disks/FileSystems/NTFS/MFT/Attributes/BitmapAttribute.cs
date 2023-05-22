using ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes.Headers;
using ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes
{
    public class BitmapAttribute : BaseAttribute
    {
        public byte[] BitmapData { get; private set; }

        public BitmapAttribute(StandardAttributeHeader attributeHeader)
        {
            if (attributeHeader is ResidentAttributeHeader ResidentHeader)
            {
                ParseNonResidentData(ResidentHeader);
                Debug.WriteLine("BITMAP RESIDENT");
            }
            else
            {
                Debug.WriteLine("BITMAP NON RESIDENT");
            }
        }

        private void ParseNonResidentData(ResidentAttributeHeader attributeHeader)
        {
            // Parsing non-resident data involves reading the run list and fetching the data from
            // the clusters specified by the run list. This requires access to the disk or disk image,
            // and is more complex than parsing resident data.
            // You'll need to implement the logic for reading the run list and fetching the data.
        }

        public override void PrintAttribute()
        {
            Console.WriteLine("Attribute                   :    $BITMAP");
            Console.WriteLine("             [Needs Implementing]");
        }
    }
}
