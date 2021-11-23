using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Generator.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetProtocolCodeGen.Editor.Generator.Utils;
using NetProtocolCodeGen.Editor.Generator.Utils.Directives;
using NetProtocolCodeGen.Editor.Scheme;

namespace NetProtocolCodeGen.Editor.Generator.Parameters
{
    public class MethodParametersReaderRepositoryGenerator
    {
        private const string ClassName = "MethodParametersReaderRepository";
        
        public GeneratedFile Generate(DirectoryInfo generationDirectory, string baseNamespace,
            List<MethodScheme> methodSchemes)
        {
            var namespaceStr = baseNamespace;
            var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(namespaceStr)).NormalizeWhitespace();

            var classInfo = GenerateTemplate(methodSchemes);
            @namespace = @namespace.AddMembers(classInfo);

            var cu = SyntaxFactory.CompilationUnit()
                .AddUsings(UsingDirectivesHolder.Instance.UsingDirectives.MethodParametersReaderRepositoryGenerator);
         
            cu = cu.AddMembers(@namespace);

            var code = cu
                .NormalizeWhitespace()
                .ToFullString();
            var autoPropRegex = new Regex(@"\s*\{\s*get;\s*set;\s*}\s");
            code = autoPropRegex.Replace(code, " { get; set; }");
            code = Utils.Utils.Comment + "\n" + code;
                
            var implFileInfo = Utils.Utils.GetImplFileInfo(generationDirectory, ClassName);
            
            return new GeneratedFile(implFileInfo, code);                
        }
        
        private ClassDeclarationSyntax GenerateTemplate(List<MethodScheme> methodSchemes)
        {
            var classDeclaration = SyntaxFactory.ClassDeclaration(ClassName);
            classDeclaration = classDeclaration
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBaseList(
                    SyntaxFactory.BaseList(
                        SyntaxFactory.SeparatedList<BaseTypeSyntax>(
                            new SyntaxNodeOrToken[]{
                                SyntaxFactory.SimpleBaseType(
                                    SyntaxFactory.IdentifierName("IMethodParametersReaderRepository")),
                                SyntaxFactory.Token(SyntaxKind.CommaToken),
                                SyntaxFactory.SimpleBaseType(
                                    SyntaxFactory.IdentifierName("IInitializable"))})));


            var dictionaryField = CreateDictionaryField();
            classDeclaration = classDeclaration.AddMembers(dictionaryField);

            var initializeMethod = CreateInitializeMethod(methodSchemes);
            classDeclaration = classDeclaration.AddMembers(initializeMethod);
            
            var handleMethod = CreateGetMethod();
            classDeclaration = classDeclaration.AddMembers(handleMethod);

            return classDeclaration;
        }



        private FieldDeclarationSyntax CreateDictionaryField()
        {
            var variableDeclaration = SyntaxFactory.VariableDeclaration(
                SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier("Dictionary"))
                    .WithTypeArgumentList(
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SeparatedList<TypeSyntax>(
                                new SyntaxNodeOrToken[]
                                {
                                    SyntaxFactory.IdentifierName("CompositeId"),
                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                    SyntaxFactory.IdentifierName("IMethodParametersReader")
                                }))));
            
            variableDeclaration = variableDeclaration.WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier("_readers"))
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.ObjectCreationExpression(
                                            SyntaxFactory.GenericName(
                                                    SyntaxFactory.Identifier("Dictionary"))
                                                .WithTypeArgumentList(
                                                    SyntaxFactory.TypeArgumentList(
                                                        SyntaxFactory.SeparatedList<TypeSyntax>(
                                                            new SyntaxNodeOrToken[]{
                                                                SyntaxFactory.IdentifierName("CompositeId"),
                                                                SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                                SyntaxFactory.IdentifierName("IMethodParametersReader")}))))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList())))));

            var field = SyntaxFactory.FieldDeclaration(variableDeclaration)
                .WithModifiers(
                    SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)));
            return field;
        }

        private MethodDeclarationSyntax CreateInitializeMethod(List<MethodScheme> methodSchemes)
        {
            var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    SyntaxFactory.Identifier("Initialize"))
                .WithModifiers(
                    SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

            var body = SyntaxFactory.Block();
            foreach (var methodScheme in methodSchemes)
            {
                var mn= methodScheme.method.ToMethodName();
                var an = methodScheme.agent.FirstCharToUpper();
                var baseMethodName = an + "." + an + "." + mn.FirstCharToUpper();
                var st = string.Format("_readers.Add(new CompositeId({0}.Id,  {0}.AgentId), new {0}());", baseMethodName);
                body = body.AddStatements(SyntaxFactory.ParseStatement(st));
            }
            
            method = method.WithBody(body);
            
            return method;
        }
        
        private MemberDeclarationSyntax CreateGetMethod()
        {
            var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.ParseTypeName("IMethodParametersReader"),
                    SyntaxFactory.Identifier("Get")
                )
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            method = method.WithParameterList(
                SyntaxFactory.ParameterList(
                    SyntaxFactory.SeparatedList<ParameterSyntax>(
                        new SyntaxNodeOrToken[]{
                            SyntaxFactory.Parameter(
                                    SyntaxFactory.Identifier("agentId"))
                                .WithType(
                                    SyntaxFactory.PredefinedType(
                                        SyntaxFactory.Token(SyntaxKind.ByteKeyword))),
                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                            SyntaxFactory.Parameter(
                                    SyntaxFactory.Identifier("methodId"))
                                .WithType(
                                    SyntaxFactory.PredefinedType(
                                        SyntaxFactory.Token(SyntaxKind.ByteKeyword)))})));
            
            var body = SyntaxFactory.Block();
            
            var forEachBlock = SyntaxFactory.Block();
            var forEachStatements = new List<StatementSyntax>();
            forEachStatements.Add(SyntaxFactory.ParseStatement("if (entry.Key.AgentId == agentId && entry.Key.ID == methodId) return entry.Value;"));
            forEachBlock = forEachBlock.AddStatements(forEachStatements.ToArray());
            
            var forEach = SyntaxFactory.ForEachStatement(
                SyntaxFactory.IdentifierName(
                    SyntaxFactory.Identifier(
                        SyntaxFactory.TriviaList(),
                        SyntaxKind.TypeVarKeyword,
                        "var",
                        "var",
                        SyntaxFactory.TriviaList())),
                SyntaxFactory.Identifier("entry"),
                SyntaxFactory.IdentifierName("_readers"),
                forEachBlock);

            body = body.AddStatements(forEach);
            body = body.AddStatements(SyntaxFactory.ParseStatement("throw new ArgumentException(\"[MethodParametersProcessor] No method with agentId\" + agentId + \", methodId: \" + methodId);"));
            method = method.WithBody(body);
            
            return method;
        }
    }
}