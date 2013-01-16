using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeParser
{
    public class SectionHeader : ReBoomerang.ImageSection
    {
        public SectionHeader(ulong imageBase)
        {
            ImageBase = imageBase;
        }

        public void Read(BinaryReader br)
        {
            Name = new String(br.ReadChars(8));
            VirtualSize = br.ReadUInt32();
            _VirtualAddress = br.ReadUInt32();
            SizeOfRawData = br.ReadUInt32();
            PointerToRawData = br.ReadUInt32();
            PointerToRelocations = br.ReadUInt32();
            PointerToLinenumbers = br.ReadUInt32();
            NumberOfRelocations = br.ReadUInt16();
            NumberOfLinenumbers = br.ReadUInt16();
            Characteristics = br.ReadUInt32();
        }

        public override bool Bss
        {
            get
            {
                return (Characteristics & 0x80) != 0;
            }
        }

        public override bool Code
        {
            get
            {
                return (Characteristics & 0x20) != 0;
            }
        }

        public override bool Data
        {
            get
            {
                return (Characteristics & 0x40) != 0;
            }
        }

        public override bool ReadOnly
        {
            get
            {
                return (Characteristics & 0x80000000) != 0;
            }
        }

        public override ulong Size
        {
            get
            {
                return VirtualSize;
            }
        }

        public override ulong VirtualAddress
        {
            get
            {
                return _VirtualAddress + ImageBase;
            }
        }

        public String Name = null;
        public uint VirtualSize = 0;
        public uint _VirtualAddress = 0;
        public uint SizeOfRawData = 0;
        public uint PointerToRawData = 0;
        public uint PointerToRelocations = 0;
        public uint PointerToLinenumbers = 0;
        public ushort NumberOfRelocations = 0;
        public ushort NumberOfLinenumbers = 0;
        public uint Characteristics = 0;

        private ulong ImageBase;
    }
}
