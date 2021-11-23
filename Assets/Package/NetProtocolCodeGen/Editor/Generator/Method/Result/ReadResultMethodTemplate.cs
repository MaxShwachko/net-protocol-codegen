using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetProtocolCodeGen.Editor.Generator.Method.Result
{
    public static class ReadResultMethodTemplate
    {
        public static MethodDeclarationSyntax Create()
        {
            var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.ParseTypeName("IMethodResult"),
                    SyntaxFactory.Identifier("ReadResult"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Parameter(
                                    SyntaxFactory.Identifier("reader"))
                                .WithType(
                                    SyntaxFactory.IdentifierName("ByteReader")))))
                .WithExpressionBody(
                    SyntaxFactory.ArrowExpressionClause(
                        SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.ObjectCreationExpression(
                                            SyntaxFactory.IdentifierName("ResultReader"))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList()),
                                    SyntaxFactory.IdentifierName("ReadResult")))
                            .WithArgumentList(
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.IdentifierName("reader"))))))
                )
                .WithSemicolonToken(
                    SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            return method;
        }
    }
}