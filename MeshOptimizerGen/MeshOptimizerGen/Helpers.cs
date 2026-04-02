using CppAst;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshOptimizerGen
{
    public static class Helpers
    {
        public static List<string> TypedefList = new List<string>();

        private static readonly Dictionary<string, string> csNameMappings = new Dictionary<string, string>()
        {
            { "bool", "byte" },
            { "uint8_t", "byte" },
            { "uint16_t", "ushort" },
            { "uint32_t", "uint" },
            { "uint64_t", "ulong" },
            { "int8_t", "sbyte" },
            { "int32_t", "int" },
            { "int16_t", "short" },
            { "int64_t", "long" },
            { "int64_t*", "long*" },
            { "char", "byte" },
            { "size_t", "nuint" },
            { "intptr_t", "nint" },
            { "uintptr_t", "nuint" },
        };

        public static string ConvertToCSharpType(CppType type, bool isPointer = false)
        {
            if (type is CppPrimitiveType primitiveType)
            {
                return GetCsTypeName(primitiveType, isPointer);
            }

            if (type is CppQualifiedType qualifiedType)
            {
                return GetCsTypeName(qualifiedType.ElementType, isPointer);
            }

            if (type is CppEnum enumType)
            {
                var enumCsName = GetCsCleanName(enumType.Name);
                if (isPointer)
                    return enumCsName + "*";

                return enumCsName;
            }

            if (type is CppTypedef typedef)
            {
                var originalName = typedef.Name;
                csNameMappings.TryGetValue(originalName, out string typeDefCsName);

                if (typeDefCsName == null)
                    typeDefCsName = GetCsCleanName(originalName);

                if (isPointer)
                    return typeDefCsName + "*";

                return typeDefCsName;
            }

            if (type is CppClass @class)
            {
                var className = StripPrefix(@class.Name);
                if (isPointer)
                    return className + "*";

                return className;
            }

            if (type is CppPointerType pointerType)
            {
                return GetCsTypeName(pointerType);
            }

            if (type is CppFunctionType functionType)
            {
                return GetCsFunctionPointerType(functionType);
            }

            if (type is CppArrayType arrayType)
            {
                return GetCsTypeName(arrayType.ElementType, isPointer);
            }

            return string.Empty;
        }

        private static string GetCsTypeName(CppPointerType pointerType)
        {
            if (pointerType.ElementType is CppQualifiedType qualifiedType)
            {
                if (qualifiedType.ElementType is CppPrimitiveType primitiveType)
                {
                    return GetCsTypeName(primitiveType, true);
                }
                else if (qualifiedType.ElementType is CppClass @classType)
                {
                    return GetCsTypeName(@classType, true);
                }
                else if (qualifiedType.ElementType is CppPointerType subPointerType)
                {
                    return GetCsTypeName(subPointerType, true) + "*";
                }
                else if (qualifiedType.ElementType is CppTypedef typedef)
                {
                    return GetCsTypeName(typedef, true);
                }
                else if (qualifiedType.ElementType is CppEnum @enum)
                {
                    return GetCsTypeName(@enum, true);
                }

                return GetCsTypeName(qualifiedType.ElementType, true);
            }

            if (pointerType.ElementType is CppFunctionType functionType)
            {
                return GetCsFunctionPointerType(functionType);
            }

            return GetCsTypeName(pointerType.ElementType, true);
        }

        private static string GetCsTypeName(CppType type, bool isPointer = false)
        {
            if (type is CppPrimitiveType primitiveType)
            {
                return GetCsTypeName(primitiveType, isPointer);
            }

            if (type is CppQualifiedType qualifiedType)
            {
                return GetCsTypeName(qualifiedType.ElementType, isPointer);
            }

            if (type is CppEnum enumType)
            {
                var enumCsName = GetCsCleanName(enumType.Name);
                if (isPointer)
                    return enumCsName + "*";

                return enumCsName;
            }

            if (type is CppTypedef typedef)
            {
                var originalName = typedef.Name;
                csNameMappings.TryGetValue(originalName, out string typeDefCsName);

                if (typeDefCsName == null)
                    typeDefCsName = GetCsCleanName(originalName);

                if (isPointer)
                    return typeDefCsName + "*";

                return typeDefCsName;
            }

            if (type is CppClass @class)
            {
                var className = StripPrefix(@class.Name);

                if (isPointer)
                    className += "*";

                return className;
            }

            if (type is CppPointerType pointerType)
            {
                return GetCsTypeName(pointerType);
            }

            if (type is CppFunctionType functionType)
            {
                return GetCsFunctionPointerType(functionType);
            }

            if (type is CppArrayType arrayType)
            {
                return GetCsTypeName(arrayType.ElementType, isPointer);
            }

            return string.Empty;
        }

        private static string GetCsTypeName(CppPrimitiveType primitiveType, bool isPointer)
        {
            string result = string.Empty;

            switch (primitiveType.Kind)
            {
                case CppPrimitiveKind.Void:
                    result = "void";
                    break;
                case CppPrimitiveKind.Bool:
                    result = "bool";
                    break;
                case CppPrimitiveKind.Char:
                    result = "byte";
                    break;
                case CppPrimitiveKind.WChar:
                    result = "char";
                    break;
                case CppPrimitiveKind.Short:
                    result = "short";
                    break;
                case CppPrimitiveKind.Int:
                    result = "int";
                    break;
                case CppPrimitiveKind.UnsignedShort:
                    result = "ushort";
                    break;
                case CppPrimitiveKind.UnsignedInt:
                    result = "uint";
                    break;
                case CppPrimitiveKind.Float:
                    result = "float";
                    break;
                case CppPrimitiveKind.Double:
                    result = "double";
                    break;
                case CppPrimitiveKind.UnsignedChar:
                    result = "byte";
                    break;
                case CppPrimitiveKind.LongLong:
                    result = "long";
                    break;
                case CppPrimitiveKind.UnsignedLongLong:
                    result = "ulong";
                    break;
                case CppPrimitiveKind.LongDouble:
                    result = "double";
                    break;
                default:
                    break;
            }

            if (isPointer)
            {
                result += "*";
            }

            return result;
        }

        private static string GetCsFunctionPointerType(CppFunctionType functionType)
        {
            var sb = new StringBuilder("delegate* unmanaged[Cdecl]<");

            foreach (var param in functionType.Parameters)
            {
                sb.Append(ConvertToCSharpType(param.Type));
                sb.Append(", ");
            }

            sb.Append(ConvertToCSharpType(functionType.ReturnType));
            sb.Append(">");

            return sb.ToString();
        }

        public static string GetCsCleanName(string name)
        {
            if (name.StartsWith("PFN"))
                return "IntPtr";

            if (name.EndsWith("Flags"))
            {
                var stripped = name.Substring(0, name.Length - "Flags".Length);
                return StripPrefix(stripped);
            }

            if (TypedefList.Contains(name))
                return "IntPtr";

            if (csNameMappings.TryGetValue(name, out string mappedName))
                return mappedName;

            return StripPrefix(name);
        }

        public enum Family
        {
            param,
            field,
            ret,
        }

        public static string ShowAsMarshalType(string type, Family family)
        {
            switch (type)
            {
                case "bool":
                    switch (family)
                    {
                        case Family.param:
                            return "[MarshalAs(UnmanagedType.I1)] bool";
                        case Family.ret:
                            return "bool";
                        case Family.field:
                        default:
                            return "byte";
                    }
                case "bool*":
                    return "byte*";
                case "char*":
                case "unsigned char*":
                    switch (family)
                    {
                        case Family.param:
                            return "[MarshalAs(UnmanagedType.LPStr)] string";
                        case Family.ret:
                            return "byte*";
                        case Family.field:
                        default:
                            return "byte*";
                    }
                default:
                    return type;
            }
        }

        public static string ConvertEnumType(string value, out string csDataType)
        {
            if (value.StartsWith("(") && value.EndsWith(")"))
            {
                value = value.Substring(1, value.Length - 2);
            }

            csDataType = "uint";
            if (value.EndsWith("ULL", StringComparison.OrdinalIgnoreCase))
            {
                csDataType = "ulong";
                value = value.Replace("ULL", String.Empty);
            }

            return value.Replace("UL", String.Empty);
        }

        /// <summary>
        /// Strip the meshoptimizer-specific C prefix from a name.
        /// Handles: meshopt_, MESHOPTIMIZER_, meshopt (for PascalCase-style names).
        /// </summary>
        public static string StripPrefix(string name)
        {
            if (name.StartsWith("meshopt_"))
                return name.Substring("meshopt_".Length);

            if (name.StartsWith("MESHOPTIMIZER_"))
                return name.Substring("MESHOPTIMIZER_".Length);

            return name;
        }

        /// <summary>
        /// Capitalize the first letter of a struct field name (camelCase/snake_case → PascalCase).
        /// Also converts snake_case to PascalCase: "vertex_offset" → "VertexOffset".
        /// Preserves existing uppercase runs: "bodyID" → "BodyID".
        /// </summary>
        public static string PascalCaseField(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            // Handle snake_case: split on underscores and capitalize each segment
            if (name.Contains('_'))
            {
                var parts = name.Split('_');
                var sb = new StringBuilder();
                foreach (var part in parts)
                {
                    if (part.Length == 0) continue;
                    sb.Append(char.ToUpperInvariant(part[0]));
                    sb.Append(part.Substring(1));
                }
                return sb.ToString();
            }

            // Simple camelCase → PascalCase: capitalize first letter
            return char.ToUpperInvariant(name[0]) + name.Substring(1);
        }

        /// <summary>
        /// Find the longest common prefix at word boundaries among a list of names.
        /// Handles both SCREAMING_CASE and PascalCase boundaries.
        /// E.g. given ["meshopt_EncodeExpSeparate", "meshopt_EncodeExpSharedVector"] → "meshopt_EncodeExp".
        /// </summary>
        public static string FindCommonPrefix(IEnumerable<string> names)
        {
            var list = names.ToList();
            if (list.Count == 0) return string.Empty;
            if (list.Count == 1) return string.Empty;

            string first = list[0];
            int prefixLen = first.Length;

            for (int i = 1; i < list.Count; i++)
            {
                prefixLen = Math.Min(prefixLen, list[i].Length);
                for (int j = 0; j < prefixLen; j++)
                {
                    if (first[j] != list[i][j])
                    {
                        prefixLen = j;
                        break;
                    }
                }
            }

            string commonPrefix = first.Substring(0, prefixLen);

            // Check if the prefix already ends at a word boundary:
            // - The next char after the prefix (in any name that's longer) is uppercase or '_'
            bool alreadyAtBoundary = false;
            foreach (var name in list)
            {
                if (name.Length > prefixLen)
                {
                    char next = name[prefixLen];
                    if (char.IsUpper(next) || next == '_')
                    {
                        alreadyAtBoundary = true;
                    }
                    else
                    {
                        alreadyAtBoundary = false;
                    }
                    break;
                }
            }

            if (alreadyAtBoundary)
                return commonPrefix;

            // Trim backward to the nearest word boundary
            for (int i = commonPrefix.Length - 1; i > 0; i--)
            {
                if (commonPrefix[i] == '_')
                    return commonPrefix.Substring(0, i + 1); // Include the underscore

                if (char.IsUpper(commonPrefix[i]) && i > 0 && !char.IsUpper(commonPrefix[i - 1]) && commonPrefix[i - 1] != '_')
                    return commonPrefix.Substring(0, i);
            }

            return commonPrefix;
        }

        /// <summary>
        /// Convert a SCREAMING_CASE identifier to PascalCase.
        /// E.g. "DONT_ACTIVATE" → "DontActivate", "SUCCESS" → "Success".
        /// Numeric-leading segments are preserved as-is.
        /// </summary>
        public static string ScreamingToPascalCase(string screaming)
        {
            if (string.IsNullOrEmpty(screaming))
                return screaming;

            // If the string doesn't contain underscores and isn't all-uppercase, it's already PascalCase
            if (!screaming.Contains('_') && !screaming.All(c => char.IsUpper(c) || char.IsDigit(c)))
                return screaming;

            var parts = screaming.Split('_');
            var sb = new StringBuilder();
            foreach (var part in parts)
            {
                if (part.Length == 0) continue;
                if (char.IsDigit(part[0]))
                {
                    sb.Append(part);
                }
                else
                {
                    sb.Append(char.ToUpperInvariant(part[0]));
                    sb.Append(part.Substring(1).ToLowerInvariant());
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Escape C# reserved keywords by prefixing with @.
        /// </summary>
        public static string EscapeReservedKeyword(string name)
        {
            switch (name)
            {
                case "abstract":
                case "as":
                case "base":
                case "bool":
                case "break":
                case "byte":
                case "case":
                case "catch":
                case "char":
                case "checked":
                case "class":
                case "const":
                case "continue":
                case "decimal":
                case "default":
                case "delegate":
                case "do":
                case "double":
                case "else":
                case "enum":
                case "event":
                case "explicit":
                case "extern":
                case "false":
                case "finally":
                case "fixed":
                case "float":
                case "for":
                case "foreach":
                case "goto":
                case "if":
                case "implicit":
                case "in":
                case "int":
                case "interface":
                case "internal":
                case "is":
                case "lock":
                case "long":
                case "namespace":
                case "new":
                case "null":
                case "object":
                case "operator":
                case "out":
                case "override":
                case "params":
                case "private":
                case "protected":
                case "public":
                case "readonly":
                case "ref":
                case "return":
                case "sbyte":
                case "sealed":
                case "short":
                case "sizeof":
                case "stackalloc":
                case "static":
                case "string":
                case "struct":
                case "switch":
                case "this":
                case "throw":
                case "true":
                case "try":
                case "typeof":
                case "uint":
                case "ulong":
                case "unchecked":
                case "unsafe":
                case "ushort":
                case "using":
                case "virtual":
                case "void":
                case "volatile":
                case "while":
                    return "@" + name;
                default:
                    return name;
            }
        }

        public static void PrintComments(StreamWriter file, CppComment comment, string tabs = "", bool newLine = false)
        {
            if (comment != null)
            {
                if (newLine) file.WriteLine();

                file.WriteLine($"{tabs}/// <summary>");
                GetText(file, comment, tabs);
                file.WriteLine($"{tabs}/// </summary>");
            }
        }

        private static void GetText(StreamWriter file, CppComment comment, string tabs)
        {
            switch (comment.Kind)
            {
                case CppCommentKind.Text:
                    var commentText = comment as CppCommentTextBase;
                    file.WriteLine($"{tabs}/// {commentText.Text}");
                    break;
                case CppCommentKind.Paragraph:
                case CppCommentKind.Full:
                    foreach (var child in comment.Children)
                    {
                        GetText(file, child, tabs);
                    }
                    break;
                default:
                    ;
                    break;
            }
        }
    }
}
