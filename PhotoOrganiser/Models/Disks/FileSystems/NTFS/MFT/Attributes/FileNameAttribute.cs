using ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes.Headers;
using ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes
{
    public class FileNameAttribute : BaseAttribute
    {
        public ulong ParentDirectoryReference { get; private set; }
        public DateTime CreationTime { get; private set; }
        public DateTime ModificationTime { get; private set; }
        public DateTime MftModificationTime { get; private set; }
        public DateTime LastAccessTime { get; private set; }
        public ulong AllocatedSize { get; private set; }
        public ulong RealSize { get; private set; }
        public uint Flags { get; private set; }
        public uint EaSize { get; private set; }
        public byte FilenameLength { get; private set; }
        public byte FilenameNamespace { get; private set; }
        public string Filename { get; private set; }

        public FileNameAttribute(ResidentAttributeHeader attributeHeader)
        {
            ParseAttribute(attributeHeader.AttributeData);
            //PrintAttribute();
        }

        private void ParseAttribute(byte[] AttributeData)
        {
            int attributeContentOffset = 0;
            ParentDirectoryReference = BitConverter.ToUInt64(AttributeData, attributeContentOffset);
            CreationTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(AttributeData, attributeContentOffset + 8));
            ModificationTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(AttributeData, attributeContentOffset + 16));
            MftModificationTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(AttributeData, attributeContentOffset + 24));
            LastAccessTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(AttributeData, attributeContentOffset + 32));
            AllocatedSize = BitConverter.ToUInt64(AttributeData, attributeContentOffset + 40);
            RealSize = BitConverter.ToUInt64(AttributeData, attributeContentOffset + 48);
            Flags = BitConverter.ToUInt32(AttributeData, attributeContentOffset + 56);
            EaSize = BitConverter.ToUInt32(AttributeData, attributeContentOffset + 60);
            FilenameLength = AttributeData[attributeContentOffset + 64];
            FilenameNamespace = AttributeData[attributeContentOffset + 65];
            Filename = Encoding.Unicode.GetString(AttributeData, attributeContentOffset + 66, FilenameLength * 2);
        }

        public override void PrintAttribute()
        {
            Console.WriteLine("Attribute                   :    $FILE_NAME");
            Console.WriteLine("Creation Time               :    " + CreationTime);
            Console.WriteLine("Modification Time           :    " + ModificationTime);
            Console.WriteLine("MFT Modification Time       :    " + MftModificationTime);
            Console.WriteLine("Last Access Time            :    " + LastAccessTime);
            Console.WriteLine("Allocated Size              :    " + AllocatedSize);
            Console.WriteLine("Real Size                   :    " + RealSize);
            Console.WriteLine("Flags                       :    " + Flags);
            Console.WriteLine("Ea Size                     :    " + EaSize);
            Console.WriteLine("Filename Length             :    " + FilenameLength);
            Console.WriteLine("Filename Namespace          :    " + FilenameNamespace);
            Console.WriteLine("Filename                    :    " + Filename);
        }
    }
}
