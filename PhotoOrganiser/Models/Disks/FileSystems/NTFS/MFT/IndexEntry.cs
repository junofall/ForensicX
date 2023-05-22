using System;
using System.Diagnostics;
using System.IO;

namespace ForensicX.Models.Disks.FileSystems.NTFS.MFT
{
    public class IndexEntry
    {
        public ulong FileReference { get; set; }
        public ushort LengthOfIndexEntry { get; set; }
        public ushort LengthOfStream { get; set; }
        public byte Flags { get; set; }
        public byte[] Padding1 { get; set; }
        public byte[] Stream { get; set; }
        public byte[] Padding2 { get; set; }
        public ulong? SubNodeVCN { get; set; }


        public IndexEntry(BinaryReader reader)
        {
            FileReference = reader.ReadUInt64();
            Console.WriteLine("DEBUG: FileRef = " + FileReference);
            LengthOfIndexEntry = reader.ReadUInt16();
            Console.WriteLine("DEBUG: Index Entry Length = " + LengthOfIndexEntry);
            LengthOfStream = reader.ReadUInt16();
            Console.WriteLine("DEBUG: Stream Length = " + LengthOfStream);
            Flags = reader.ReadByte();
            Console.WriteLine("DEBUG: Flags = " + Flags);
            Padding1 = reader.ReadBytes(3); // 3 bytes padding after flag (?)

            if ((Flags & 0x02) == 0) // Check if the last entry flag is NOT set
            {
                Stream = reader.ReadBytes(LengthOfStream);
            }

            // Calculate padding length to reach a multiple of 8
            int paddingLength = (8 - ((int)reader.BaseStream.Position % 8)) % 8;
            Padding2 = reader.ReadBytes(paddingLength);

            Console.WriteLine("Padding1: ");
            for (int i = 0; i < Padding1.Length; i++)
            {
                Console.Write($"{Padding1[i]:X2} ");
            }
            if (Padding1.Length == 0)
            {
                Console.Write("Padding1 is empty.");
            }
            Console.WriteLine();


            Console.WriteLine("Padding2: ");
            for (int i = 0; i < Padding2.Length; i++)
            {
                Console.Write($"{Padding2[i]:X2} ");
            }
            if (Padding2.Length == 0)
            {
                Console.Write("Padding2 is empty.");
            }
            Console.WriteLine();

            if ((Flags & 0x01) != 0) // Check for the SubNode flag
            {
                var SubNodeVCNPosition = LengthOfIndexEntry - 8;
                Debug.WriteLine("SubnodeVCNPosition : " + SubNodeVCNPosition);
                reader.BaseStream.Position = (LengthOfIndexEntry - 8);
                SubNodeVCN = reader.ReadUInt64();
            }
            else
            {
                SubNodeVCN = null;
            }
        }



        public void PrintIndexEntry()
        {
            Console.WriteLine("====================================================================");
            Console.WriteLine($"File Reference               :   {FileReference} (0x{FileReference:X16})");
            Console.WriteLine($"Length of Index Entry        :   {LengthOfIndexEntry} (0x{LengthOfIndexEntry:X4})");
            Console.WriteLine($"Length of Stream             :   {LengthOfStream} (0x{LengthOfStream:X4})");
            Console.WriteLine($"Flags                        :   0x{Flags:X2}");
            Console.WriteLine($"  Sub-node Present           :   {((Flags & 0x01) != 0)}");
            Console.WriteLine($"  Last Entry                 :   {((Flags & 0x02) != 0)}");
            if (SubNodeVCN.HasValue)
            {
                Console.WriteLine($"Sub-node VCN                 :   {SubNodeVCN.Value} (0x{SubNodeVCN.Value:X16})");
            }
            else
            {
                Console.WriteLine($"Sub-node VCN                 :   N/A");
            }
            Console.WriteLine("Stream                        :");
            if (Stream != null)
            {
                for (int i = 0; i < Stream.Length; i++)
                {
                    Console.Write($"{Stream[i]:X2} ");
                    if ((i + 1) % 16 == 0)
                    {
                        Console.WriteLine();
                    }
                }
            }
            else
            {
                Console.WriteLine("N/A");
            }
            Console.WriteLine();
        }
    }
}
