using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.Helpers
{
    public static class FileAttributeBitmaskToStringConverter
    {
        public const byte ReadOnly = 0x01;
        public const byte Hidden = 0x02;
        public const byte System = 0x04;
        public const byte VolumeLabel = 0x08;
        public const byte LongFileName = 0x0F;
        public const byte Directory = 0x10;
        public const byte Archive = 0x20;

        public static string GetString(byte attributes)
        {
            var attributeList = new List<string>();

            if ((attributes & ReadOnly) == ReadOnly)
                attributeList.Add("Read-only");

            if ((attributes & Hidden) == Hidden)
                attributeList.Add("Hidden");

            if ((attributes & System) == System)
                attributeList.Add("System");

            if ((attributes & VolumeLabel) == VolumeLabel)
                attributeList.Add("Volume Label");

            if ((attributes & Directory) == Directory)
                attributeList.Add("Directory");

            if ((attributes & Archive) == Archive)
                attributeList.Add("Archive");

            if (attributeList.Count == 0)
                attributeList.Add("None");

            return string.Join(", ", attributeList);
        }
    }
}
