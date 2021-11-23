using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetProtocolCodeGen.Editor.Generator.Utils.Directives;
using UnityEngine.UIElements;

namespace Samples.Editor
{
    public class UsingDirectives : IUsingDirectives
    {
        public UsingDirectiveSyntax[] MethodParametersProcessorGenerator =>  new []
        {
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic"))
                .NormalizeWhitespace(),
            // SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(IMethodParametersProcessor).Namespace))
            //     .NormalizeWhitespace(),
            // SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(IMethodParameters).Namespace))
            //     .NormalizeWhitespace(),
        }; 
        
        
        public UsingDirectiveSyntax[] MethodParametersReaderRepositoryGenerator => new []
        {
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic"))
                .NormalizeWhitespace(),
            // SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(IMethodParametersReaderRepository).Namespace))
            //     .NormalizeWhitespace(),
            // SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(IMethodParametersReader).Namespace))
            //     .NormalizeWhitespace(),
            // SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(IInitializable).Namespace))
            //     .NormalizeWhitespace(),
            // SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(CompositeId).Namespace))
            //     .NormalizeWhitespace(),          
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(ArgumentException).Namespace))
                .NormalizeWhitespace(),
        };
        
        
        public UsingDirectiveSyntax[] MethodResultProcessorGenerator => new []
        {
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic"))
                .NormalizeWhitespace(),
            // SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(IMethodResultProcessor).Namespace))
            //     .NormalizeWhitespace(),
            // SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(IMethodResult).Namespace))
            //     .NormalizeWhitespace(),
        };     
        
        
        
        public UsingDirectiveSyntax[] MethodGenerator => new []
        {
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")).NormalizeWhitespace(),
            // SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(IMethod).Namespace))
            //     .NormalizeWhitespace(),
            // SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(ByteWriterPool).Namespace))
            //     .NormalizeWhitespace(),
            // SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(ByteWriter).Namespace))
            //     .NormalizeWhitespace(),
            // SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(CompositeId).Namespace))
            //     .NormalizeWhitespace(),
            // SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(IMethodResultListener<>).Namespace))
            //     .NormalizeWhitespace(),
            // SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(PositionExtensions).Namespace))
            //     .NormalizeWhitespace(),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(Position).Namespace))
                .NormalizeWhitespace(),
            // SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(IMethodParametersListener<>).Namespace))
            //     .NormalizeWhitespace(),
        };
        
        
        public UsingDirectiveSyntax[] MethodResultReaderRepositoryGenerator => new []
        {
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic"))
                .NormalizeWhitespace(),
            // SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(IMethodResultReaderRepository).Namespace))
            //     .NormalizeWhitespace(),
            // SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(IMethodResultReader).Namespace))
            //     .NormalizeWhitespace(),
            // SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(IInitializable).Namespace))
            //     .NormalizeWhitespace(),
            // SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(CompositeId).Namespace))
            //     .NormalizeWhitespace(),          
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(ArgumentException).Namespace))
                .NormalizeWhitespace(),
        };            
    }
}