using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeParser
{
    public struct OptionalHeader
    {
        public void Read(BinaryReader br)
        {
            Magic = br.ReadUInt16();
            MajorLinkerVersion = br.ReadByte();
            MinorLinkerVersion = br.ReadByte();
            SizeOfCode = br.ReadUInt32();
            SizeOfInitializedData = br.ReadUInt32();
            SizeOfUninitializedData = br.ReadUInt32();
            AddressOfEntryPoint = br.ReadUInt32();
            BaseOfCode = br.ReadUInt32();

            if (Magic == 0x10B)
            {
                BaseOfData = br.ReadUInt32();
                ImageBase = br.ReadUInt32();
            }
            else
            {
                ImageBase = br.ReadUInt64();
                BaseOfData = 0;
            }

            SectionAlignment = br.ReadUInt32();
            FileAlignment = br.ReadUInt32();

            MajorOperatingSystemVersion = br.ReadUInt16();
            MinorOperatingSystemVersion = br.ReadUInt16();
            MajorImageVersion = br.ReadUInt16();
            MinorImageVersion = br.ReadUInt16();
            MajorSubsystemVersion = br.ReadUInt16();
            MinorSubsystemVersion = br.ReadUInt16();
            Win32VersionValue = br.ReadUInt32();
            SizeOfImage = br.ReadUInt32();
            SizeOfHeaders = br.ReadUInt32();
            CheckSum = br.ReadUInt32();
            Subsystem = br.ReadUInt16();
            DllCharacteristics = br.ReadUInt16();

            if (Magic == 0x10B)
            {
                SizeOfStackReserve = br.ReadUInt32();
                SizeOfStackCommit = br.ReadUInt32();
                SizeOfHeapReserve = br.ReadUInt32();
                SizeOfHeapCommit = br.ReadUInt32();
            }
            else
            {
                SizeOfStackReserve = br.ReadUInt64();
                SizeOfStackCommit = br.ReadUInt64();
                SizeOfHeapReserve = br.ReadUInt64();
                SizeOfHeapCommit = br.ReadUInt64();
            }

            LoaderFlags = br.ReadUInt32();
            NumberOfRvaAndSizes = br.ReadUInt32();

            RvaSizes = new RvaSize[NumberOfRvaAndSizes];
            for (int i = 0; i < NumberOfRvaAndSizes; i++)
            {
                RvaSizes[i].Rva = br.ReadUInt32();
                RvaSizes[i].Size = br.ReadUInt32();
            }
        }

        public ushort Magic;
        public byte MajorLinkerVersion;
        public byte MinorLinkerVersion;
        public uint SizeOfCode;
        public uint SizeOfInitializedData;
        public uint SizeOfUninitializedData;
        public uint AddressOfEntryPoint;
        public uint BaseOfCode;
        public uint BaseOfData;
        public ulong ImageBase;
        public uint SectionAlignment;
        public uint FileAlignment;
        public ushort MajorOperatingSystemVersion;
        public ushort MinorOperatingSystemVersion;
        public ushort MajorImageVersion;
        public ushort MinorImageVersion;
        public ushort MajorSubsystemVersion;
        public ushort MinorSubsystemVersion;
        public uint Win32VersionValue;
        public uint SizeOfImage;
        public uint SizeOfHeaders;
        public uint CheckSum;
        public ushort Subsystem;
        public ushort DllCharacteristics;
        public ulong SizeOfStackReserve;
        public ulong SizeOfStackCommit;
        public ulong SizeOfHeapReserve;
        public ulong SizeOfHeapCommit;
        public uint LoaderFlags;
        public uint NumberOfRvaAndSizes;

        public struct RvaSize
        {
            public uint Rva;
            public uint Size;
        };

        RvaSize[] RvaSizes;
    }
}
