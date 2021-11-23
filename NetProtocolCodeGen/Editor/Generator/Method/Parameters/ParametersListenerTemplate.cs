using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetProtocolCodeGen.Editor.Generator.Method.Parameters
{
    public static class ParametersListenerTemplate
    {
        public static InterfaceDeclarationSyntax Create()
        {
            var interfaceDeclaration = SyntaxFactory.InterfaceDeclaration("IParametersListener")
                .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IMethodParameters")))

                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBaseList(
                    SyntaxFactory.BaseList(
                        SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                            SyntaxFactory.SimpleBaseType(
                                SyntaxFactory.GenericName(
                                        SyntaxFactory.Identifier("IMethodParametersListener"))
                                    .WithTypeArgumentList(
                                        SyntaxFactory.TypeArgumentList(
                                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                                SyntaxFactory.IdentifierName("Parameters"))))))));

            return interfaceDeclaration;
        }
    }
}