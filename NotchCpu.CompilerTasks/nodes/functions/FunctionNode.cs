using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace DCPUC
{
    public class FunctionNode : CompilableNode
    {
        protected override void DoInit(ParsingContext context, ParseTreeNode treeNode)
        {
            foreach (var f in treeNode.ChildNodes)
                AddChild("Function Declaration", f);
        }

        public override void DoCompile(Scope scope, Register target)
        {
            BlockStart();

            foreach (var child in ChildNodes)
            {
                (child as CompilableNode).DoCompile(scope, Register.DISCARD);
            }

            BlockEnd();
        }

    }
}
