using System;

namespace NetProtocolCodeGen.Editor.Generator.Utils
{
    public static class FromTypeAndSizeToCSharpTypeUtils
    { 
        public static string FromTypeAndSizeToCSharpType(this string type, int size, bool isNull, Lang lang)
        {
            switch (lang)
            {
                case Lang.CSharp:
                    var nullable = isNull ? "?" : "";
                    return type != "string" ? $"{type}{nullable}" : type;
                case Lang.Javascript:
                    return type.JsFromTypeAndSizeToCSharpType(size, isNull); 
                default:
                    throw new ArgumentOutOfRangeException(nameof(lang), lang, null);
            }
            return null;
        }
        
        private static string JsFromTypeAndSizeToCSharpType(this string type, int size, bool isNull)
        {
            string cType;
            var nullable = isNull ? "?" : "";
            
            switch (type)
            {
                case "string": 
                    cType = "string";
                    break;
                case "number":
                    if (size == -1)
                        throw new ArgumentException("type is number, but size is null");
                    switch (size)
                    {
                        case 1:
                            cType = "byte";
                            break;
                        case 2:
                            cType = "short";
                            break;
                        case 4:
                            cType = "int";
                            break;
                        case 8:
                            cType = "long";
                            break;
                        default: throw new ArgumentException("size " + size + " is invalid.");
                    }
                    cType = $"{cType}{nullable}";
                    break;
                case "bool":    
                    cType = "bool"; 
                    cType = $"{cType}{nullable}";
                    break;
                default: throw new ArgumentException("type " + type + " is invalid.");
            }
            return cType;
        }
    }
}