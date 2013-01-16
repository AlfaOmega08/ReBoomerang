using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReBoomerang
{
    public abstract class ImageSection
    {
        public byte[] Content;

        public abstract bool Bss { get; }
        public abstract bool Code { get; }
        public abstract bool Data { get; }
        public abstract bool ReadOnly { get; }
        public abstract ulong Size { get; }
        public abstract ulong VirtualAddress { get; }
    }
}
