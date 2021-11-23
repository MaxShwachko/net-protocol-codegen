using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetProtocolCodeGen.Editor.Generator.Utils
{
    public struct DeclarationAndUsingInfo
    {
        public BaseTypeDeclarationSyntax BaseDeclaration;
        public List<UsingDirectiveSyntax> UsingDirectives;

        public DeclarationAndUsingInfo(BaseTypeDeclarationSyntax declaration, List<UsingDirectiveSyntax> usingDirectives)
        {
            BaseDeclaration = declaration;
            UsingDirectives = usingDirectives;
        }
    }
}