using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetProtocolCodeGen.Editor.Generator.Utils.Directives
{
    public interface IUsingDirectives
    {
        UsingDirectiveSyntax[] MethodParametersProcessorGenerator { get; }
        UsingDirectiveSyntax[] MethodParametersReaderRepositoryGenerator { get; }
        UsingDirectiveSyntax[] MethodResultProcessorGenerator { get; }
        UsingDirectiveSyntax[] MethodGenerator { get; }
        UsingDirectiveSyntax[] MethodResultReaderRepositoryGenerator { get; }
    }
}