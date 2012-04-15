using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class ComparisonNode : CompilableNode
    {
        private static Dictionary<String, String> opcodes = null;

        protected override void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.DoInit(context, treeNode);
            AddChild("Parameter", treeNode.ChildNodes[0]);
            AddChild("Parameter", treeNode.ChildNodes[2]);

            this.AsString = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override void DoCompile(Scope scope, Register target)
        {
            throw new Exception("Comparisons in general expressions are not implemented");

            //Evaluate in reverse in case both need to go on the stack
            var secondTarget = scope.FindFreeRegister();

            if (Scope.IsRegister((Register)secondTarget)) 
                scope.UseRegister(secondTarget);

            (ChildNodes[1] as CompilableNode).DoCompile(scope, (Register)secondTarget);
                        
            if (AsString == "==" || AsString == "!=")
            {
                (ChildNodes[0] as CompilableNode).DoCompile(scope, Register.STACK);
                if (target == Register.STACK)
                {
                    AddInstruction("SET", Scope.TempRegister, "0x0", "Equality onto stack");
                    AddInstruction((AsString == "==" ? "IFE" : "IFN"), "POP", Scope.GetRegisterLabelSecond(secondTarget));
                    AddInstruction("SET", Scope.TempRegister, "0x1");
                    AddInstruction("SET", "PUSH", Scope.TempRegister);
                }
                else
                {
                    AddInstruction("SET", Scope.GetRegisterLabelFirst((int)target), "0x0", "Equality into register");
                    AddInstruction((AsString == "==" ? "IFE" : "IFN"), "POP", Scope.GetRegisterLabelSecond(secondTarget));
                    AddInstruction("SET", Scope.GetRegisterLabelFirst((int)target), "0x1");
                }
            }
            
            if (secondTarget == (int)Register.STACK)
                scope.stackDepth -= 1;
            else
                scope.FreeRegister(secondTarget);
        }
    }

    
}
