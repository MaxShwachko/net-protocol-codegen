using System;
using System.Collections.Generic;
using System.Linq;

namespace NetProtocolCodeGen.Editor.Scheme
{

    [Serializable]
    public class MethodScheme
    {
        public string agent;
        public string method;
        public byte byteMethod;
        public byte byteAgent;
        public List<Parameter> parameters;
        public List<Return> returns;
        
        
        public bool HasEnums
        {
            get
            {
                var parameterOrReturns = new List<AParameterOrReturn>();
                parameterOrReturns.AddRange(parameters);
                parameterOrReturns.AddRange(returns);
                return parameterOrReturns.Any(parameterOrReturn => parameterOrReturn.type.Equals("enum"));
            }
        }
    }
    
    
    [Serializable]
    public class Parameter : AParameterOrReturn
    {

    }
    
    [Serializable]
    public class Return : AParameterOrReturn
    {

    }

    [Serializable]
    public abstract class AParameterOrReturn
    {
        public string name;
        public string type;
        public int size = -1;
        public string itemsType;
        public bool required;

        public List<string> allowedValues;
    } 
}