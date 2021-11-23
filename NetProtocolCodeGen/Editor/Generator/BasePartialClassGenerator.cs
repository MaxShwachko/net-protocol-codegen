using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Generator.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetProtocolCodeGen.Editor.Generator.Utils;

namespace NetProtocolCodeGen.Editor.Generator
{
    public class BasePartialClassGenerator
    {
        public GeneratedFile Generate(DirectoryInfo generationDirectory, string baseNamespace, string agent)
        {
            var namespaceStr = baseNamespace + "." + agent.FirstCharToUpper();
            var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(namespaceStr)).NormalizeWhitespace();
            
            var usingDirectives = new List<UsingDirectiveSyntax>();

            var classInfo = GenerateTemplate(agent);
            @namespace = @namespace.AddMembers(classInfo);
            
            var cu = SyntaxFactory.CompilationUnit();
                // .AddUsings(usingDirectives.ToArray());
            
            cu = cu.AddMembers(@namespace);

            var code = cu
                .NormalizeWhitespace()
                .ToFullString();
            var autoPropRegex = new Regex(@"\s*\{\s*get;\s*set;\s*}\s");
            code = autoPropRegex.Replace(code, " { get; set; }");
            code = Utils.Utils.Comment + "\n" + code;
            
            var implFileInfo = GetImplFileInfo(generationDirectory, agent.FirstCharToUpper());
            
            return new GeneratedFile(implFileInfo, code);
        }
        
        private static FileInfo GetImplFileInfo(DirectoryInfo directory, string name)
        {
            var fileName = name + ".cs";
            return new FileInfo(Path.Combine(directory.FullName, fileName));
        }

        private ClassDeclarationSyntax GenerateTemplate(string agent)
        {
            var classDeclaration = SyntaxFactory.ClassDeclaration(agent.FirstCharToUpper());
            classDeclaration = classDeclaration
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword));
            return classDeclaration;
        }
    }
}