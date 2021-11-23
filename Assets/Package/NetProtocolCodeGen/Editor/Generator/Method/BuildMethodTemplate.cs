using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetProtocolCodeGen.Editor.Generator.Utils;
using NetProtocolCodeGen.Editor.Scheme;

namespace NetProtocolCodeGen.Editor.Generator.Method
{
    public static class BuildMethodTemplate
    {
        public static MethodDeclarationSyntax Create(List<Parameter> parameters, Lang lang)
        {
            var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.ParseTypeName("IMethod"),
                    SyntaxFactory.Identifier("Build")
                )
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword));


            var parameterSyntaxes = new List<ParameterSyntax>();
            var parametersStr = new StringBuilder();
            var counter = 0;
            foreach (var parameter in parameters)
            {
                string cSharpType;
                var isArray = parameter.type.Equals("array");
                var isEnum = parameter.type.Equals("enum");
                if (isArray)
                {
                    cSharpType = parameter.itemsType.FromTypeAndSizeToCSharpType(parameter.size, false, lang);
                }
                else if (isEnum)
                {
                    var type = "E" + parameter.name.FirstCharToUpper();
                    cSharpType = !parameter.required ? $"{type}?" : type;
                }
                else
                {
                    cSharpType = parameter.type.FromTypeAndSizeToCSharpType(parameter.size, !parameter.required, lang);
                }
                
                var parameterSyntax = isArray 
                    ? SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameter.name)).WithType(SyntaxFactory.ParseTypeName($"{cSharpType}[]")) 
                    : SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameter.name)).WithType(SyntaxFactory.ParseTypeName(cSharpType));

                parameterSyntaxes.Add(parameterSyntax);
                parametersStr.Append(parameter.name);

                counter++;
                if (counter < parameters.Count)
                {
                    parametersStr.Append(", ");
                }
            }
            method = method.AddParameterListParameters(parameterSyntaxes.ToArray());
            var body = SyntaxFactory.Block();
            body = body.AddStatements(
                SyntaxFactory.ParseStatement("var parameters = new Parameters(" + parametersStr + ");"),
                SyntaxFactory.ParseStatement("return new Method(Id, AgentId, parameters);")
            );

            method = method.WithBody(body);
            
            return method;
        }
        
        
    }
}