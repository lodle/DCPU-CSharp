using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NotchCpu.CompilerTasks.misc;

namespace NotchCpu.CompilerTasks.nodes.loops
{

    public class BreakStatementNode : FunctionCompilableNode
    {
        protected override void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
        }

        public override void DoCompile(DCPUC.Scope scope, DCPUC.Register target)
        {
            scope.PeekLoop().Break();
        }
    }

    public class ContinueStatementNode : FunctionCompilableNode
    {

        protected override void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
        }

        public override void DoCompile(DCPUC.Scope scope, DCPUC.Register target)
        {
            scope.PeekLoop().Continue();
        }
    }

    public abstract class LoopFunctionCompilableNode : FunctionCompilableNode
    {
        protected String _Label;

        public abstract void Break();
        public abstract void Continue();
    }
}
