using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Builders
{
    internal class BuilderContext
    {
        private readonly Workspace _workspace;
        private readonly SyntaxGenerator _generator;
        private readonly SyntaxNode _originalRoot;
        private SyntaxNode _currentRoot;

        public BuilderContext(Workspace workspace, SyntaxNode root)
        {
            _workspace = workspace;
            _generator = SyntaxGenerator.GetGenerator(workspace, root.Language);
            _originalRoot = root;
            _currentRoot = root;
        }

        public Workspace Workspace => _workspace;
        public SyntaxGenerator Generator => _generator;
        public SyntaxNode OriginalRoot => _originalRoot;
        public SyntaxNode CurrentRoot => _currentRoot;

        public void Replace(SyntaxNode currentNode, SyntaxNode newNode)
        {
            _currentRoot = _currentRoot.ReplaceNode(currentNode, newNode);
        }

        public void TrackNodes(IEnumerable<SyntaxNode> currentNodes)
        {
            _currentRoot = _currentRoot.TrackNodes(currentNodes);
        }

        public void RemoveNode(SyntaxNode currentNode)
        {
            _currentRoot = _generator.RemoveNode(_currentRoot, currentNode);
        }

        public void InsertAfter(SyntaxNode existingNode, SyntaxNode newNode)
        {
            _currentRoot = _generator.InsertNodesAfter(_currentRoot, existingNode, new[] { newNode });
        }
    }
}
