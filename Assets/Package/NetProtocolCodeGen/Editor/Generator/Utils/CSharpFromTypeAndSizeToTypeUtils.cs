using System;

namespace NetProtocolCodeGen.Editor.Generator.Utils
{
    public static class CSharpFromTypeAndSizeToTypeUtils
    {
        public static string ReadMethodFromTypeAndSizeToType(this string type, int size, bool isNull, Lang lang)
        {
            switch (lang)
            {
                case Lang.CSharp:
                    return CSharpReadMethodFromTypeAndSizeToCSharpType(type, isNull);
                case Lang.Javascript:
                    return JsReadMethodFromTypeAndSizeToReadMethod(type, size, isNull);
                default:
                    throw new ArgumentOutOfRangeException(nameof(lang), lang, null);
            }
        }
        
        private static string CSharpReadMethodFromTypeAndSizeToCSharpType(string type, bool isNull)
        {
            string read = null;

            switch (type)
            {
                case "string": 
                    read = "ReadString";
                    break;
                case "byte":
                    read = "ReadByte";
                    break;
                case "short":
                    read = "ReadInt16";
                    break;
                case "int":
                    read = "ReadInt32";
                    break;
                case "long":
                    read = "ReadInt64";
                    break;
                case "float":
                    read = "ReadSingle";
                    break;
                case "double":
                    break;
                case "bool":    
                    read = "ReadBoolean"; 
                    break;
                case "Position":    
                    read = "ReadPositionFromBytes";
                    break;
                default: throw new ArgumentException("type " + type + " is invalid.");
            }

            var res = read;  
            if (type != "string")
            {
                res = isNull ? read + "Nullable" : read;    
            }
            return res;
        }
        
        private static string JsReadMethodFromTypeAndSizeToReadMethod(string type, int size, bool isNull = false)
        {
            string read;

            switch (type)
            {
                case "string": 
                    read = "ReadString";
                    break;
                case "number":
                    if (size == -1)
                        throw new ArgumentException("type is number, but size is null");
                    switch (size)
                    {
                        case 1:
                            read = "ReadByte";
                            break;
                        case 2:
                            read = "ReadInt16";
                            break;
                        case 4:
                            read = "ReadInt32";
                            break;
                        case 8:
                            read = "ReadInt64";
                            break;
                        default: throw new ArgumentException("size " + size + " is invalid.");
                    }
                    break;
                case "bool":    
                    read = "ReadBoolean"; 
                    break;
                case "Position":    
                    read = "ReadPositionFromBytes"; 
                    break;
                default: throw new ArgumentException("type " + type + " is invalid.");
            }

            var res = read;  
            if (type != "string")
            {
                res = isNull ? read + "Nullable" : read;    
            }
            return res;
        }
    }
    
}