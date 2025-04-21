using Evergine.Bindings.MeshOptimizer;
using System;

namespace HelloMeshlets
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, Meshlets!");
            
            uint kMaxVertices = 64;
            uint kMaxTriangles = 124;
            uint meshIndices = 67164;

            // 1084
            uint maxMeshlets = (uint)MeshOptNative.meshopt_buildMeshletsBound(meshIndices, kMaxVertices, kMaxTriangles);            
            
        }
    }
}
