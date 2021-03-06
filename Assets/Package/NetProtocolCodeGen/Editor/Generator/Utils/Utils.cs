using System;
using System.IO;
using System.Text;

namespace NetProtocolCodeGen.Editor.Generator.Utils
{
    public static class Utils
    {
        /// <summary>Converts a .Net type name to a C# type name. It will remove the "System." namespace, if present,</summary>
        public static string FromDotNetTypeToCSharpType(this string dotNetTypeName, bool isNull = false)
        {
            var cstype = "";
            var nullable = isNull ? "?" : "";
            var prefix = "System.";
            var typeName = dotNetTypeName.StartsWith(prefix) ? dotNetTypeName.Remove(0, prefix.Length) : dotNetTypeName;

            switch (typeName)
            {
                case "Boolean": cstype = "bool"; break;
                case "Byte":    cstype = "byte"; break;
                case "SByte":   cstype = "sbyte"; break;
                case "Char":    cstype = "char"; break;
                case "Decimal": cstype = "decimal"; break;
                case "Double":  cstype = "double"; break;
                case "Single":  cstype = "float"; break;
                case "Int32":   cstype = "int"; break;
                case "UInt32":  cstype = "uint"; break;
                case "Int64":   cstype = "long"; break;
                case "UInt64":  cstype = "ulong"; break;
                case "Object":  cstype = "object"; break;
                case "Int16":   cstype = "short"; break;
                case "UInt16":  cstype = "ushort"; break;
                case "String":  cstype = "string"; break;

                default: cstype = typeName; break; // do nothing
            }
            return $"{cstype}{nullable}";

        }
        
        public static Type GetTypeFromAnyAssembly(string fullName)
        {
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var t in a.GetTypes())
                {
                    if (t.FullName != null && t.FullName.Equals(fullName))
                        return t;
                }
            }

            return null;
        }
        
        public static string FirstCharToLower(this string source) => source.Substring(0, 1).ToLower() + source.Substring(1);
        
        public static string FirstCharToUpper(this string source) => source.Substring(0, 1).ToUpper() + source.Substring(1);
        
        public static FileInfo GetImplFileInfo(DirectoryInfo directory, string name)
        {
            var fileName = name + ".cs";
            return new FileInfo(Path.Combine(directory.FullName, fileName));
        }
        
        public static string ToMethodName(this string value)
        {
            var tmp = value.Split('_');
            var sb = new StringBuilder();
            foreach (var temp in tmp)
            {
                sb.Append(temp.FirstCharToUpper());
            }
            return sb.ToString();
        }
        
        public static string Comment 
            => $@"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by ProtocolGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
        ";
    }
}