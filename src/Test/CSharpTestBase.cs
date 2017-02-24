using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Builders;

namespace Test
{
    public abstract class CSharpTestBase
    {
        private readonly Workspace _workspace = new AdhocWorkspace();

        protected CompilationUnitBuilder GetBuilder(string code = "")
        {
            var cu = SyntaxFactory.ParseCompilationUnit(code);
            return SyntaxBuilder.CreateCompilationUnit(_workspace, cu);
        }
    }
}
