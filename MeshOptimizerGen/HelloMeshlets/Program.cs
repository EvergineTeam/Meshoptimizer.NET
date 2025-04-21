using Evergine.Bindings.MeshOptimizer;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace HelloMeshlets
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // This will not be needed when we use a nuget package
            NativeLibrary.SetDllImportResolver(typeof(Evergine.Bindings.MeshOptimizer.meshopt_Bounds).Assembly, ResolveRuntimes);

            Console.WriteLine("Hello, Meshlets!");
            
            uint kMaxVertices = 64;
            uint kMaxTriangles = 124;
            uint meshIndices = 67164;

            // 1084
            uint maxMeshlets = MeshOptNative.meshopt_buildMeshletsBound(meshIndices, kMaxVertices, kMaxTriangles);    
            
        }

        private static IntPtr ResolveRuntimes(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            var rid = RuntimeInformation.RuntimeIdentifier;
            var runtimesFolder = Path.Combine(Path.GetDirectoryName(assembly.Location), $@"runtimes/{rid}/native");
            if (Directory.Exists(runtimesFolder))
            {
                var files = Directory.GetFiles(runtimesFolder, $"*{libraryName}.*");
                foreach (var file in files)
                {
                    if (NativeLibrary.TryLoad(file, out var handle))
                    {
                        return handle;
                    }
                }
            }

            return NativeLibrary.Load(libraryName, assembly, searchPath);
        }
    }
}
