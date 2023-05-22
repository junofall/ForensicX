using ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes.Headers;
using ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes
{
    public class StandardInformationAttribute : BaseAttribute
    {
        public MftFileRecord ParentFileRecord { get; private set; }

        public DateTime CreationTime { get; private set; }
        public DateTime ModificationTime { get; private set; }
        public DateTime MftModificationTime { get; private set; }
        public DateTime LastAccessTime { get; private set; }
        public uint FileAttributes { get; private set; }
        public uint MaximumVersions { get; private set; }
        public uint VersionNumber { get; private set; }
        public uint ClassId { get; private set; }

        public StandardInformationAttribute(ResidentAttributeHeader attributeHeader, MftFileRecord parentFileRecord)
        {
            ParentFileRecord = parentFileRecord;
            ParseAttribute(attributeHeader.AttributeData);
        }

        private void ParseAttribute(byte[] attributeData)
        {
            CreationTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(attributeData, 0));
            ModificationTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(attributeData, 8));
            MftModificationTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(attributeData, 16));
            LastAccessTime = DateTime.FromFileTimeUtc(BitConverter.ToInt64(attributeData, 24));
            FileAttributes = BitConverter.ToUInt32(attributeData, 32);
            MaximumVersions = BitConverter.ToUInt32(attributeData, 36);
            VersionNumber = BitConverter.ToUInt32(attributeData, 40);
            ClassId = BitConverter.ToUInt32(attributeData, 44);
        }

        public override void PrintAttribute()
        {
            Console.WriteLine("Attribute                   :    $STANDARD_INFORMATION");
            Console.WriteLine("Creation Time               :    " + CreationTime);
            Console.WriteLine("Modification Time           :    " + ModificationTime);
            Console.WriteLine("MFT Modification Time       :    " + MftModificationTime);
            Console.WriteLine("Last Access Time            :    " + LastAccessTime);

            Console.WriteLine("DOS File Attributes         :    ");
            PrintFileAttribute("Read-Only", 0x0001, FileAttributes);
            PrintFileAttribute("Hidden", 0x0002, FileAttributes);
            PrintFileAttribute("System", 0x0004, FileAttributes);
            PrintFileAttribute("Archive", 0x0020, FileAttributes);
            PrintFileAttribute("Device", 0x0040, FileAttributes);
            PrintFileAttribute("Normal", 0x0080, FileAttributes);
            PrintFileAttribute("Temporary", 0x0100, FileAttributes);
            PrintFileAttribute("Sparse File", 0x0200, FileAttributes);
            PrintFileAttribute("Reparse Point", 0x0400, FileAttributes);
            PrintFileAttribute("Compressed", 0x0800, FileAttributes);
            PrintFileAttribute("Offline", 0x1000, FileAttributes);
            PrintFileAttribute("Not Content Indexed", 0x2000, FileAttributes);
            PrintFileAttribute("Encrypted", 0x4000, FileAttributes);

            Console.WriteLine("Maximum Versions            :    " + MaximumVersions);
            Console.WriteLine("Version Number              :    " + VersionNumber);
            Console.WriteLine("Class ID                    :    " + ClassId);
        }

        private void PrintFileAttribute(string attributeName, int attributeValue, uint fileAttributes)
        {
            if ((fileAttributes & attributeValue) == attributeValue)
            {
                Console.WriteLine($"    {attributeName} (0x{attributeValue:X4})");
            }
        }
    }
}
