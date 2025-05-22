using Evergine.Bindings.MeshOptimizer;
using Evergine.Mathematics;
using OBJRuntime.DataTypes;
using OBJRuntime.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HelloMeshlets
{
    public class Program
    {
        public unsafe static void Main(string[] args)
        {
            // This will not be needed when we use a nuget package
            NativeLibrary.SetDllImportResolver(typeof(Evergine.Bindings.MeshOptimizer.meshopt_Bounds).Assembly, ResolveRuntimes);

            Console.WriteLine("Hello, Meshlets!");

            // Read obj file
            var attrib = new OBJAttrib();
            var shapes = new List<OBJShape>();
            var materials = new List<OBJMaterial>();
            var warning = string.Empty;
            var error = string.Empty;
            using (var stream = new FileStream("horse_statue_01_1k.obj", FileMode.Open, FileAccess.Read))
            using (var srObj = new StreamReader(stream))
            {
                bool success = OBJLoader.Load(srObj, ref attrib, shapes, materials, ref warning, ref error, null, string.Empty, true, true);
                if (!success)
                {
                    throw new Exception($"OBJ Load failed. Error:{error}");
                }
            }

            var mesh = shapes[0].Mesh;

            uint kMaxVertices = 64;
            uint kMaxTriangles = 124;
            float kConeWeight = 0.0f;
            UIntPtr meshNumIndices = (UIntPtr)mesh.Indices.Count;
            UIntPtr meshNumVertices = (UIntPtr)attrib.Vertices.Count;

            uint[] indices = new uint[meshNumIndices];
            for (int i = 0; i < (int)meshNumIndices; i++)
            {
                indices[i] = (uint)mesh.Indices[i].VertexIndex;
            }

            // Create Meshlet array
            UIntPtr maxMeshlets = MeshOptNative.meshopt_buildMeshletsBound(meshNumIndices, kMaxVertices, kMaxTriangles);

            meshopt_Meshlet[] meshlets = new meshopt_Meshlet[maxMeshlets];
            uint[] meshletVertices = new uint[maxMeshlets * kMaxVertices];
            byte[] meshletTriangles = new byte[maxMeshlets * kMaxTriangles * 3];
            Vector3[] positions = attrib.Vertices.ToArray();

            meshopt_Meshlet* pMeshlets = (meshopt_Meshlet*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(meshlets));
            uint* pMeshletVertices = (uint*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(meshletVertices));
            byte* pMeshletTriangles = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(meshletTriangles));
            uint* pIndices = (uint*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(indices));
            float* pVertsAsFloats = (float*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(positions));

            UIntPtr meshletCount = MeshOptNative.meshopt_buildMeshlets(
                                                                    pMeshlets,
                                                                    pMeshletVertices,
                                                                    pMeshletTriangles,
                                                                    pIndices,
                                                                    meshNumIndices,
                                                                    pVertsAsFloats,
                                                                    meshNumVertices,
                                                                    (uint)sizeof(Vector3),
                                                                    kMaxVertices,
                                                                    kMaxTriangles,
                                                                    kConeWeight);

            var last = meshlets[meshletCount - 1];
            Array.Resize(ref meshletVertices, (int)(last.vertex_offset + last.vertex_count));
            Array.Resize(ref meshletTriangles, (int)(last.triangle_offset + ((last.triangle_count * 3 + 3) & ~3)));
            Array.Resize(ref meshlets, (int)meshletCount);            

            Console.WriteLine($"{meshletCount} meshlets generated");
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
