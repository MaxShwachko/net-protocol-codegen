using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetProtocolCodeGen.Editor.Generator.Utils;
using NetProtocolCodeGen.Editor.Scheme;

namespace NetProtocolCodeGen.Editor.Generator.Method.Result
{
    public static class ResultReaderStructTemplate
    {
        public static StructDeclarationSyntax Create(List<Return> returns, Lang lang)
        {
            var structDeclaration = SyntaxFactory.StructDeclaration("ResultReader")
                .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IMethodResultReader")));

            var members = new List<MemberDeclarationSyntax>();

            var composeMethod = CreateReadResult(returns, lang);
            members.Add(composeMethod);

            structDeclaration = structDeclaration.AddMembers(members.ToArray())
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                    SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));

            return structDeclaration;

        }

        private static MethodDeclarationSyntax CreateReadResult(List<Return> returns, Lang lang)
        {
            var parameterSyntaxes = new List<ParameterSyntax>
            {
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("reader"))
                    .WithType(SyntaxFactory.ParseTypeName("ByteReader"))
            };

            var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.ParseTypeName("IMethodResult"),
                    SyntaxFactory.Identifier("ReadResult"))

                .AddParameterListParameters(parameterSyntaxes.ToArray())
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            var body = SyntaxFactory.Block();
            var statements = new List<StatementSyntax>();
            var sb = new StringBuilder();
            var paramList = new List<string>();
            foreach (var rReturn in returns)
            {
                sb.Clear();

                var isArray = !string.IsNullOrEmpty(rReturn.itemsType);
                var isEnum = rReturn.type.Equals("enum");
                
                if (isArray)
                {
                    var cSharpType = rReturn.itemsType.FromTypeAndSizeToCSharpType(rReturn.size, false, lang);
                    var counterName = $"{rReturn.name}Count";
                    var readCountSt = SyntaxFactory.ParseStatement($"var {counterName} = reader.ReadByte();");
                    var initArray = SyntaxFactory.ParseStatement($"var {rReturn.name} = new {cSharpType}[{counterName}];");
                    var readMethodStr = rReturn.itemsType.ReadMethodFromTypeAndSizeToType(rReturn.size, false, lang);
                    var forStatement = Helpers.CreateReadForArray(rReturn.name, counterName, readMethodStr);
                    statements.Add(readCountSt);
                    statements.Add(initArray);
                    statements.Add(forStatement);
                    paramList.Add(rReturn.name);
                }
                else if (isEnum)
                {
                    sb.Append("var ").Append(rReturn.name).Append(" = reader.ReadString();");
                    var paramName = rReturn.name + "Val";
                    var enumName = "E" + rReturn.name.FirstCharToUpper();
                    var readSt = SyntaxFactory.ParseStatement(sb.ToString());
                    var convertSt = SyntaxFactory.ParseStatement($"var {paramName} = ({enumName}) Enum.Parse(typeof({enumName}), {rReturn.name});");
                    statements.Add(readSt);
                    statements.Add(convertSt);
                    paramList.Add(paramName);
                }
                else
                {
                    var readMethodStr = rReturn.type.ReadMethodFromTypeAndSizeToType(rReturn.size, !rReturn.required, lang);
                    sb.Append("var ").Append(rReturn.name).Append(" = reader.").Append(readMethodStr).Append("();");
                    paramList.Add(rReturn.name);
                    var st = SyntaxFactory.ParseStatement(sb.ToString());
                    statements.Add(st);
                }
                
 
            }

            sb.Clear();
            sb.Append("var result = new Result(");
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
            body = body.AddStatements(SyntaxFactory.ParseStatement("return result;"));

            method = method.WithBody(body);

            return method;
        }
        
    }
}