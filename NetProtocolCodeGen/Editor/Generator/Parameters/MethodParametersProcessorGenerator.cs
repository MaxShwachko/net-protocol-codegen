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
    public class MethodParametersProcessorGenerator
    {
        private const string ClassName = "MethodParametersProcessor";
        
        public GeneratedFile Generate(DirectoryInfo generationDirectory, string baseNamespace,
            List<MethodScheme> methodSchemes)
        {
            var namespaceStr = baseNamespace;
            var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(namespaceStr)).NormalizeWhitespace();

            var classInfo = GenerateTemplate(methodSchemes);
            @namespace = @namespace.AddMembers(classInfo);
            
            var cu = SyntaxFactory.CompilationUnit()
                .AddUsings(UsingDirectivesHolder.Instance.UsingDirectives.MethodParametersProcessorGenerator);
            
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
                        SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                            SyntaxFactory.SimpleBaseType(
                                SyntaxFactory.IdentifierName("IMethodParametersProcessor")))));


            foreach (var methodScheme in methodSchemes)
            {
                var listenerField = CreateListenersListField(methodScheme);
                classDeclaration = classDeclaration.AddMembers(listenerField);
            }

            var handleMethod = CreateHandle(methodSchemes);
            classDeclaration = classDeclaration.AddMembers(handleMethod);

            var addParametersListener = CreateAddParametersListener(methodSchemes);
            classDeclaration = classDeclaration.AddMembers(addParametersListener);
            
            var removeParametersListener = CreateRemoveParametersListener(methodSchemes);
            classDeclaration = classDeclaration.AddMembers(removeParametersListener);

            return classDeclaration;
        }

        private FieldDeclarationSyntax CreateListenersListField(MethodScheme methodScheme)
        {
            var typeName = "List<" + GetTypeName(methodScheme) + ">";
            var variableName = GetListName(methodScheme);
            var variableDeclaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(typeName))
                .AddVariables(
                    SyntaxFactory.VariableDeclarator(variableName)
                        .WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.ObjectCreationExpression(
                                    SyntaxFactory.GenericName(
                                            SyntaxFactory.Identifier("List"))
                                        .WithTypeArgumentList(
                                            SyntaxFactory.TypeArgumentList(
                                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                                    SyntaxFactory.IdentifierName(GetTypeName(methodScheme))
                                                    )))
                                    )
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList()
                                )))
                );
            
            var field = SyntaxFactory.FieldDeclaration(variableDeclaration)
                .WithModifiers(
                    SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)));
            return field;
        }

        private MethodDeclarationSyntax CreateHandle(List<MethodScheme> methodSchemes)
        {
            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                SyntaxFactory.Identifier("Handle"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            
            var parameterList = SyntaxFactory.ParameterList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Parameter(
                            SyntaxFactory.Identifier("parameters"))
                        .WithType(
                            SyntaxFactory.IdentifierName("IMethodParameters"))));

            method = method.WithParameterList(parameterList);

            var body = SyntaxFactory.Block();
            foreach (var methodScheme in methodSchemes)
            {
                
                var listName = GetListName(methodScheme);
                var parameter = GetParameterName(methodScheme);

                var forEachBlock = SyntaxFactory.Block();
                forEachBlock = forEachBlock.AddStatements(SyntaxFactory.ParseStatement("listener.Handle((" + parameter + ")parameters);"));
                
                var forEach = SyntaxFactory.ForEachStatement(
                    SyntaxFactory.IdentifierName(
                        SyntaxFactory.Identifier(
                            SyntaxFactory.TriviaList(),
                            SyntaxKind.TypeVarKeyword,
                            "var",
                            "var",
                            SyntaxFactory.TriviaList())),
                    SyntaxFactory.Identifier("listener"),
                    SyntaxFactory.IdentifierName(listName),
                    forEachBlock);

                
                var mn= methodScheme.method.ToMethodName();
                var an = methodScheme.agent.FirstCharToUpper();
                var nameSpace = an + "." + an + "." + mn;
                
                var ifBlock = SyntaxFactory.Block();
                ifBlock = ifBlock.AddStatements(forEach);

                var ifStatement = SyntaxFactory.IfStatement(
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.EqualsExpression,
                        SyntaxFactory.IdentifierName("parameters.Id"),
                        SyntaxFactory.IdentifierName($"{nameSpace}.Id && parameters.AgentId == {nameSpace}.AgentId")),
                    ifBlock);
                
                
                body = body.AddStatements(ifStatement);
            }
            method = method.WithBody(body);
            
            return method;
        }

        private MethodDeclarationSyntax CreateAddParametersListener(List<MethodScheme> methodSchemes)
        {
            var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    SyntaxFactory.Identifier("AddParametersListener"))
                .WithTypeParameterList(
                    SyntaxFactory.TypeParameterList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.TypeParameter(
                                SyntaxFactory.Identifier("T")))))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Parameter(
                                    SyntaxFactory.Identifier("listener"))
                                .WithType(
                                    SyntaxFactory.GenericName(
                                            SyntaxFactory.Identifier("IMethodParametersListener"))
                                        .WithTypeArgumentList(
                                            SyntaxFactory.TypeArgumentList(
                                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                                    SyntaxFactory.IdentifierName("T"))))))))
                .WithConstraintClauses(
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.TypeParameterConstraintClause(
                                SyntaxFactory.IdentifierName("T"))
                            .WithConstraints(
                                SyntaxFactory.SingletonSeparatedList<TypeParameterConstraintSyntax>(
                                    SyntaxFactory.TypeConstraint(
                                        SyntaxFactory.IdentifierName("IMethodParameters"))))));
            

            method = method.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            var body = SyntaxFactory.Block();
            body = body.AddStatements(SyntaxFactory.ParseStatement("var type = typeof(T);"));

            foreach (var methodScheme in methodSchemes)
            {
                var listName = GetListName(methodScheme);
                var typeName = GetTypeName(methodScheme);
                var parameter = GetParameterName(methodScheme);
                
                var block = SyntaxFactory.Block();
                block = block.AddStatements(SyntaxFactory.ParseStatement(listName + ".Add((" + typeName +")listener);"));
                
                var ifStatement = SyntaxFactory.IfStatement(
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.EqualsExpression,
                        SyntaxFactory.IdentifierName("type"),
                        SyntaxFactory.IdentifierName("typeof(" + parameter + ")")),
                    block);
                
                body = body.AddStatements(ifStatement);
            }

            method = method.WithBody(body);
            
            return method;
        }
        
        private MethodDeclarationSyntax CreateRemoveParametersListener(List<MethodScheme> methodSchemes)
        {
            var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    SyntaxFactory.Identifier("RemoveParametersListener"))
                .WithTypeParameterList(
                    SyntaxFactory.TypeParameterList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.TypeParameter(
                                SyntaxFactory.Identifier("T")))))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Parameter(
                                    SyntaxFactory.Identifier("listener"))
                                .WithType(
                                    SyntaxFactory.GenericName(
                                            SyntaxFactory.Identifier("IMethodParametersListener"))
                                        .WithTypeArgumentList(
                                            SyntaxFactory.TypeArgumentList(
                                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                                    SyntaxFactory.IdentifierName("T"))))))))
                .WithConstraintClauses(
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.TypeParameterConstraintClause(
                                SyntaxFactory.IdentifierName("T"))
                            .WithConstraints(
                                SyntaxFactory.SingletonSeparatedList<TypeParameterConstraintSyntax>(
                                    SyntaxFactory.TypeConstraint(
                                        SyntaxFactory.IdentifierName("IMethodParameters"))))));
            

            method = method.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            var body = SyntaxFactory.Block();
            body = body.AddStatements(SyntaxFactory.ParseStatement("var type = typeof(T);"));

            foreach (var methodScheme in methodSchemes)
            {
                var listName = GetListName(methodScheme);
                var typeName = GetTypeName(methodScheme);
                var parameter = GetParameterName(methodScheme);
                
                var block = SyntaxFactory.Block();
                block = block.AddStatements(SyntaxFactory.ParseStatement(listName + ".Remove((" + typeName +")listener);"));
                
                var ifStatement = SyntaxFactory.IfStatement(
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.EqualsExpression,
                        SyntaxFactory.IdentifierName("type"),
                        SyntaxFactory.IdentifierName("typeof(" + parameter + ")")),
                    block);
                
                body = body.AddStatements(ifStatement);
            }

            method = method.WithBody(body);
            
            return method;
        }
        
        private string GetTypeName(MethodScheme methodScheme)
        {
            var mn= methodScheme.method.ToMethodName();
            var an = methodScheme.agent.FirstCharToUpper();
            return an + "." + an + "." + mn + ".IParametersListener";
        }
        
        private string GetListName(MethodScheme methodScheme)
        {
            var mn= methodScheme.method.ToMethodName();
            var an = methodScheme.agent.FirstCharToUpper();
            return "_" + an.FirstCharToLower() + mn + "Listeners";
        }
        
        private string GetParameterName(MethodScheme methodScheme)
        {
            var mn= methodScheme.method.ToMethodName();
            var an = methodScheme.agent.FirstCharToUpper();
            return an + "." + an + "." + mn + ".Parameters";
        }
    }
    
}