using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReBoomerang
{
    public class ExeLoaderFactory
    {
        public static IExecutableLoader Create(Stream stream)
        {
            String[] parsers = Directory.GetFiles("loaders", "*.dll");
            foreach (String plugin in parsers)
            {
                stream.Seek(0, SeekOrigin.Begin);

                Assembly a = Assembly.LoadFrom(plugin);
                Type[] types = a.GetTypes();
                foreach (Type type in types)
                {
                    if (type.GetInterface("IExecutableLoader") != null)
                    {
                        MethodInfo isValid = type.GetMethod("IsValidExecutable", BindingFlags.Static | BindingFlags.Public);
                        if (isValid != null)
                        {
                            if ((bool)isValid.Invoke(null, new object[] { stream }))
                                return Activator.CreateInstance(type) as IExecutableLoader;
                        }
                    }
                }
            }

            return null;
        }
    }
}
