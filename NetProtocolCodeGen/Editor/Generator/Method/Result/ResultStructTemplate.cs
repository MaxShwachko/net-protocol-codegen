using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetProtocolCodeGen.Editor.Generator.Utils;
using NetProtocolCodeGen.Editor.Scheme;

namespace NetProtocolCodeGen.Editor.Generator.Method.Result
{
    public static class ResultStructTemplate
    {
        public static StructDeclarationSyntax Create(string methodName, List<Return> returns, Lang lang)
        {
            var structDeclaration = SyntaxFactory.StructDeclaration("Result")
                .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IMethodResult")));

            var members = new List<MemberDeclarationSyntax>();
            foreach (var rReturn in returns)
            {
                string cSharpType;
                var isArray = rReturn.type.Equals("array");
                var isEnum = rReturn.type.Equals("enum");
                if (isArray)
                {
                    cSharpType = rReturn.itemsType.FromTypeAndSizeToCSharpType(rReturn.size, false, lang);
                }
                else if (isEnum)
                {
                    var type = "E" + rReturn.name.FirstCharToUpper();
                    cSharpType = !rReturn.required ? $"{type}?" : type;
                }
                else
                {
                    cSharpType = rReturn.type.FromTypeAndSizeToCSharpType(rReturn.size, !rReturn.required, lang);
                }
                var field = Helpers.GenerateReadonlyPublicField(rReturn.name, cSharpType, isArray);
                members.Add(field);
            }
            
            if (returns.Count > 0)
            {
                var ctor = CreateConstructor(returns, lang);
                members.Add(ctor);   
            }

            var idField = GenerateProperty("Id", methodName);
            var agentIdField = GenerateProperty("AgentId", methodName);
            var getBytesMethod = GenerateGetBytesMethod(returns);
            
            members.Add(idField);
            members.Add(agentIdField);
            members.Add(getBytesMethod);
            
            structDeclaration = structDeclaration.AddMembers(members.ToArray())
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));

            return structDeclaration;
        }

        private static ConstructorDeclarationSyntax CreateConstructor(List<Return> returns, Lang lang)
        {
            var ctorParameters = new SeparatedSyntaxList<ParameterSyntax>();
            
            var body = SyntaxFactory.Block();
            var bodyStatements = new List<StatementSyntax>();
            
            foreach (var rReturn in returns)
            {
                string cSharpType;
                var isArray = rReturn.type.Equals("array");
                var isEnum = rReturn.type.Equals("enum");
                if (isArray)
                {
                    cSharpType = rReturn.itemsType.FromTypeAndSizeToCSharpType(rReturn.size, false, lang);
                    cSharpType += "[]";
                }
                else if (isEnum)
                {
                    var type = "E" + rReturn.name.FirstCharToUpper();
                    cSharpType = !rReturn.required ? $"{type}?" : type;
                }
                else
                {
                    cSharpType = rReturn.type.FromTypeAndSizeToCSharpType(rReturn.size, !rReturn.required, lang);
                }
                var ctorParam = SyntaxFactory.Parameter(SyntaxFactory.Identifier(rReturn.name))
                    .WithType(SyntaxFactory.ParseTypeName(cSharpType));
                ctorParameters = ctorParameters.Add(ctorParam);

                var bodySt = SyntaxFactory.ParseStatement(rReturn.name.FirstCharToUpper() + " = " + rReturn.name + ";");
                bodyStatements.Add(bodySt);
            }

            body = body.AddStatements(bodyStatements.ToArray());
            
            var ctor = SyntaxFactory.ConstructorDeclaration("Result")
                .WithParameterList(SyntaxFactory.ParameterList(ctorParameters))
                .WithBody(body)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            return ctor;
        }

        private static PropertyDeclarationSyntax GenerateProperty(string getterName, string methodName)
        {
            var property = SyntaxFactory.PropertyDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.ByteKeyword)),
                    SyntaxFactory.Identifier(getterName))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithExpressionBody(
                    SyntaxFactory.ArrowExpressionClause(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(methodName),
                            SyntaxFactory.IdentifierName(getterName))))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            return property;
        }

        private static MethodDeclarationSyntax GenerateGetBytesMethod(List<Return> returns)
        {
            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.ArrayType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ByteKeyword)))
                    .WithRankSpecifiers( SyntaxFactory.SingletonList(
                        SyntaxFactory.ArrayRankSpecifier(
                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                SyntaxFactory.OmittedArraySizeExpression()))))
                    , SyntaxFactory.Identifier("GetBytes"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            var body = SyntaxFactory.Block();
            var statements = new List<StatementSyntax>();
            
            if (returns.Count == 0)
            {
                var st = SyntaxFactory.ParseStatement("return new byte[]{};");
                statements.Add(st);
            }
            else
            {
                var writerSt = SyntaxFactory.ParseStatement("var writer = ByteWriterPool.Instance.Get();");
                statements.Add(writerSt);
                foreach (var rReturn in returns)
                {
                    switch ( rReturn.type)
                    {
                        case "Position":
                            statements.Add(SyntaxFactory.ParseStatement(rReturn.name.FirstCharToUpper() +
                                                                        ".ComposePosition(writer);"));
                            break;
                        case "array":
                            var writeCountSt = SyntaxFactory.ParseStatement($"writer.Write((byte){rReturn.name.FirstCharToUpper()}.Length);");
                            var forStatement = Helpers.CreateWriteForArray(rReturn.name.FirstCharToUpper());
                            statements.Add(writeCountSt);
                            statements.Add(forStatement);
                            break;
                        case "enum":
                            statements.Add(SyntaxFactory.ParseStatement("writer.Write(" + rReturn.name.FirstCharToUpper() + ".ToString());"));
                            break;
                        default:
                            statements.Add(SyntaxFactory.ParseStatement("writer.Write(" + rReturn.name.FirstCharToUpper() + ");"));
                            break;
                    }
                    
                    
                }
                statements.Add(SyntaxFactory.ParseStatement("var bytes =  writer.ToArray();"));
                statements.Add(SyntaxFactory.ParseStatement("ByteWriterPool.Instance.Return(writer);"));
                statements.Add(SyntaxFactory.ParseStatement("return bytes;"));
            }
            
            body = body.AddStatements(statements.ToArray());
            method = method.WithBody(body);
            return method;
        }

    }
}