using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReBoomerang
{
    public class DisassemblerFactory
    {
        public static IDisassembler Create(Architecture arch)
        {
            String plug = null;

            switch (arch)
            {
                case Architecture.PowerPC:
                    plug = "disasm\\PPCDisassembler.dll";
                    break;
                default:
                    break;
            }

            if (plug != null)
            {
                Assembly a = Assembly.LoadFrom(plug);
                Type[] types = a.GetTypes();
                foreach (Type type in types)
                {
                    if (type.GetInterface("IDisassembler") != null)
                        return Activator.CreateInstance(type) as IDisassembler;
                }
            }

            return null;
        }
    }
}
