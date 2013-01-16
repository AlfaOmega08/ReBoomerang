using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReBoomerang
{
    public interface IDisassembler
    {
        String Decode(uint instruction);
    }
}
