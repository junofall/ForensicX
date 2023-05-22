using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Helpers
{
    public static class VhdConverter
    {
        public static void CopyToNewVhd(string ddFilePath, string vhdFilePath)
        {
            Console.WriteLine("COPY");
            Console.WriteLine("Converting " + ddFilePath + " to VHD image '" + vhdFilePath + "'...");
            using (var ddStream = new FileStream(ddFilePath, FileMode.Open, FileAccess.Read))
            {
                Console.WriteLine("Source file opened.");
                using (var vhdStream = new FileStream(vhdFilePath, FileMode.Create, FileAccess.Write))
                {
                    // Copy the content of the .dd file to the .vhd file
                    Console.WriteLine("Copying to destination (this may take a while).");
                    ddStream.CopyTo(vhdStream);

                    // Create and write the VHD footer
                    Console.WriteLine("Generating VHD footer.");
                    byte[] vhdFooter = GenerateVhdFooter((ulong)ddStream.Length);
                    Console.WriteLine("Appending VHD footer.");
                    vhdStream.Write(vhdFooter, 0, vhdFooter.Length);
                    Console.WriteLine("Done!");
                }
            }
        }

        public static void ConvertToFixedVhd(string ddFilePath)
        {
            Console.WriteLine("CONVERT");
            Console.WriteLine("Converting " + ddFilePath + " into a VHD...");
            using (FileStream ddFile = new FileStream(ddFilePath, FileMode.Open, FileAccess.ReadWrite))
            {
                Console.WriteLine("Source file opened.");
                // Seek to the end of the file
                ddFile.Seek(0, SeekOrigin.End);

                // Generate the VHD footer
                Console.WriteLine("Generating VHD footer.");
                byte[] vhdFooter = GenerateVhdFooter((ulong)ddFile.Length);

                // Append the VHD footer to the .dd file
                Console.WriteLine("Appending VHD footer.");
                ddFile.Write(vhdFooter, 0, vhdFooter.Length);
                Console.WriteLine("Done!");
            }

            // Generate new VHD file path
            var vhdFilePath = Path.ChangeExtension(ddFilePath, ".vhd");

            // Rename the file to .vhd
            if (File.Exists(vhdFilePath))
            {
                Console.WriteLine("A file with the same name already exists. File was not renamed.");
            }
            else
            {
                File.Move(ddFilePath, vhdFilePath);
                Console.WriteLine("File renamed to: " + vhdFilePath);
            }
        }

        public static void RemoveVhdFooter(string vhdFilePath)
        {
            Console.WriteLine("REMOVE");
            Console.WriteLine("Removing VHD footer...");
            using (FileStream vhdFile = new FileStream(vhdFilePath, FileMode.Open, FileAccess.ReadWrite))
            {
                Console.WriteLine("File opened");
                // Check if the file size is larger than 512 bytes
                if (vhdFile.Length > 512)
                {
                    // Seek to the beginning of the footer
                    vhdFile.Seek(-512, SeekOrigin.End);

                    // Read the first 8 bytes of the footer to check for the 'connectix' cookie
                    byte[] cookieBuffer = new byte[8];
                    vhdFile.Read(cookieBuffer, 0, 8);
                    string cookie = Encoding.ASCII.GetString(cookieBuffer);

                    if (cookie == "conectix")
                    {
                        // Remove the last 512 bytes (VHD footer) by setting the file length
                        vhdFile.SetLength(vhdFile.Length - 512);
                    }
                    else
                    {
                        // The footer does not contain the 'connectix' cookie, do not modify the file
                        Console.WriteLine("The file does not have a VHD footer.");
                    }
                }
                else
                {
                    // The file is too small, do not modify it
                    Console.WriteLine("The file is too small to remove a VHD footer.");
                }
            }
        }

        private static byte[] GenerateVhdFooter(ulong fileSize)
        {
            byte[] footer = new byte[512];

            // 1. Cookie
            byte[] cookie = Encoding.ASCII.GetBytes("conectix");
            Buffer.BlockCopy(cookie, 0, footer, 0, 8);

            // 2. Features
            BitConverter.GetBytes(0x02000000).CopyTo(footer, 8);

            // 3. File Format Version
            BitConverter.GetBytes(0x00000100).CopyTo(footer, 12);

            // 4. Data Offset
            BitConverter.GetBytes(0xFFFFFFFFFFFFFFFF).CopyTo(footer, 16);

            // 5. Time Stamp
            int secondsSince20000101 = (int)(DateTime.UtcNow - new DateTime(2000, 1, 1)).TotalSeconds;
            BitConverter.GetBytes(secondsSince20000101).CopyTo(footer, 24);

            // 6. Creator Application
            byte[] creatorApp = Encoding.ASCII.GetBytes("FnzX");
            Buffer.BlockCopy(creatorApp, 0, footer, 28, 4);

            // 7. Creator Version
            BitConverter.GetBytes(0x00001000).CopyTo(footer, 32);

            // 8. Creator Host OS
            byte[] creatorHostOS = Encoding.ASCII.GetBytes("Wi2k");
            Buffer.BlockCopy(creatorHostOS, 0, footer, 36, 4);

            // 9. Original Size
            BitConverter.GetBytes(BitConverter.IsLittleEndian ? BitConverter.ToUInt64(BitConverter.GetBytes(fileSize).Reverse().ToArray(), 0) : fileSize).CopyTo(footer, 40);
            // 10. Current Size
            BitConverter.GetBytes(BitConverter.IsLittleEndian ? BitConverter.ToUInt64(BitConverter.GetBytes(fileSize).Reverse().ToArray(), 0) : fileSize).CopyTo(footer, 48);
            // 11. Disk Geometry
            BitConverter.GetBytes((ushort)0xFFFF).Reverse().ToArray().CopyTo(footer, 56);
            footer[58] = 0x10;
            footer[59] = 0xFF;

            // 12. Disk Type
            BitConverter.GetBytes(0x02000000).CopyTo(footer, 60);

            // 14. Unique ID
            Guid uniqueId = Guid.NewGuid();
            byte[] uniqueIdBytes = uniqueId.ToByteArray();
            Buffer.BlockCopy(uniqueIdBytes, 0, footer, 68, 16);

            // 15. Saved State
            footer[84] = 0x00;

            // 13. Checksum
            uint checksum = CalculateVhdFooterChecksum(footer);
            BitConverter.GetBytes(BitConverter.IsLittleEndian ? BitConverter.ToUInt32(BitConverter.GetBytes(checksum).Reverse().ToArray(), 0) : checksum).CopyTo(footer, 64);

            return footer;
        }

        private static uint CalculateVhdFooterChecksum(byte[] footer)
        {
            uint checksum = 0;
            for (int i = 0; i < footer.Length; i++)
            {
                if (i < 64 || i > 67) // Skip the Checksum field (offset 64, length 4)
                {
                    checksum += footer[i];
                }
            }
            return ~checksum; // One's complement of the sum
        }
    }
}
