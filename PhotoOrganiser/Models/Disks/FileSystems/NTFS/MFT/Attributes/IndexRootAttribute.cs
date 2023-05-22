using ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes.Headers;
using ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes;
using ForensicX.Models.Disks.FileSystems.NTFS.MFT;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes
{
    // This is the root node of the binary tree that implements an index (e.g. a directory)
    public class IndexRootAttribute : BaseAttribute
    {
        //$INDEX_ROOT structure
        //Offset / Size / Description
        //~ / ~ / [Standard Attribute Header]
        //0 / 4 / Attribute Type
        //4 / 4 / Collation Rule
        //8 / 4 / Bytes per Index Record
        //12 / 1 / Clusters per Index Record
        //16 / 16 / Index Node Header
        //32 / X / Index Entry
        //32 + X / Y / Next Index Entry

        // If bytes per index record is less than the cluster size, then clusters per index record and all file's index record VCNs are specified in sectors.
        public ResidentAttributeHeader AttributeHeader { get; set; }
        public uint AttributeType { get; set; } //4 bytes
        public uint CollationRule { get; set; } //4 bytes
        public uint BytesPerIndexRecord { get; set; } // 4 bytes
        public byte ClustersPerIndexRecord { get; set; } // 1 byte
                                                         // 3 bytes padding

        public IndexNodeHeader IndexNodeHeader { get; set; }
        public List<IndexEntry> IndexEntries { get; set; } //Only present if the index header's flag is 0x00.

        public IndexRootAttribute(ResidentAttributeHeader attributeHeader)
        {
            AttributeHeader = attributeHeader;
            IndexEntries = new List<IndexEntry>();
            ParseAttributeData();
        }

        public void ParseAttributeData()
        {
            using (MemoryStream ms = new MemoryStream(AttributeHeader.AttributeData))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                AttributeType = reader.ReadUInt32();
                CollationRule = reader.ReadUInt32();
                BytesPerIndexRecord = reader.ReadUInt32();
                ClustersPerIndexRecord = reader.ReadByte();

                reader.ReadBytes(3); //Padding 3 bytes, discard.

                IndexNodeHeader = new IndexNodeHeader(reader);

                // Parse Index Entries
                while (reader.BaseStream.Position < IndexNodeHeader.IndexEntriesTotalLength)
                {
                    // Check if the BinaryReader has reached the end of the stream
                    if (reader.BaseStream.Position == reader.BaseStream.Length)
                    {
                        Console.WriteLine("End of the stream reached. No more Index Entries to parse.");
                        break;
                    }

                    IndexEntry entry = new IndexEntry(reader);
                    IndexEntries.Add(entry);

                    if ((entry.Flags & 0x02) != 0) // Check for the Last Index Entry in the Node flag
                    {
                        break; // Stop reading index entries
                    }
                }
            }
        }

        public override void PrintAttribute()
        {
            Console.WriteLine("Attribute                        :   $INDEX_ROOT");
            Console.WriteLine("Attribute Type                   :   " + AttributeType);
            Console.WriteLine("Collation Rule                   :   " + CollationRule);
            Console.WriteLine("Bytes per Index Record           :   " + BytesPerIndexRecord);
            Console.WriteLine("Cluster per Index Record         :   " + ClustersPerIndexRecord);
            Console.WriteLine("-----");
            Console.WriteLine("IndexNodeHeader: ");
            Console.WriteLine("     Offset to First Index Entry : " + IndexNodeHeader.OffsetToFirstIndexEntry);
            Console.WriteLine("     Index Entries Total Length  : " + IndexNodeHeader.IndexEntriesTotalLength);
            Console.WriteLine("     Allocated Node Size         : " + IndexNodeHeader.AllocatedNodeSize);
            Console.WriteLine("     Non-Leaf Node Flag          : " + IndexNodeHeader.NonLeafNodeFlag);
            Console.WriteLine("     Padding                     : " + IndexNodeHeader.Padding);
            foreach (IndexEntry indexEntry in IndexEntries)
            {
                //Debug.WriteLine("IE Filename: " + indexEntry.Filename);
                Debug.WriteLine("Index Entry MFT Number     :   " + indexEntry.FileReference);
            }
        }

        public string GetCollationRuleName(uint collationRule)
        {
            switch (collationRule)
            {
                case 0x00:
                    return "Binary";
                case 0x01:
                    return "Filename (Unicode Chains)";
                case 0x02:
                    return "Unicode (Equal, Uppercase first)";
                case 0x10:
                    return "ULONG (32 bits, little endian)";
                case 0x11:
                    return "SID (Security Identifier)";
                case 0x12:
                    return "Security Mix";
                case 0x13:
                    return "ULONGS (Sets of little endian 32 bits)";
                default:
                    return "Unknown";
            }
        }

        public string GetAttributeTypeName(uint attributeType)
        {
            switch (attributeType)
            {
                case 0x30:
                    return "$FILE_NAME";
                case 0x00:
                    return "View Index Only";
                default:
                    return "Invalid";
            }
        }
    }
}
