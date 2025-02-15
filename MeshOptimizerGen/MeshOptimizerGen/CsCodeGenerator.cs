using CppAst;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshOptimizerGen
{
    public class CsCodeGenerator
    {
        private CsCodeGenerator()
        {
        }

        public static CsCodeGenerator Instance { get; } = new CsCodeGenerator();

        public void Generate(CppCompilation compilation, string outputPath)
        {
            GenerateConstants(compilation, outputPath);
            GenerateEnums(compilation, outputPath);
            GenerateStructs(compilation, outputPath);
            GenerateFuntions(compilation, outputPath);
        }

        private void GenerateConstants(CppCompilation compilation, string outputPath)
        {
            Debug.WriteLine("Generating Constants...");

            using (StreamWriter file = File.CreateText(Path.Combine(outputPath, "Constants.cs")))
            {
                file.WriteLine("using System;");
                file.WriteLine("using System.Runtime.InteropServices;\n");
                file.WriteLine($"namespace Evergine.Bindings.MeshOptimizer");
                file.WriteLine("{");
                file.WriteLine($"\tpublic static partial class MeshOptNative");
                file.WriteLine("\t{");

                foreach (var cppMacro in compilation.Macros)
                {
                    if (string.IsNullOrEmpty(cppMacro.Value)
                        || cppMacro.Name.Equals("MESHOPTIMIZER_ALLOC_CALLCONV")
                        || cppMacro.Name.Equals("MESHOPTIMIZER_EXPERIMENTAL"))
                        continue;

                    string enumType = Helpers.ConvertEnumType(cppMacro.Value, out string csDataType);
                    file.Write($"\t\tpublic const uint {cppMacro.Name} = {cppMacro.Value};\n");
                }
                file.WriteLine("\t}\n}");
            }
        }

        public void GenerateEnums(CppCompilation compilation, string outputPath)
        {
            Debug.WriteLine("Generating Enums...");

            using (StreamWriter file = File.CreateText(Path.Combine(outputPath, "Enums.cs")))
            {
                file.WriteLine("using System;\n");
                file.WriteLine("namespace Evergine.Bindings.MeshOptimizer");
                file.WriteLine("{");

                var enums = compilation.Enums.Where(e => e.Items.Count > 0 && !e.IsAnonymous).ToList();

                foreach (var cppEnum in enums)
                {
                    Helpers.PrintComments(file, cppEnum.Comment, "\t");
                    if (compilation.Typedefs.Any(t => t.Name == cppEnum.Name + "Flags"))
                    {
                        file.WriteLine("\t[Flags]");
                    }

                    file.WriteLine($"\tpublic enum {cppEnum.Name}");
                    file.WriteLine("\t{");

                    foreach (var member in cppEnum.Items)
                    {
                        Helpers.PrintComments(file, member.Comment, "\t\t", true);
                        file.WriteLine($"\t\t{member.Name} = {member.Value},");
                    }

                    file.WriteLine("\t}\n");
                }

                file.WriteLine("}");
            }
        }

        private void GenerateStructs(CppCompilation compilation, string outputPath)
        {
            Debug.WriteLine("Generating Structs...");

            using (StreamWriter file = File.CreateText(Path.Combine(outputPath, "Structs.cs")))
            {
                file.WriteLine("using System;");
                file.WriteLine("using System.Runtime.InteropServices;\n");
                file.WriteLine("namespace Evergine.Bindings.MeshOptimizer");
                file.WriteLine("{");

                var structs = compilation.Classes.Where(c => c.ClassKind == CppClassKind.Struct && c.IsDefinition == true);

                foreach (var structure in structs)
                {
                    Helpers.PrintComments(file, structure.Comment, "\t");
                    file.WriteLine("\t[StructLayout(LayoutKind.Sequential)]");
                    file.WriteLine($"\tpublic unsafe struct {structure.Name}");
                    file.WriteLine("\t{");
                    foreach (var member in structure.Fields)
                    {
                        Helpers.PrintComments(file, member.Comment, "\t\t", true);
                        string type = Helpers.ConvertToCSharpType(member.Type);
                        type = Helpers.ShowAsMarshalType(type, Helpers.Family.field);
                        // Check if this is an array
                        if (member.Type is CppArrayType)
                        {
                            int count = (member.Type as CppAst.CppArrayType).Size;
                            file.WriteLine($"\t\tpublic fixed {type} {member.Name}[{count}];");
                        }
                        else // default case
                        {

                            file.WriteLine($"\t\tpublic {type} {member.Name};");
                        }
                    }

                    file.WriteLine("\t}\n");
                }
                file.WriteLine("}\n");
            }
        }

        private void GenerateFuntions(CppCompilation compilation, string outputPath)
        {
            Debug.WriteLine("Generating Functions...");

            using (StreamWriter file = File.CreateText(Path.Combine(outputPath, "Funtions.cs")))
            {
                file.WriteLine("using System;");
                file.WriteLine("using System.Runtime.InteropServices;\n");
                file.WriteLine($"namespace Evergine.Bindings.MeshOptimizer");
                file.WriteLine("{");
                file.WriteLine($"\tpublic static unsafe partial class MeshOptNative");
                file.WriteLine("\t{");

                foreach (var cppFunction in compilation.Functions)
                {
                    if ((cppFunction.Flags & CppFunctionFlags.FunctionTemplate) != CppFunctionFlags.None) continue;
                    if ((cppFunction.Flags & CppFunctionFlags.Inline) != CppFunctionFlags.None) continue;
                    if (cppFunction.Name == "meshopt_setAllocator") continue;

                    Helpers.PrintComments(file, cppFunction.Comment, "\t\t");
                    file.WriteLine($"\t\t[DllImport(\"meshoptimizer\", CallingConvention = CallingConvention.Cdecl)]");
                    string returnType = Helpers.ConvertToCSharpType(cppFunction.ReturnType);
                    returnType = Helpers.ShowAsMarshalType(returnType, Helpers.Family.ret);
                    file.Write($"\t\tpublic static extern {returnType} {cppFunction.Name}(");
                    foreach (var parameter in cppFunction.Parameters)
                    {
                        if (parameter != cppFunction.Parameters.First())
                            file.Write(", ");

                        var convertedType = Helpers.ConvertToCSharpType(parameter.Type);
                        convertedType = Helpers.ShowAsMarshalType(convertedType, Helpers.Family.param);
                        file.Write($"{convertedType} {parameter.Name}");
                    }
                    file.WriteLine(");\n");
                }
                file.WriteLine("\t}\n}");
            }
        }
    }
}
