using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Generator.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetProtocolCodeGen.Editor.Generator.Method.Parameters;
using NetProtocolCodeGen.Editor.Generator.Method.Result;
using NetProtocolCodeGen.Editor.Generator.Utils;
using NetProtocolCodeGen.Editor.Generator.Utils.Directives;
using NetProtocolCodeGen.Editor.Scheme;

namespace NetProtocolCodeGen.Editor.Generator.Method
{
    public class MethodGenerator
    {
        public GeneratedFile Generate(DirectoryInfo generationDirectory, string baseNamespace, MethodScheme methodScheme, Lang lang)
        {
            var namespaceStr = baseNamespace + "." + methodScheme.agent.FirstCharToUpper();
            var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(namespaceStr)).NormalizeWhitespace();
            
            var classInfo = GenerateBaseClassTemplate(methodScheme.agent);
            var mainStruct = GenerateMainStruct(methodScheme, lang);

            classInfo = classInfo.AddMembers(mainStruct);
            @namespace = @namespace.AddMembers(classInfo);

            
            var usingList = new List<UsingDirectiveSyntax>();
            if (methodScheme.HasEnums)
            {
                var enumUsing = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(baseNamespace + ".Enums"))
                    .NormalizeWhitespace();  
                usingList.Add(enumUsing);
            }
            usingList.AddRange(UsingDirectivesHolder.Instance.UsingDirectives.MethodGenerator);
            var cu = SyntaxFactory.CompilationUnit()
                .AddUsings(usingList.ToArray());
            
            cu = cu.AddMembers(@namespace);

            var code = cu
                .NormalizeWhitespace()
                .ToFullString();
            var autoPropRegex = new Regex(@"\s*\{\s*get;\s*set;\s*}\s");
            code = autoPropRegex.Replace(code, " { get; set; }");
            code = code.Replace(" ?", "?");
            code = Utils.Utils.Comment + "\n" + code;
            
            var implFileInfo = GetImplFileInfo(generationDirectory, methodScheme.method.ToMethodName());
            
            return new GeneratedFile(implFileInfo, code);
        }
        
        private static FileInfo GetImplFileInfo(DirectoryInfo directory, string name)
        {
            var fileName = name + ".cs";
            return new FileInfo(Path.Combine(directory.FullName, fileName));
        }
        
        private StructDeclarationSyntax GenerateMainStruct(MethodScheme methodScheme, Lang lang)
        {
            var structDeclaration = SyntaxFactory.StructDeclaration(methodScheme.method.ToMethodName())
                .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IMethodResultReader")),
                    SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IMethodParametersReader")));

            var idField = Helpers.GenerateByteConstant("Id", methodScheme.byteMethod);
            var agentIdField = Helpers.GenerateByteConstant("AgentId", methodScheme.byteAgent);
            var buildMethod = BuildMethodTemplate.Create(methodScheme.parameters, lang);
            
            var parametersStruct = ParametersStructTemplate.Create(methodScheme.method.FirstCharToUpper(), methodScheme.parameters, lang);
            var parametersReaderStruct = ParametersReaderStructTemplate.Create(methodScheme.parameters, lang);
            var readParametersMethod = ReadParametersMethodTemplate.Create();
            var parametersListenerInterface = ParametersListenerTemplate.Create();
            
            var resultStruct = ResultStructTemplate.Create(methodScheme.method.FirstCharToUpper(), methodScheme.returns, lang);
            var resultReaderStruct = ResultReaderStructTemplate.Create(methodScheme.returns, lang);
            var readResultMethod = ReadResultMethodTemplate.Create();
            var resultListenerInterface = ResultListenerTemplate.Create();
            
            structDeclaration = structDeclaration.AddMembers(idField, agentIdField, buildMethod, 
                    parametersStruct, parametersReaderStruct, readParametersMethod, parametersListenerInterface, 
                    resultStruct, resultReaderStruct, readResultMethod, resultListenerInterface)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));

            return structDeclaration;
        }
        
        private ClassDeclarationSyntax GenerateBaseClassTemplate(string agent)
        {
            var classDeclaration = SyntaxFactory.ClassDeclaration(agent.FirstCharToUpper());
            classDeclaration = classDeclaration
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword));
            return classDeclaration;
        }
    }
}