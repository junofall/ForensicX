using ForensicX.Models.Disks.DOS_BPB;

namespace ForensicX.Models.Disks.DOS_EBPB
{
    public class Dos40Ebpb : Dos331Bpb // FAT12 / FAT16 / HPFS
    {
        // DOS 3.31 BPB Fields
        public byte PhysicalDriveNumber { get; set; }

        public byte Reserved { get; set; }

        public byte ExtendedBootSignature { get; set; }

        public uint VolumeId { get; set; }

        public string VolumeLabel { get; set; }

        public string FileSystemType { get; set; }
    }
}
