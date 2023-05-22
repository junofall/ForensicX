using ForensicX.Models.Disks.FileSystems.NTFS.MFT;
using ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes;
using ForensicX.Models.Disks.FileSystems.NTFS.MFT.Attributes.Headers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Models.Disks.FileSystems.NTFS.MFT
{
    public class MftFileRecord
    {
        // File record header properties
        public string Signature { get; set; } // Signature (must be 'FILE') ; 4 bytes
        public ushort OffsetToUpdateSequence { get; set; } // 2 bytes
        public ushort UpdateSequenceSizeInWords { get; set; } // 2 bytes
        public long LogFileSequenceNumber { get; set; } // 8 bytes
        public ushort SequenceNumber { get; set; } // 2 bytes
        public ushort HardLinkCount { get; set; } // 2 bytes
        public ushort OffsetToFirstAttribute { get; set; }  // 2 bytes
        public ushort Flags { get; set; } // 2 bytes ; Flags ; 0x01 = In Use ; 0x02 = Record is Directory (Filename Index present) ; 0x04 = Record is an extension (Set for records in the $Extend directory) ; 0x08 = Special index present (Set for non-directory records containing an index, $Secure, $ObjID, $Quota, $Reparse 
        public uint EntrySize { get; set; } // 4 bytes ; Real size of the MFT record
        public uint AllocatedEntrySize { get; set; } // 4 bytes ; Allocated size of the MFT record
        public long BaseFileRecord { get; set; } // 8 bytes ; File reference to the base FILE record
        public ushort NextAttributeID { get; set; } // 2 bytes ; Next attribute ID

        // Here, there is a 2 byte alignment to a 4 byte boundary.
        public uint RecordID { get; set; } // 4 bytes (GPT4 says this is 8 bytes?)
        public ushort UpdateSequenceNumber { get; set; } // 2 bytes
        public byte[] UpdateSequenceArray { get; set; } // (2 * UpdateSequenceSizeInWords) - 2 bytes
        public List<BaseAttribute> Attributes { get; set; }
        public NTFSFileSystem ParentFileSystem { get; private set; }

        public string FilePath { get; set; }
        public byte[] EntryData { get; set; }

        public MftFileRecord(byte[] entryData, NTFSFileSystem parentFileSystem)
        {
            ParentFileSystem = parentFileSystem;
            EntryData = entryData;
            ParseFileRecordHeader(EntryData);
        }

        public bool IsValid()
        {
            return Signature == "FILE";
        }

        private void ParseFileRecordHeader(byte[] entryData)
        {
            // Read the file record header properties
            Signature = Encoding.ASCII.GetString(entryData, 0, 4);
            // Check if entryData is smaller than the minimum required size (e.g., 24 bytes)
            if (!IsValid())
            {
                Debug.WriteLine("Invalid Entry");
                return;
            }
            OffsetToUpdateSequence = BitConverter.ToUInt16(entryData, 4);
            UpdateSequenceSizeInWords = BitConverter.ToUInt16(entryData, 6);
            LogFileSequenceNumber = BitConverter.ToInt64(entryData, 8);
            SequenceNumber = BitConverter.ToUInt16(entryData, 16);
            HardLinkCount = BitConverter.ToUInt16(entryData, 18);
            OffsetToFirstAttribute = BitConverter.ToUInt16(entryData, 20);
            Flags = BitConverter.ToUInt16(entryData, 22);
            EntrySize = BitConverter.ToUInt32(entryData, 24);
            AllocatedEntrySize = BitConverter.ToUInt32(entryData, 28);
            BaseFileRecord = BitConverter.ToInt64(entryData, 32);
            NextAttributeID = BitConverter.ToUInt16(entryData, 40);
            // Align to 4 byte boundary. (2 bytes)
            RecordID = BitConverter.ToUInt32(entryData, 44);
            UpdateSequenceNumber = BitConverter.ToUInt16(entryData, 48);
            UpdateSequenceArray = new byte[4];
            Buffer.BlockCopy(entryData, 50, UpdateSequenceArray, 0, 4);
            //PrintProperties();
            ParseAttributes(entryData);
            PrintMftRecord(this);
        }

        private void ParseAttributes(byte[] entryData)
        {
            Attributes = new List<BaseAttribute>();
            int entryIndex = OffsetToFirstAttribute;

            while (true)
            {
                uint attributeType = BitConverter.ToUInt32(entryData, entryIndex);
                if (attributeType == 0xFFFFFFFF) // End of attribute list
                {
                    break;
                }

                int attributeLength = BitConverter.ToInt32(entryData, entryIndex + 4);
                byte[] attributeData = new byte[attributeLength];
                Array.Copy(entryData, entryIndex, attributeData, 0, attributeLength);

                StandardAttributeHeader attributeHeader;
                bool isResident = attributeData[8] == 0x00;

                if (isResident)
                {
                    attributeHeader = new ResidentAttributeHeader(attributeData);
                }
                else
                {
                    attributeHeader = new NonResidentAttributeHeader(attributeData);

                }

                //PrintAttributeHeader(attributeHeader);

                switch (attributeType)
                {
                    case 0x10: //$STANDARD_INFORMATION
                        if (isResident)
                        {
                            StandardInformationAttribute standardInfo = new StandardInformationAttribute((ResidentAttributeHeader)attributeHeader, this);
                            Attributes.Add(standardInfo);
                        }
                        // Handle the case if the standard information attribute is non-resident (unlikely, but you can decide what to do)
                        break;
                    case 0x20: //$ATTRIBUTE_LIST
                        Debug.WriteLine("=====UNIMPLEMENTED ATTRIBUTE: 0x20 $ATTRIBUTE_LIST");
                        break;
                    // Add cases for other attribute types you want to handle
                    case 0x30: //$FILE_NAME
                        if (isResident)
                        {
                            FileNameAttribute fileNameAttr = new FileNameAttribute((ResidentAttributeHeader)attributeHeader);
                            Attributes.Add(fileNameAttr);
                        }
                        break;
                    case 0x40: //$VOLUME_VERSION (NT) // $OBJECT_ID (2K)
                        Debug.WriteLine("=====UNIMPLEMENTED ATTRIBUTE: 0x40 $VOLUME_VERSION");
                        break;
                    case 0x50: //$SECURITY_DESCRIPTOR
                        Debug.WriteLine("=====UNIMPLEMENTED ATTRIBUTE: 0x50 $SECURITY_DESCRIPTOR");
                        break;
                    case 0x60: //$VOLUME_NAME
                        Debug.WriteLine("=====UNIMPLEMENTED ATTRIBUTE: 0x60 $VOLUME_NAME");
                        break;
                    case 0x70: //$VOLUME_INFORMATION
                        Debug.WriteLine("=====UNIMPLEMENTED ATTRIBUTE: 0x70 $VOLUME_INFORMATION");
                        break;
                    case 0x80: //$DATA
                        if (isResident)
                        {
                            DataAttribute dataAttr = new DataAttribute((ResidentAttributeHeader)attributeHeader);
                            Attributes.Add(dataAttr);
                        }
                        else
                        {
                            DataAttribute dataAttr = new DataAttribute((NonResidentAttributeHeader)attributeHeader);
                            Attributes.Add(dataAttr);
                        }
                        break;
                    case 0x90: //$INDEX_ROOT
                        if (attributeHeader is ResidentAttributeHeader a)
                        {
                            if (a.AttributeName != "$I30")
                            {
                                break;
                            }
                            //IndexRootAttribute indexRootAttr = new IndexRootAttribute(a);
                            //Attributes.Add(indexRootAttr);
                        }
                        break;
                    case 0xA0: //$INDEX_ALLOCATION
                        if (attributeHeader is NonResidentAttributeHeader b)
                        {
                            //if (b.AttributeName != "$I30")
                            //{
                            //   break;
                            //}
                            //List<byte[]> indexBlocksData = new List<byte[]>();
                            foreach (var dataRun in b.DataRuns)
                            {
                                //byte[] indexBlockData = ParentFileSystem.ReadDataByClusters(dataRun.StartCluster, dataRun.Length);
                                //indexBlocksData.Add(indexBlockData);
                            }

                            //IndexAllocationAttribute indexAllocAttr = new IndexAllocationAttribute(b, indexBlocksData);
                            //Attributes.Add(indexAllocAttr);
                        }
                        break;

                    case 0xB0: //$BITMAP
                        if (isResident)
                        {
                            BitmapAttribute bitmapAttribute = new BitmapAttribute((ResidentAttributeHeader)attributeHeader);
                            Attributes.Add(bitmapAttribute);
                        }
                        else
                        {
                            BitmapAttribute bitmapAttribute = new BitmapAttribute((NonResidentAttributeHeader)attributeHeader);
                            Attributes.Add(bitmapAttribute);
                        }
                        break;
                    case 0xC0: //$SYMBOLIC_LINK (NT) / $REPARSE_POINT (2K)
                        Debug.WriteLine("=====UNIMPLEMENTED ATTRIBUTE: 0xC0 $SYMBOLIC_LINK");
                        break;
                    case 0xD0: //$EA_INFORMATION
                        Debug.WriteLine("=====UNIMPLEMENTED ATTRIBUTE: 0xD0 $EA_INFORMATION");
                        break;
                    case 0xE0: //$EA
                        Debug.WriteLine("=====UNIMPLEMENTED ATTRIBUTE: 0xE0 $EA");
                        break;
                    case 0xF0: //$PROPERTY_SET (NT)
                        Debug.WriteLine("=====UNIMPLEMENTED ATTRIBUTE: 0xF0 $PROPERTY_SET");
                        break;
                    case 0x100: //$LOGGED_UTILITY_STREAM (2K)
                        Debug.WriteLine("=====UNIMPLEMENTED ATTRIBUTE: 0x100 $LOGGED_UTILITY_STREAM");
                        break;
                    default:
                        throw new Exception("=====Attribute of unknown type " + attributeType);
                }
                entryIndex += attributeLength;
            }
        }

        public static void PrintAttributeHeader(StandardAttributeHeader attributeHeader)
        {
            Debug.WriteLine("~~Attribute Header @ MftFileRecord~~~");
            Debug.WriteLine($"Attribute Type: 0x{attributeHeader.AttributeType:X}");
            Debug.WriteLine($"Attribute Length: {attributeHeader.AttributeLength}");
            Debug.WriteLine($"Non-resident Flag: {attributeHeader.NonResidentFlag}");
            Debug.WriteLine($"Name Length: {attributeHeader.NameLength}");
            Debug.WriteLine($"Offset to Name: {attributeHeader.OffsetToNameOrAttribute}");
            Debug.WriteLine($"Flags: {attributeHeader.Flags}");
            Debug.WriteLine($"Attribute ID: {attributeHeader.AttributeID}");

            if (attributeHeader is ResidentAttributeHeader residentHeader)
            {
                Debug.WriteLine("Header is Resident");
                Debug.WriteLine($"Length of Attribute: {residentHeader.AttributeLengthC}");
                Debug.WriteLine($"Offset to Attribute: {residentHeader.AttributeOffset}");
                Debug.WriteLine($"Indexed Flag: {residentHeader.IndexationFlag}");
            }
            else if (attributeHeader is NonResidentAttributeHeader nonResidentHeader)
            {
                Debug.WriteLine("Header is Non-Resident");
                Debug.WriteLine($"Starting VCN: {nonResidentHeader.InitialVCN}");
                Debug.WriteLine($"Last VCN: {nonResidentHeader.FinalVCN}");
                Debug.WriteLine($"Offset to Data Runs: {nonResidentHeader.DataRunOffset}");
                Debug.WriteLine($"Compression Unit Size: {nonResidentHeader.CompressionUnitSize}");
                Debug.WriteLine($"Allocated Size: {nonResidentHeader.AllocatedSize}");
                Debug.WriteLine($"Real Size: {nonResidentHeader.RealSize}");
                Debug.WriteLine($"Initialized Data Size: {nonResidentHeader.InitializedSize}");
            }
        }

        private FileNameAttribute GetFirstFileNameAttribute()
        {
            foreach (BaseAttribute attribute in Attributes)
            {
                if (attribute is FileNameAttribute fileNameAttribute)
                {
                    return fileNameAttribute;
                }
            }
            return null;
        }

        public string GetFileName()
        {
            FileNameAttribute fileNameAttribute = GetFirstFileNameAttribute();
            return fileNameAttribute?.Filename;
        }

        public ulong? GetFileAllocatedSize()
        {
            FileNameAttribute fileNameAttribute = GetFirstFileNameAttribute();
            return fileNameAttribute?.AllocatedSize;
        }

        public DateTime? GetModificationTime()
        {
            FileNameAttribute fileNameAttribute = GetFirstFileNameAttribute();
            return fileNameAttribute?.ModificationTime;
        }

        public DateTime? GetLastAccessedTime()
        {
            FileNameAttribute fileNameAttribute = GetFirstFileNameAttribute();
            return fileNameAttribute?.LastAccessTime;
        }

        public DateTime? GetCreationTime()
        {
            FileNameAttribute fileNameAttribute = GetFirstFileNameAttribute();
            return fileNameAttribute?.CreationTime;
        }

        public void PrintMftRecord(MftFileRecord record)
        {
            Console.WriteLine();
            Console.WriteLine("===================");
            Console.WriteLine($"MFT Entry: {RecordID}");
            Console.WriteLine("===================");

            // Print the raw MFT record data
            //int bytesPerLine = 16;
            //for (int i = 0; i < EntryData.Length; i++)
            //{
            // Print byte in hexadecimal with 2 digits
            //Console.Write($"{EntryData[i]:X2} ");

            // Add a dash after every 8 bytes
            //if ((i + 1) % 8 == 0) Console.Write("-");

            // Add a newline and ASCII rendition after every 16 bytes
            //if ((i + 1) % bytesPerLine == 0)
            //{
            //Console.Write("|");
            //for (int j = i - bytesPerLine + 1; j <= i; j++)
            //{
            // Print the ASCII character for the byte if it's a printable character, otherwise print a dot '.'
            //char c = (EntryData[j] >= 0x20 && EntryData[j] <= 0x7E) ? (char)EntryData[j] : '.';
            //Console.Write(c);
            //}
            //Console.WriteLine();
            //}
            //}

            // Print the remaining bytes and ASCII rendition if the data size is not a multiple of 16
            //int remainingBytes = EntryData.Length % bytesPerLine;
            //if (remainingBytes > 0)
            //{
            //int padding = bytesPerLine - remainingBytes;
            //for (int i = 0; i < padding; i++)
            //{
            // Console.Write("   "); // Print 3 spaces for alignment
            //if ((i + 1 + remainingBytes) % 8 == 0) Console.Write(" "); // Add a space after every 8 bytes
            //}
            //Console.Write("|");
            //for (int i = EntryData.Length - remainingBytes; i < EntryData.Length; i++)
            //{
            // Print the ASCII character for the byte if it's a printable character, otherwise print a dot '.'
            //char c = (EntryData[i] >= 0x20 && EntryData[i] <= 0x7E) ? (char)EntryData[i] : '.';
            //Console.Write(c);
            //}
            //Console.WriteLine();
            //}


            Console.WriteLine("====================================================================");
            Console.WriteLine($"Signature                   :   {record.Signature}");
            Console.WriteLine($"OffsetToUpdateSequence      :   {record.OffsetToUpdateSequence} (0x{record.OffsetToUpdateSequence:X4})");
            Console.WriteLine($"UpdateSequenceSizeInWords   :   {record.UpdateSequenceSizeInWords} (0x{record.UpdateSequenceSizeInWords:X4})");
            Console.WriteLine($"LogFileSequenceNumber       :   {record.LogFileSequenceNumber} (0x{record.LogFileSequenceNumber:X16})");
            Console.WriteLine($"SequenceNumber              :   {record.SequenceNumber} (0x{record.SequenceNumber:X4})");
            Console.WriteLine($"HardLinkCount               :   {record.HardLinkCount} (0x{record.HardLinkCount:X4})");
            Console.WriteLine($"OffsetToFirstAttribute      :   {record.OffsetToFirstAttribute} (0x{record.OffsetToFirstAttribute:X4})");
            Console.WriteLine($"Flags                       :   0x{record.Flags:X4}");
            Console.WriteLine($"  In Use                    :   {((record.Flags & 0x01) != 0)}");
            Console.WriteLine($"  Is Directory              :   {((record.Flags & 0x02) != 0)}");
            Console.WriteLine($"  Is Extension              :   {((record.Flags & 0x04) != 0)}");
            Console.WriteLine($"  Special Index Present     :   {((record.Flags & 0x08) != 0)}");
            Console.WriteLine($"EntrySize                   :   {record.EntrySize} (0x{record.EntrySize:X8})");
            Console.WriteLine($"AllocatedEntrySize          :   {record.AllocatedEntrySize} (0x{record.AllocatedEntrySize:X8})");
            Console.WriteLine($"BaseFileRecord              :   {record.BaseFileRecord} (0x{record.BaseFileRecord:X8})");
            Console.WriteLine($"NextAttributeID             :   {record.NextAttributeID} (0x{record.NextAttributeID:X4})");
            Console.WriteLine($"RecordID                    :   {record.RecordID} (0x{record.RecordID:X8})");
            Console.WriteLine($"UpdateSequenceNumber        :   {record.UpdateSequenceNumber} (0x{record.UpdateSequenceNumber:X16})");
            Console.Write("UpdateSequenceArray         :   ");
            for (int i = 0; i < record.UpdateSequenceArray.Length; i++)
            {
                Console.Write($"{record.UpdateSequenceArray[i]:X2} ");
            }
            Console.WriteLine();

            int attributeCounter = 1;
            foreach (var attribute in Attributes)
            {
                Console.WriteLine("---------------");
                Console.WriteLine($"Attribute: {attributeCounter}");
                Console.WriteLine("---------------");

                attribute.PrintAttribute();

                if (attribute is IndexRootAttribute a)
                {
                    foreach (IndexEntry b in a.IndexEntries)
                    {
                        b.PrintIndexEntry();
                    }
                }

                if (attribute is IndexAllocationAttribute c)
                {
                    foreach (IndexEntry d in c.IndexEntries)
                    {
                        d.PrintIndexEntry();
                    }
                }

                attributeCounter++;
            }
            Console.WriteLine();
        }
    }
}
