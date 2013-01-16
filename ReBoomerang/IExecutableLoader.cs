using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReBoomerang
{
    public enum Architecture
    {
        x86, x64, PowerPC, ARM, Sparc, Mips
    }

    public interface IExecutableLoader
    {
        void Initialize(Stream stream);

        ulong GetEntryPoint();
        Architecture GetArchitecture();
    }
}
