using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetProtocolCodeGen.Editor.Generator.Utils;
using NetProtocolCodeGen.Editor.Scheme;

namespace NetProtocolCodeGen.Editor.Generator.Method.Parameters
{
    public static class ParametersStructTemplate
    {
        public static StructDeclarationSyntax Create(string methodName, List<Parameter> parameters, Lang lang)
        {
            var structDeclaration = SyntaxFactory.StructDeclaration("Parameters")
                .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IMethodParameters")));
            
            var members = new List<MemberDeclarationSyntax>();
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
                var field = Helpers.GenerateReadonlyPublicField(parameter.name, cSharpType, isArray);
                members.Add(field);
            }

            if (parameters.Count > 0)
            {
                var ctor = CreateConstructor(parameters, lang);
                members.Add(ctor);   
            }

            var composeMethod = CreateComposeMethod(parameters);
            members.Add(composeMethod);
            
            var idField = GenerateProperty("Id", methodName);
            var agentIdField = GenerateProperty("AgentId", methodName);
            var getBytesMethod = GenerateGetBytesMethod(parameters);
            
            members.Add(idField);
            members.Add(agentIdField);
            members.Add(getBytesMethod);
            
            structDeclaration = structDeclaration.AddMembers(members.ToArray())
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));

            return structDeclaration;
        }

        private static ConstructorDeclarationSyntax CreateConstructor(List<Parameter> parameters, Lang lang)
        {
            var ctorParameters = new SeparatedSyntaxList<ParameterSyntax>();
            
            var body = SyntaxFactory.Block();
            var bodyStatements = new List<StatementSyntax>();
            
            foreach (var parameter in parameters)
            {
                string cSharpType;
                var isArray = parameter.type.Equals("array");
                var isEnum = parameter.type.Equals("enum");
                if (isArray)
                {
                    cSharpType = parameter.itemsType.FromTypeAndSizeToCSharpType(parameter.size, false, lang);
                    cSharpType += "[]";
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
                
                var ctorParam = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameter.name))
                    .WithType(SyntaxFactory.ParseTypeName(cSharpType));
                ctorParameters = ctorParameters.Add(ctorParam);

                var bodySt = SyntaxFactory.ParseStatement(parameter.name.FirstCharToUpper() + " = " + parameter.name + ";");
                bodyStatements.Add(bodySt);
            }

            body = body.AddStatements(bodyStatements.ToArray());
            
            var ctor = SyntaxFactory.ConstructorDeclaration("Parameters")
                .WithParameterList(SyntaxFactory.ParameterList(ctorParameters))
                .WithBody(body)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            return ctor;
        }

        private static MethodDeclarationSyntax CreateComposeMethod(List<Parameter> parameters)
        {
            var parameterSyntaxes = new List<ParameterSyntax>
            {
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("writer"))
                    .WithType(SyntaxFactory.ParseTypeName("ByteWriter"))
            };
            
            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    SyntaxFactory.Identifier("Compose"))
                
                .AddParameterListParameters(parameterSyntaxes.ToArray())
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            var body = SyntaxFactory.Block();
            var statements = new List<StatementSyntax>();
            foreach (var parameter in parameters)
            {
                switch (parameter.type)
                {
                    case "Position":
                        statements.Add(SyntaxFactory.ParseStatement(parameter.name.FirstCharToUpper() +
                                                                    ".ComposePosition(writer);"));
                        break;
                    case "array":
                        var writeCountSt = SyntaxFactory.ParseStatement($"writer.Write((byte){parameter.name.FirstCharToUpper()}.Length);");
                        var forStatement = Helpers.CreateWriteForArray(parameter.name.FirstCharToUpper());
                        statements.Add(writeCountSt);
                        statements.Add(forStatement);
                        break;
                    case "enum":
                        statements.Add(SyntaxFactory.ParseStatement("writer.Write(" + parameter.name.FirstCharToUpper() + ".ToString());"));
                        break;
                    default:
                        statements.Add(SyntaxFactory.ParseStatement("writer.Write(" + parameter.name.FirstCharToUpper() + ");"));
                        break;
                }
            }
            body = body.AddStatements(statements.ToArray());

            method = method.WithBody(body);
            
            return method;
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
        
        private static MethodDeclarationSyntax GenerateGetBytesMethod(List<Parameter> parameters)
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
            
            if (parameters.Count == 0)
            {
                var st = SyntaxFactory.ParseStatement("return new byte[]{};");
                statements.Add(st);
            }
            else
            {
                var writerSt = SyntaxFactory.ParseStatement("var writer = ByteWriterPool.Instance.Get();");
                statements.Add(writerSt);
                foreach (var parameter in parameters)
                {
                    switch ( parameter.type)
                    {
                        case "Position":
                            statements.Add(SyntaxFactory.ParseStatement(parameter.name.FirstCharToUpper() + ".ComposePosition(writer);"));
                            break;
                        case "array":
                            var writeCountSt = SyntaxFactory.ParseStatement($"writer.Write((byte){parameter.name.FirstCharToUpper()}.Length);");
                            var forStatement = Helpers.CreateWriteForArray(parameter.name.FirstCharToUpper());
                            statements.Add(writeCountSt);
                            statements.Add(forStatement);
                            break;
                        case "enum":
                            statements.Add(SyntaxFactory.ParseStatement("writer.Write(" + parameter.name.FirstCharToUpper() + ".ToString());"));
                            break;
                        default:
                            statements.Add(SyntaxFactory.ParseStatement("writer.Write(" + parameter.name.FirstCharToUpper() + ");"));
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