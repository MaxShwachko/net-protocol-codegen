using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetProtocolCodeGen.Editor.Scheme
{
    public class MethodJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var methodScheme = new MethodScheme
            {
                parameters = new List<Parameter>(),
                returns = new List<Return>()
            };
            var item = JObject.Load(reader);
            if (item["agent"] != null)
            {
                methodScheme.agent = item["agent"].ToObject<string>();
            }
            if (item["method"] != null)
            {
                methodScheme.method = item["method"].ToObject<string>();
            }
            if (item["byteAgent"] != null)
            {
                methodScheme.byteAgent = item["byteAgent"].ToObject<byte>();
            }
            if (item["byteMethod"] != null)
            {
                methodScheme.byteMethod = item["byteMethod"].ToObject<byte>();
            }
            
            var parameters = item["params"];
            
                //======= properties ======
            var paramProperties = parameters?["properties"];
            if (parameters != null && paramProperties != null)
            {
                foreach (var property in paramProperties)
                {
                    var tmp = property.Path.Split('.');
                    var parameter = new Parameter
                    {
                        name = tmp[tmp.Length - 1],
                        required = false
                    };
                    var children = property.Children();
                    var allowedValues = GetAllowedValues(children);
                    if (allowedValues != null)
                    {
                        parameter.type = "enum";
                        parameter.allowedValues = allowedValues;
                    }
                    else
                    {
                        FillParameterAndReturn(parameter, children);   
                    }
                    methodScheme.parameters.Add(parameter);
                }
            }
            
            //======= required parameters ======
            var paramRequired = parameters?["required"]?.ToObject<List<string>>();
            if (paramRequired != null)
            {
                foreach (var parameter in methodScheme.parameters)
                {
                    if (paramRequired.Contains(parameter.name))
                    {
                        parameter.required = true;
                    }
                }    
            }

            //======= returns ======
            var returns = item["returns"];
            var returnsProperties = returns?["properties"];
            if (returns != null && returnsProperties != null)
            {
                foreach (var r in returnsProperties)
                {
                    var tmp = r.Path.Split('.');
                    var rReturn = new Return
                    {
                        name = tmp[tmp.Length - 1]
                    };
                    var children = r.Children();
                    var allowedValues = GetAllowedValues(children);
                    if (allowedValues != null)
                    {
                        rReturn.type = "enum";
                        rReturn.allowedValues = allowedValues;
                    }
                    else
                    {
                        FillParameterAndReturn(rReturn, children);   
                    }
                    methodScheme.returns.Add(rReturn);
                }   
            }
            
            //======= required returns ======
            var returnRequired = returns?["required"]?.ToObject<List<string>>();
            if (returnRequired != null)
            {
                foreach (var eReturn in methodScheme.returns)
                {
                    if (returnRequired.Contains(eReturn.name))
                    {
                        eReturn.required = true;
                    }
                }    
            }

            return methodScheme;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(MethodScheme);
        }


        private void FillParameterAndReturn(AParameterOrReturn parameterOrReturn, JEnumerable<JToken> children)
        {
            foreach (var child in children)
            {
                if (child["type"] != null)
                {
                    var type = child["type"].ToObject<string>();
                    if (type != null && type.Equals("array"))
                    {
                        parameterOrReturn.type = "array";
                        var items = child["items"];
                        if (items?["type"] != null)
                        {
                            parameterOrReturn.itemsType = items["type"].ToObject<string>();
                        }

                        if (items?["size"] != null)
                        {
                            parameterOrReturn.size = items["size"].ToObject<int>();
                        }
                        else
                        {
                            parameterOrReturn.size = -1;
                        }
                    }
                    else
                    {
                        parameterOrReturn.type = type;
                    }
                }

                if (child["size"] != null)
                {
                    parameterOrReturn.size = child["size"].ToObject<int>();
                }
                
                
            }
        }

        private List<string> GetAllowedValues(JEnumerable<JToken> children)
        {
            foreach (var child in children)
            {
                if (child["allowedValues"] != null)
                {
                    var values = new List<string>();
                    foreach (var valToken in child["allowedValues"])
                    {
                        var val = valToken.ToObject<string>();
                        values.Add(val);
                    }
                    return values;
                } 
            }
            return null;
        }
        
    }
}