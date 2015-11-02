﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Language
{
    public interface IASTBuilder
    {

        void BeginModule();

        void BeginModuleBody();
        
        void EndModuleBody();

        void CompleteModule();

        void DefineExportVariable(string symbolicName);

        void DefineVariable(string symbolicName);

        IASTNode SelectOrCreateVariable(string identifier);

        IASTNode BuildAssignment(IASTNode acceptor, IASTNode source);

        IASTNode ReadLiteral(Lexem lexem);

        IASTNode ReadVariable(string identifier);

        IASTNode BinaryOperation(Token operationToken, IASTNode leftHandedNode, IASTNode rightHandedNode);

        IASTNode UnaryOperation(Token token, IASTNode operandNode);

        IASTNode BuildFunctionCall(IASTNode target, string identifier, IASTNode[] args);

        void BuildProcedureCall(IASTNode target, string identifier, IASTNode[] args);

        IASTNode ResolveProperty(IASTNode target, string propertyName);

        IASTNode BuildIndexedAccess(IASTNode target, IASTNode expression);

        IASTMethodDefinitionNode BeginMethod();

        void EndMethod(IASTMethodDefinitionNode methodNode);

        IASTNode BeginBatch();
        
        void EndBatch(IASTNode batch);

        IASTConditionNode BeginConditionStatement();

        void EndConditionStatement(IASTConditionNode node);

        IASTWhileNode BeginWhileStatement();

        void EndWhileStatement(IASTWhileNode node);

        IASTForLoopNode BeginForLoopNode();

        void EndForLoopNode(IASTForLoopNode node);

        IASTForEachNode BeginForEachNode();
        
        void EndForEachNode(IASTForEachNode node);

        IASTTryExceptNode BeginTryExceptNode();

        void EndTryBlock(IASTTryExceptNode node);

        void EndExceptBlock(IASTTryExceptNode node);

        void BuildBreakStatement();

        void BuildContinueStatement();

        void BuildReturnStatement(IASTNode expression);

        void BuildRaiseExceptionStatement(IASTNode expression);

    }
}
