using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetProtocolCodeGen.Editor.Generator.Utils;

namespace NetProtocolCodeGen.Editor.Generator.Method
{
    public static class Helpers
    {
        public static FieldDeclarationSyntax GenerateByteConstant(string name, byte value)
        {
            var constant = SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(
                            SyntaxFactory.PredefinedType(
                                SyntaxFactory.Token(SyntaxKind.ByteKeyword)))
                        .WithVariables(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.VariableDeclarator(
                                        SyntaxFactory.Identifier(name))
                                    .WithInitializer(
                                        SyntaxFactory.EqualsValueClause(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.NumericLiteralExpression,
                                                SyntaxFactory.Literal(value)))))))
                .WithModifiers(
                    SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.ConstKeyword)));
            return constant;
        }
        
        public static FieldDeclarationSyntax GenerateReadonlyPrivateField(string name, string cSharpType)
        {
            var variableDeclaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(cSharpType))
                .AddVariables(SyntaxFactory.VariableDeclarator("_" + name));
            
            var field = SyntaxFactory.FieldDeclaration(variableDeclaration)
                .WithModifiers(
                    SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)));
            return field;
        }
        
        public static FieldDeclarationSyntax GenerateReadonlyPublicField(string name, string cSharpType, bool array = false)
        {
            var variableDeclaration = array
                ? SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(cSharpType + "[]"))
                    .AddVariables(SyntaxFactory.VariableDeclarator(name.FirstCharToUpper()))
                : SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(cSharpType))
                    .AddVariables(SyntaxFactory.VariableDeclarator(name.FirstCharToUpper()));


            var field = SyntaxFactory.FieldDeclaration(variableDeclaration)
                .WithModifiers(
                    SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)));
            return field;
        }
        
        public static ForStatementSyntax CreateWriteForArray(string arrayName)
        {
            var forStatement = SyntaxFactory.ForStatement(
                    SyntaxFactory.Block(
                        SyntaxFactory.SingletonList<StatementSyntax>(
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.IdentifierName("writer"),
                                            SyntaxFactory.IdentifierName("Write")))
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.ElementAccessExpression(
                                                            SyntaxFactory.IdentifierName(arrayName))
                                                        .WithArgumentList(
                                                            SyntaxFactory.BracketedArgumentList(
                                                                SyntaxFactory.SingletonSeparatedList(
                                                                    SyntaxFactory.Argument(
                                                                        SyntaxFactory.IdentifierName("i")))))))))))))
                .WithDeclaration(
                    SyntaxFactory.VariableDeclaration(
                            SyntaxFactory.IdentifierName(
                                SyntaxFactory.Identifier(
                                    SyntaxFactory.TriviaList(),
                                    SyntaxKind.TypeVarKeyword,
                                    "var",
                                    "var",
                                    SyntaxFactory.TriviaList())))
                        .WithVariables(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.VariableDeclarator(
                                        SyntaxFactory.Identifier("i"))
                                    .WithInitializer(
                                        SyntaxFactory.EqualsValueClause(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.NumericLiteralExpression,
                                                SyntaxFactory.Literal(0)))))))
                .WithCondition(
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.LessThanExpression,
                        SyntaxFactory.IdentifierName("i"),
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(arrayName),
                            SyntaxFactory.IdentifierName("Length"))))
                .WithIncrementors(
                    SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                        SyntaxFactory.PostfixUnaryExpression(
                            SyntaxKind.PostIncrementExpression,
                            SyntaxFactory.IdentifierName("i"))));
            
            return forStatement;
        }
        
        public static ForStatementSyntax CreateReadForArray(string arrayName, string counterName, string cSharpMethod)
        {
            var forStatement = SyntaxFactory.ForStatement(
                    SyntaxFactory.Block(
                        SyntaxFactory.SingletonList<StatementSyntax>(
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.ElementAccessExpression(
                                        SyntaxFactory.IdentifierName(arrayName))
                                .WithArgumentList(
                                    SyntaxFactory.BracketedArgumentList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.IdentifierName("i"))))),
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName("reader"),
                                        SyntaxFactory. IdentifierName(cSharpMethod))))))))
            .WithDeclaration(
                SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.IdentifierName(
                            SyntaxFactory.Identifier(
                                SyntaxFactory.TriviaList(),
                            SyntaxKind.TypeVarKeyword,
                            "var",
                            "var",
                                SyntaxFactory.TriviaList())))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(
                                SyntaxFactory. Identifier("i"))
                        .WithInitializer(
                            SyntaxFactory. EqualsValueClause(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    SyntaxFactory.Literal(0)))))))
            .WithCondition(
                SyntaxFactory.BinaryExpression(
                    SyntaxKind.LessThanExpression,
                    SyntaxFactory.IdentifierName("i"),
                    SyntaxFactory.IdentifierName(counterName)))
            .WithIncrementors(
                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                    SyntaxFactory. PostfixUnaryExpression(
                        SyntaxKind.PostIncrementExpression,
                        SyntaxFactory. IdentifierName("i"))));
            
            return forStatement;
        }
        
    }
    
}