using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Builders;

namespace Test
{
    public abstract class CSharpTestBase
    {
        private static readonly MefHostServices _services = MefHostServices.Create(MefHostServices.DefaultAssemblies.Add(typeof(SyntaxBuilder).Assembly));
        private static readonly Workspace _workspace = new AdhocWorkspace(_services);

        protected CompilationUnitBuilder GetBuilder(string code = "")
        {
            var cu = SyntaxFactory.ParseCompilationUnit(code);
            return SyntaxBuilder.CreateCompilationUnit(_workspace, cu);
        }
    }
}
