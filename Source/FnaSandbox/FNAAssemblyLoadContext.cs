using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace FnaSandbox
{
    public class FNAAssemblyLoadContext : AssemblyLoadContext
    {
        internal void Init()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Environment.Is64BitProcess ? "x64" : "x86");
            var dlls = Directory.EnumerateFiles(path, "*.dll");

            foreach (var dll in dlls)
            {
                this.LoadUnmanagedDllFromPath(dll);
            }
        }

        protected override Assembly Load(AssemblyName assemblyName) => null;
    }
}
