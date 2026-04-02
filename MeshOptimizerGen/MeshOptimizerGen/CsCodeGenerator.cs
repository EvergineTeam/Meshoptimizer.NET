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
            Helpers.TypedefList = compilation.Typedefs
                .Where(t => t.TypeKind == CppTypeKind.Typedef
                       && t.ElementType is CppPointerType
                       && ((CppPointerType)t.ElementType).ElementType.TypeKind != CppTypeKind.Function)
                .Select(t => t.Name).ToList();

            GenerateConstants(compilation, outputPath);
            GenerateEnums(compilation, outputPath);
            GenerateStructs(compilation, outputPath);
            GenerateFunctions(compilation, outputPath);
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
                file.WriteLine($"\tpublic static partial class MeshOptimizer");
                file.WriteLine("\t{");

                // Constants from #define macros
                foreach (var cppMacro in compilation.Macros)
                {
                    if (string.IsNullOrEmpty(cppMacro.Value)
                        || cppMacro.Name.Equals("MESHOPTIMIZER_ALLOC_CALLCONV")
                        || cppMacro.Name.Equals("MESHOPTIMIZER_EXPERIMENTAL"))
                        continue;

                    string csName = Helpers.StripPrefix(cppMacro.Name);
                    string enumType = Helpers.ConvertEnumType(cppMacro.Value, out string csDataType);
                    file.Write($"\t\tpublic const uint {csName} = {cppMacro.Value};\n");
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

                    string csEnumName = Helpers.StripPrefix(cppEnum.Name);
                    file.WriteLine($"\tpublic enum {csEnumName}");
                    file.WriteLine("\t{");

                    // Find common prefix among enum values for stripping
                    var valueNames = cppEnum.Items.Select(i => i.Name).ToList();
                    string commonPrefix = Helpers.FindCommonPrefix(valueNames);

                    foreach (var member in cppEnum.Items)
                    {
                        Helpers.PrintComments(file, member.Comment, "\t\t", true);

                        string valueName = member.Name;
                        // Strip common prefix
                        if (!string.IsNullOrEmpty(commonPrefix) && valueName.StartsWith(commonPrefix))
                        {
                            valueName = valueName.Substring(commonPrefix.Length);
                        }

                        // Convert to PascalCase if it's SCREAMING_CASE
                        valueName = Helpers.ScreamingToPascalCase(valueName);

                        file.WriteLine($"\t\t{valueName} = {member.Value},");
                    }

                    file.WriteLine("\t}\n");
                }

                // Anonymous enums → [Flags] enums with names derived from common prefix
                var anonymousEnums = compilation.Enums.Where(e => e.IsAnonymous && e.Items.Count > 0);
                foreach (var anonEnum in anonymousEnums)
                {
                    var valueNames = anonEnum.Items.Select(i => i.Name).ToList();
                    string commonPrefix = Helpers.FindCommonPrefix(valueNames);

                    // Derive enum name from common prefix (e.g. "meshopt_Simplify" → "SimplifyOptions")
                    string enumName = Helpers.StripPrefix(commonPrefix.TrimEnd('_'));
                    if (string.IsNullOrEmpty(enumName))
                        enumName = "AnonymousFlags";
                    enumName += "Options";

                    Helpers.PrintComments(file, anonEnum.Comment, "\t");
                    file.WriteLine("\t[Flags]");
                    file.WriteLine($"\tpublic enum {enumName} : uint");
                    file.WriteLine("\t{");

                    foreach (var item in anonEnum.Items)
                    {
                        Helpers.PrintComments(file, item.Comment, "\t\t", true);

                        string valueName = item.Name;
                        if (!string.IsNullOrEmpty(commonPrefix) && valueName.StartsWith(commonPrefix))
                        {
                            valueName = valueName.Substring(commonPrefix.Length);
                        }
                        else
                        {
                            valueName = Helpers.StripPrefix(valueName);
                        }

                        valueName = Helpers.ScreamingToPascalCase(valueName);
                        file.WriteLine($"\t\t{valueName} = {item.Value},");
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

                    bool isUnion = structure.ClassKind == CppClassKind.Union;
                    if (isUnion)
                        file.WriteLine("\t[StructLayout(LayoutKind.Explicit)]");
                    else
                        file.WriteLine("\t[StructLayout(LayoutKind.Sequential)]");

                    string csStructName = Helpers.StripPrefix(structure.Name);
                    file.WriteLine($"\tpublic unsafe struct {csStructName}");
                    file.WriteLine("\t{");
                    foreach (var member in structure.Fields)
                    {
                        Helpers.PrintComments(file, member.Comment, "\t\t", true);
                        string type = Helpers.ConvertToCSharpType(member.Type);
                        type = Helpers.ShowAsMarshalType(type, Helpers.Family.field);
                        string fieldName = Helpers.PascalCaseField(member.Name);
                        fieldName = Helpers.EscapeReservedKeyword(fieldName);

                        if (isUnion)
                            file.Write("\t\t[FieldOffset(0)] ");
                        else
                            file.Write("\t\t");

                        // Check if this is an array
                        if (member.Type is CppArrayType)
                        {
                            int count = (member.Type as CppAst.CppArrayType).Size;
                            file.WriteLine($"public fixed {type} {fieldName}[{count}];");
                        }
                        else // default case
                        {
                            file.WriteLine($"public {type} {fieldName};");
                        }
                    }

                    file.WriteLine("\t}\n");
                }
                file.WriteLine("}\n");
            }
        }

        private void GenerateFunctions(CppCompilation compilation, string outputPath)
        {
            Debug.WriteLine("Generating Functions...");

            using (StreamWriter file = File.CreateText(Path.Combine(outputPath, "Functions.cs")))
            {
                file.WriteLine("using System;");
                file.WriteLine("using System.Runtime.InteropServices;\n");
                file.WriteLine($"namespace Evergine.Bindings.MeshOptimizer");
                file.WriteLine("{");
                file.WriteLine($"\tpublic static unsafe partial class MeshOptimizer");
                file.WriteLine("\t{");

                foreach (var cppFunction in compilation.Functions)
                {
                    if ((cppFunction.Flags & CppFunctionFlags.FunctionTemplate) != CppFunctionFlags.None) continue;
                    if ((cppFunction.Flags & CppFunctionFlags.Inline) != CppFunctionFlags.None) continue;
                    if (cppFunction.Name == "meshopt_setAllocator") continue;

                    string originalName = cppFunction.Name;
                    string csName = Helpers.StripPrefix(originalName);
                    // Capitalize first letter for PascalCase method name
                    csName = char.ToUpperInvariant(csName[0]) + csName.Substring(1);

                    Helpers.PrintComments(file, cppFunction.Comment, "\t\t");
                    file.WriteLine($"\t\t[DllImport(\"meshoptimizer\", EntryPoint = \"{originalName}\", CallingConvention = CallingConvention.Cdecl)]");
                    string returnType = Helpers.ConvertToCSharpType(cppFunction.ReturnType);
                    returnType = Helpers.ShowAsMarshalType(returnType, Helpers.Family.ret);
                    file.Write($"\t\tpublic static extern {returnType} {csName}(");
                    foreach (var parameter in cppFunction.Parameters)
                    {
                        if (parameter != cppFunction.Parameters.First())
                            file.Write(", ");

                        var convertedType = Helpers.ConvertToCSharpType(parameter.Type);
                        convertedType = Helpers.ShowAsMarshalType(convertedType, Helpers.Family.param);
                        string paramName = Helpers.EscapeReservedKeyword(parameter.Name);
                        file.Write($"{convertedType} {paramName}");
                    }
                    file.WriteLine(");\n");
                }
                file.WriteLine("\t}\n}");
            }
        }
    }
}
