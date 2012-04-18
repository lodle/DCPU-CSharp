using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NotchCpu.CompilerTasks.misc;
using DCPUC;
using NotchCpu.CompilerTasks.nodes.loops;

namespace NotchCpu.CompilerTasks
{
    public class WhileLoopNode : LoopFunctionCompilableNode
    {

        protected override void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            AddChild("Comparison", treeNode.ChildNodes[0]);
            AddChild("Statement", treeNode.ChildNodes[1]);
        }

        public override void DoCompile(DCPUC.Scope scope, DCPUC.Register target)
        {
            scope.PushLoop(this);

            _Label = Scope.GetLabel();

            AddInstruction(":" + _Label + "_START", "", "");
            (ChildNodes[0] as CompilableNode).DoCompile(scope, Register.STACK);

            AddInstruction("SET", "PUSH", "0x00");
            AddInstruction("IFE", "POP", "POP", "Check if should stop looping", Annotation);
            AddInstruction("SET", "PC", _Label + "_END");

            (ChildNodes[1] as CompilableNode).DoCompile(scope, Register.DISCARD);

            AddInstruction("SET", "PC", _Label + "_START");
            AddInstruction(":" + _Label + "_END", "", "");

            scope.PopLoop();
        }

        public override void Break()
        {
            AddInstruction("SET", "PC", _Label + "_END", "Break");
        }

        public override void Continue()
        {
            AddInstruction("SET", "PC", _Label + "_START", "Continue");
        }
    }
}
