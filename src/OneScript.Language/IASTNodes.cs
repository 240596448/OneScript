﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Language
{
    public interface IASTNode
    {
    }

    public interface IASTStatementNode : IASTNode
    {
    }

    public interface IASTConditionNode : IASTStatementNode
    {
        IASTNode Condition { get; set; }
        IASTNode TruePart { get; set; }
        IASTNode FalsePart { get; set; }
    }

    public interface IASTWhileNode : IASTStatementNode
    {
        IASTNode Condition { get; set; }
        IASTNode Body { get; set; }
    }

    public interface IASTForLoopNode : IASTStatementNode
    {
        IASTNode InitializerExpression { get; set; }
        IASTNode BoundExpression { get; set; }
        IASTNode Body { get; set; }
    }

    public interface IASTForEachNode : IASTStatementNode
    {
        string ItemIdentifier { get; set; }
        IASTNode CollectionExpression { get; set; }
        IASTNode Body { get; set; }
    }

    public interface IASTTryCatchNode : IASTStatementNode
    {
        IASTNode TryBlock { get; set; }
        IASTNode ExceptBlock { get; set; }
    }

    public struct ASTMethodParameter
    {
        public string Name;
        public bool ByValue;
        public bool IsOptional;
        public ConstDefinition DefaultValueLiteral;
    }

    public interface IASTMethodDefinitionNode : IASTNode
    {
        string Identifier { get; set; }
        bool IsFunction { get; set; }
        bool IsExported { get; set; }
        ASTMethodParameter[] Parameters { get; set; }
    }

    public interface IASTCodeBatchNode
    {
        IList<IASTStatementNode> Children { get; }
    }

}
