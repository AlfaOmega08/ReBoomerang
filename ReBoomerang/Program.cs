using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReBoomerang
{
    class Program
    {
        const double Version = 0.1;

        static void Main(string[] args)
        {
            Console.WriteLine("ReBoomerang " + Version);
            
            String file = null;
            if (args.Length != 0)
                file = args[args.Length - 1];

            if (file == null)
            {
                Console.Write("Executable path: ");
                file = Console.ReadLine();
            }

            FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            IExecutableLoader ldr = ExeLoaderFactory.Create(stream);
            if (ldr == null)
                throw new Exception("Sorry: can't handle that kind of file.");
            
            ldr.Initialize(stream);
            IDisassembler disasm = DisassemblerFactory.Create(ldr.GetArchitecture());
            disasm.Decode(0x8D06080);

            //Decompile(file);
        }
    }
}
