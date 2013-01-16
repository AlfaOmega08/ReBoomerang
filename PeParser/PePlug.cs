using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PeParser
{
    public class PePlug : ReBoomerang.IExecutableLoader
    {
        ImageHeader imHdr;
        OptionalHeader optHdr;
        SectionHeader[] sectHdrs;

        public PePlug()
        {
        }

        public void Initialize(Stream stream)
        {
            if (!IsValidExecutable(stream))
                throw new Exception("This loader plugin can't handle that kind of stream.");

            BinaryReader br = new BinaryReader(stream); 
            stream.Seek(0x3C, SeekOrigin.Begin);

            uint peOff = br.ReadUInt32();
            stream.Seek(peOff, SeekOrigin.Begin);

            imHdr = new ImageHeader();
            imHdr.Read(br);

            optHdr = new OptionalHeader();
            optHdr.Read(br);

            sectHdrs = new SectionHeader[imHdr.NumberOfSections];
            for (int i = 0; i < imHdr.NumberOfSections; i++)
            {
                sectHdrs[i] = new SectionHeader(optHdr.ImageBase);
                sectHdrs[i].Read(br);
            }

            foreach (SectionHeader s in sectHdrs)
            {
                stream.Seek(s.PointerToRawData, SeekOrigin.Begin);
                s.Content = new byte[s.VirtualSize];
                stream.Read(s.Content, 0, s.VirtualSize < s.SizeOfRawData ? (int)s.VirtualSize : (int)s.SizeOfRawData);
            }
        }

        public ulong GetEntryPoint()
        {
            return optHdr.AddressOfEntryPoint + optHdr.ImageBase;
        }

        public ReBoomerang.Architecture GetArchitecture()
        {
            switch (imHdr.Machine)
            {
                case 0x14C:
                    return ReBoomerang.Architecture.x86;
                case 0x8664:
                    return ReBoomerang.Architecture.x64;
                case 0x1F0:
                case 0x1F2:  // Xbox 360 Executable
                    return ReBoomerang.Architecture.PowerPC;
                default:
                    throw new Exception("Unknown machine code: " + imHdr.Machine.ToString("X"));
            }
        }

        public static bool IsValidExecutable(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            
            stream.Seek(0, SeekOrigin.Begin);
            byte[] mz = br.ReadBytes(2);

            if (mz[0] != 'M' || mz[1] != 'Z')
                return false;

            stream.Seek(0x3C, SeekOrigin.Begin);
            uint peOffset = br.ReadUInt32();
            if (peOffset > stream.Length || peOffset < 0x40)
                return false;

            stream.Seek(peOffset, SeekOrigin.Begin);
            byte[] pe = br.ReadBytes(4);
            if (pe[0] != 'P' || pe[1] != 'E' || pe[2] != 0 || pe[3] != 0)
                return false;

            return true;
        }
    }
}
