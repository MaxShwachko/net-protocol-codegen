using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetProtocolCodeGen.Editor.Generator.Method.Result
{
    public static class ResultListenerTemplate
    {
        public static InterfaceDeclarationSyntax Create()
        {
            var interfaceDeclaration = SyntaxFactory.InterfaceDeclaration("IResultListener")
                .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IMethodResult")))

                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBaseList(
                    SyntaxFactory.BaseList(
                        SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                            SyntaxFactory.SimpleBaseType(
                                SyntaxFactory.GenericName(
                                        SyntaxFactory.Identifier("IMethodResultListener"))
                                    .WithTypeArgumentList(
                                        SyntaxFactory.TypeArgumentList(
                                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                                SyntaxFactory.IdentifierName("Result"))))))));

            return interfaceDeclaration;
        }
    }
}