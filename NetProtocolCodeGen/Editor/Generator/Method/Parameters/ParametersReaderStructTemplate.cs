using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetProtocolCodeGen.Editor.Generator.Utils;
using NetProtocolCodeGen.Editor.Scheme;

namespace NetProtocolCodeGen.Editor.Generator.Method.Parameters
{
    public static class ParametersReaderStructTemplate
    {
        public static StructDeclarationSyntax Create(List<Parameter> parameters, Lang lang)
        {
            var structDeclaration = SyntaxFactory.StructDeclaration("ParametersReader")
                .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IMethodParametersReader")));

            var members = new List<MemberDeclarationSyntax>();

            var composeMethod = CreateReadParameters(parameters, lang);
            members.Add(composeMethod);

            structDeclaration = structDeclaration.AddMembers(members.ToArray())
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                    SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));

            return structDeclaration;

        }

        private static MethodDeclarationSyntax CreateReadParameters(List<Parameter> parameters, Lang lang)
        {
            var parameterSyntaxes = new List<ParameterSyntax>
            {
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("reader"))
                    .WithType(SyntaxFactory.ParseTypeName("ByteReader"))
            };

            var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.ParseTypeName("IMethodParameters"),
                    SyntaxFactory.Identifier("ReadParameters"))

                .AddParameterListParameters(parameterSyntaxes.ToArray())
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            var body = SyntaxFactory.Block();
            var statements = new List<StatementSyntax>();
            var sb = new StringBuilder();
            var paramList = new List<string>();
            foreach (var parameter in parameters)
            {
                sb.Clear();
                
                var isArray = !string.IsNullOrEmpty(parameter.itemsType);
                var isEnum = parameter.type.Equals("enum");
                
                if (isArray)
                {
                    var cSharpType = parameter.itemsType.FromTypeAndSizeToCSharpType(parameter.size, false, lang);
                    var counterName = $"{parameter.name}Count";
                    var readCountSt = SyntaxFactory.ParseStatement($"var {counterName} = reader.ReadByte();");
                    var initArray = SyntaxFactory.ParseStatement($"var {parameter.name} = new {cSharpType}[{counterName}];");
                    var readMethodStr = parameter.itemsType.ReadMethodFromTypeAndSizeToType(parameter.size, false, lang);
                    var forStatement = Helpers.CreateReadForArray(parameter.name, counterName, readMethodStr);
                    statements.Add(readCountSt);
                    statements.Add(initArray);
                    statements.Add(forStatement);
                    paramList.Add(parameter.name);
                } 
                else if (isEnum)
                {
                    sb.Append("var ").Append(parameter.name).Append(" = reader.ReadString();");
                    var paramName = parameter.name + "Val";
                    var enumName = "E" + parameter.name.FirstCharToUpper();
                    var readSt = SyntaxFactory.ParseStatement(sb.ToString());
                    var convertSt = SyntaxFactory.ParseStatement($"var {paramName} = ({enumName}) Enum.Parse(typeof({enumName}), {parameter.name});");
                    statements.Add(readSt);
                    statements.Add(convertSt);
                    paramList.Add(paramName);
                }
                else
                {
                    var readMethodStr = parameter.type.ReadMethodFromTypeAndSizeToType(parameter.size, !parameter.required, lang);
                    sb.Append("var ").Append(parameter.name).Append(" = reader.").Append(readMethodStr).Append("();");
                    var st = SyntaxFactory.ParseStatement(sb.ToString());
                    paramList.Add(parameter.name);
                    statements.Add(st);
                }
            }

            sb.Clear();
            sb.Append("var parameters = new Parameters(");
            for (var i = 0; i < paramList.Count; i++)
            {
                var p = paramList[i];
                sb.Append(p);
                if (i < paramList.Count - 1)
                {
                    sb.Append(",");
                }
            }

            sb.Append(");");


            body = body.AddStatements(statements.ToArray());
            body = body.AddStatements(SyntaxFactory.ParseStatement(sb.ToString()));
            body = body.AddStatements(SyntaxFactory.ParseStatement("return parameters;"));

            method = method.WithBody(body);

            return method;
        }
    }
}