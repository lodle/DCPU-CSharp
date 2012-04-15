using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class ReturnStatementNode : CompilableNode
    {
        protected override void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            AddChild("value", treeNode.ChildNodes[1]);
            this.AsString = treeNode.FindTokenAndGetText();

            _Annotation = new Annotation(context, treeNode);
            _Annotation.type = AnotationType.FunctionReturn;
        }

        public override void DoCompile(Scope scope, Register target)
        {
            var reg = scope.FindAndUseFreeRegister();
            (ChildNodes[0] as CompilableNode).DoCompile(scope, (Register)reg);

            if (reg != (int)Register.A) 
                AddInstruction("SET", "A", Scope.GetRegisterLabelSecond(reg));

            scope.FreeMaybeRegister(reg);

            if (reg == (int)Register.STACK) 
                scope.stackDepth -= 1;

            scope.activeFunction.CompileReturn(scope);
        }
    }
}
