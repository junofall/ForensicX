using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes.Headers
{
    public class ResidentAttributeHeader : StandardAttributeHeader
    {
        public int AttributeLengthC { get; set; }
        public ushort AttributeOffset { get; set; }
        public byte IndexationFlag { get; set; }
        public byte Padding { get; set; }
        public string AttributeName { get; set; }
        public byte[] AttributeData { get; set; }

        public int DataLength { get; set; }

        public ResidentAttributeHeader(byte[] attributeData) : base(attributeData)
        {
            Console.WriteLine("=====RAW HEADER DATA=====");
            PrintHeader(attributeData);
            Console.WriteLine("=======END RAW DATA=======");
            AttributeLengthC = BitConverter.ToInt32(attributeData, 0x10);
            AttributeOffset = BitConverter.ToUInt16(attributeData, 0x14);
            IndexationFlag = attributeData[0x16];
            Padding = attributeData[0x17];

            int headerLength = NameLength > 0 ? 0x18 + (NameLength * 2) : AttributeOffset;
            int dataLength = AttributeLengthC - headerLength;

            DataLength = dataLength; // Add this line to set the DataLength property


            if (dataLength > 0)
            {
                if (NameLength > 0)
                {
                    AttributeName = Encoding.Unicode.GetString(attributeData, OffsetToNameOrAttribute, NameLength * 2);
                    Debug.WriteLine("Attribute Name :" + AttributeName);
                    AttributeData = new byte[AttributeLengthC];
                    Array.Copy(attributeData, AttributeOffset, AttributeData, 0, AttributeLengthC);
                }
                else
                {
                    AttributeData = new byte[AttributeLengthC];
                    Array.Copy(attributeData, AttributeOffset, AttributeData, 0, AttributeLengthC);
                }
                // Add the following debug lines:
                Debug.WriteLine("==== RESIDENT ATTRIBUTE HEADER ====");
                Debug.WriteLine("Name Length: " + NameLength);
                Debug.WriteLine($"AttributeType: {AttributeType:X}");
                Debug.WriteLine($"AttributeLength: {AttributeLength}");
                Debug.WriteLine($"DataLength: {DataLength}");
                Debug.WriteLine($"AttributeOffset: {AttributeOffset}");
                Debug.WriteLine($"IndexationFlag: {IndexationFlag}");
                Debug.WriteLine($"Padding: {Padding}");
                Debug.WriteLine("===================================");
            }
            else
            {
                // Attribute has no data (?)
                AttributeData = new byte[0];
            }
        }





        private void PrintHeader(byte[] EntryData)
        {
            int bytesPerLine = 16;
            for (int i = 0; i < EntryData.Length; i++)
            {
                // Print byte in hexadecimal with 2 digits
                Console.Write($"{EntryData[i]:X2} ");

                // Add a dash after every 8 bytes
                if ((i + 1) % 8 == 0) Console.Write("-");

                // Add a newline and ASCII rendition after every 16 bytes
                if ((i + 1) % bytesPerLine == 0)
                {
                    Console.Write("|");
                    for (int j = i - bytesPerLine + 1; j <= i; j++)
                    {
                        // Print the ASCII character for the byte if it's a printable character, otherwise print a dot '.'
                        char c = (EntryData[j] >= 0x20 && EntryData[j] <= 0x7E) ? (char)EntryData[j] : '.';
                        Console.Write(c);
                    }
                    Console.WriteLine();
                }
            }

            // Print the remaining bytes and ASCII rendition if the data size is not a multiple of 16
            int remainingBytes = EntryData.Length % bytesPerLine;
            if (remainingBytes > 0)
            {
                int padding = bytesPerLine - remainingBytes;
                for (int i = 0; i < padding; i++)
                {
                    Console.Write("   "); // Print 3 spaces for alignment
                    if ((i + 1 + remainingBytes) % 8 == 0) Console.Write(" "); // Add a space after every 8 bytes
                }
                Console.Write("|");
                for (int i = EntryData.Length - remainingBytes; i < EntryData.Length; i++)
                {
                    // Print the ASCII character for the byte if it's a printable character, otherwise print a dot '.'
                    char c = (EntryData[i] >= 0x20 && EntryData[i] <= 0x7E) ? (char)EntryData[i] : '.';
                    Console.Write(c);
                }
                Console.WriteLine();
            }
        }
    }
}
