using System.Collections.Generic;
using System.IO;
using Generator.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetProtocolCodeGen.Editor.Generator.Utils;
using NetProtocolCodeGen.Editor.Scheme;

namespace NetProtocolCodeGen.Editor.Generator
{
    public class EnumsGenerator
    {
        public List<GeneratedFile> GenerateAllEnums(DirectoryInfo generationDirectory, string baseNamespace, List<MethodScheme> methodSchemes)
        {
            var dir = new DirectoryInfo(Path.Combine(generationDirectory.FullName, "Enums"));
            
            var files = new List<GeneratedFile>();
            var enumInfos = GetEnumInfos(methodSchemes);
            
            foreach (var enumInfo in enumInfos)
            {
                var file = CreateEnumFile(dir, baseNamespace, enumInfo);
                files.Add(file);
            }
            
            return files;
        }


        private List<EnumInfo> GetEnumInfos(List<MethodScheme> methodSchemes)
        {
            var parameterOrReturns = new List<AParameterOrReturn>();
            
            var infos = new List<EnumInfo>();

            foreach (var methodScheme in methodSchemes)
            {
                parameterOrReturns.AddRange(methodScheme.parameters);
                parameterOrReturns.AddRange(methodScheme.returns);
            }

            foreach (var parameterOrReturn in parameterOrReturns)
            {
                var enumInfo = TryCreateByParameterOrReturn(parameterOrReturn);
                if (enumInfo.HasValue)
                {
                    infos.Add(enumInfo.Value);
                }
            }

            return infos;
        }


        private EnumInfo? TryCreateByParameterOrReturn(AParameterOrReturn parameterOrReturn)
        {
            if (!parameterOrReturn.type.Equals("enum")) return null;
            return new EnumInfo("E" + parameterOrReturn.name.FirstCharToUpper(), parameterOrReturn.allowedValues);
        }

        private GeneratedFile CreateEnumFile(DirectoryInfo dir, string baseNamespace, EnumInfo enumInfo)
        {
            var enumDeclaration = SyntaxFactory.EnumDeclaration(enumInfo.Name)
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBaseList(
                    SyntaxFactory.BaseList(
                        SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                            SyntaxFactory.SimpleBaseType(
                                SyntaxFactory.PredefinedType(
                                    SyntaxFactory.Token(SyntaxKind.ByteKeyword))))));

            var enumMembers = SyntaxFactory.SeparatedList<EnumMemberDeclarationSyntax>();
            
            var counter = 1;
            foreach (var value in enumInfo.Values)
            {
                var enumMember = SyntaxFactory.EnumMemberDeclaration(SyntaxFactory.Identifier(value))
                    .WithEqualsValue(
                        SyntaxFactory.EqualsValueClause(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                SyntaxFactory.Literal(counter))));

                enumMembers = enumMembers.Add(enumMember);
                counter++;
            }

            enumDeclaration = enumDeclaration.WithMembers(enumMembers);

            var namespaceStr = baseNamespace + ".Enums";
            var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(namespaceStr)).NormalizeWhitespace();
            @namespace = @namespace.AddMembers(enumDeclaration);
            
            var cu = SyntaxFactory.CompilationUnit().AddMembers(@namespace);
            
            var code = cu
                .NormalizeWhitespace()
                .ToFullString();
            code = Utils.Utils.Comment + "\n" + code;
            var implFileInfo = Utils.Utils.GetImplFileInfo(dir, enumInfo.Name);
            
            return new GeneratedFile(implFileInfo, code);

        }

        private readonly struct EnumInfo
        {
            public readonly string Name;
            public readonly List<string> Values;

            public EnumInfo(string name, List<string> values)
            {
                Name = name;
                Values = values;
            }
        }
    }

}