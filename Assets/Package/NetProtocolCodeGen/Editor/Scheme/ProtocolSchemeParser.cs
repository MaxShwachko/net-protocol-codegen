using System.Collections.Generic;
using Newtonsoft.Json;

namespace NetProtocolCodeGen.Editor.Scheme
{
    public class ProtocolSchemeParser
    {
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            Converters = { new MethodJsonConverter() }
        };
        
        public List<MethodScheme> ParseMethods(string json)
        {
            var res = JsonConvert.DeserializeObject<List<MethodScheme>>(json, _jsonSettings);
            return res;
        }
    }
}