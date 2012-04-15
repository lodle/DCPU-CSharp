using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using TriAxis.RunSharp;
using NotchCpu.CompilerTasks.misc;

namespace DCPUC
{
    public class BlockNode : FunctionCompilableNode
    {
        protected override void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            foreach (var f in treeNode.ChildNodes)
                AddChild("Statement", f);
        }

        public override void DoCompile(Scope scope, Register target)
        {
            BlockStart();

            var lScope = scope.Push(new Scope());

            foreach (var child in ChildNodes)
            {
                if (child is FunctionCompilableNode)
                    (child as FunctionCompilableNode).DoCompile(lScope, Register.DISCARD);
                else
                    (child as CompilableNode).DoCompile(lScope, Register.DISCARD);
            }

            BlockEnd();
        }

    }
}
