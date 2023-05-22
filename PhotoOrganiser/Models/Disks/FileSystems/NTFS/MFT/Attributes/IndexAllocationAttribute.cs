using ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes.Headers;
using ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes;
using ForensicX.Models.Disks.FileSystems.NTFS.MFT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes
{
    public class IndexAllocationAttribute : BaseAttribute
    {

        public NonResidentAttributeHeader AttributeHeader { get; }
        public List<byte[]> IndexBlocksData { get; }
        public List<IndexEntry> IndexEntries { get; set; }

        public IndexAllocationAttribute(NonResidentAttributeHeader attributeHeader, List<byte[]> indexBlocksData)
        {
            AttributeHeader = attributeHeader;
            IndexBlocksData = indexBlocksData;

            // You can now process the index blocks data as needed
            ParseIndexEntries();
        }

        public override void PrintAttribute()
        {
            Console.WriteLine("Attribute            :   $INDEX_ALLOCATION");
            PrintIndexBlocksData();
        }

        public void PrintIndexBlocksData()
        {
            //Print the raw index record data
            int bytesPerLine = 16;
            var IndexBlockCount = 0;
            foreach (byte[] block in IndexBlocksData)
            {
                Console.WriteLine("Index Block Entry " + IndexBlockCount);

                for (int i = 0; i < block.Length; i++)
                {
                    //Print byte in hexadecimal with 2 digits
                    Console.Write($"{block[i]:X2} ");

                    //Add a dash after every 8 bytes
                    if ((i + 1) % 8 == 0) Console.Write("-");

                    //Add a newline and ASCII rendition after every 16 bytes
                    if ((i + 1) % bytesPerLine == 0)
                    {
                        Console.Write("|");
                        for (int j = i - bytesPerLine + 1; j <= i; j++)
                        {
                            //Print the ASCII character for the byte if it's a printable character, otherwise print a dot '.'
                            char c = (block[j] >= 0x20 && block[j] <= 0x7E) ? (char)block[j] : '.';
                            Console.Write(c);
                        }
                        Console.WriteLine();
                    }
                }

                //Print the remaining bytes and ASCII rendition if the data size is not a multiple of 16
                int remainingBytes = block.Length % bytesPerLine;
                if (remainingBytes > 0)
                {
                    int padding = bytesPerLine - remainingBytes;
                    for (int i = 0; i < padding; i++)
                    {
                        Console.Write("   "); // Print 3 spaces for alignment
                        if ((i + 1 + remainingBytes) % 8 == 0) Console.Write(" "); // Add a space after every 8 bytes
                    }
                    Console.Write("|");
                    for (int i = block.Length - remainingBytes; i < block.Length; i++)
                    {
                        //Print the ASCII character for the byte if it's a printable character, otherwise print a dot '.'
                        char c = (block[i] >= 0x20 && block[i] <= 0x7E) ? (char)block[i] : '.';
                        Console.Write(c);
                    }
                    Console.WriteLine();
                }
                IndexBlockCount++;
            }
        }

        public void ParseIndexEntries()
        {
            IndexEntries = new List<IndexEntry>();

            foreach (byte[] indexBlockData in IndexBlocksData)
            {


                // Parse the INDX record header (Standard Index Header)
                // Assuming the fixed size of 0x18 for the Standard Index Header
                int indexEntriesOffset = BitConverter.ToInt32(indexBlockData, 0x18);

                int currentIndex = indexEntriesOffset;

                using (MemoryStream ms = new MemoryStream(indexBlockData))
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    reader.BaseStream.Seek(indexEntriesOffset, SeekOrigin.Begin);

                    while (true)
                    {
                        IndexEntry entry = new IndexEntry(reader);

                        // Check if it's the last entry (has the INDEX_ENTRY_END flag set)
                        if ((entry.Flags & 0x02) != 0)
                        {
                            break;
                        }

                        IndexEntries.Add(entry);

                        // Move to the next index entry
                        currentIndex += entry.LengthOfIndexEntry;
                        reader.BaseStream.Seek(currentIndex, SeekOrigin.Begin);
                    }
                }
            }
        }
    }
}