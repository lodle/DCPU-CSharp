using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class AssignmentNode : CompilableNode
    {
        protected override void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            AddChild("LValue", treeNode.ChildNodes[0]);
            AddChild("RValue", treeNode.ChildNodes[2]);

            Annotation.type = AnotationType.Assignment;
            Annotation.name = ChildNodes[0].AsString;
        }

        public override void DoCompile(Scope scope, Register target)
        {
            var variable = scope.FindVariable(ChildNodes[0].AsString);

            if (variable == null) 
                throw new Exception("Could not find variable " + ChildNodes[0].AsString);

            if (variable.location != Register.STACK)
            {
                (ChildNodes[1] as CompilableNode).DoCompile(scope, variable.location);

                Annotation.register = (int)variable.location;
                AddInstruction("NOP", "", "", Annotation);
            }
            else
            {
                var register = scope.FindAndUseFreeRegister();
                (ChildNodes[1] as CompilableNode).DoCompile(scope, (Register)register);
                scope.FreeMaybeRegister(register);

                //is this not on the top of the stack
                if (scope.stackDepth - variable.stackOffset > 1)
                {
                    var ins = String.Format("[{0} + {1}]", Util.hex(scope.stackDepth - variable.stackOffset - 1), Scope.TempRegister);
                    Annotation.register = register;

                    AddInstruction("SET", Scope.TempRegister, "SP");
                    AddInstruction("SET", ins, Scope.GetRegisterLabelSecond(register), "Fetching variable", Annotation);
                }
                else
                {
                    Annotation.register = register;
                    AddInstruction("SET", "PEEK", Scope.GetRegisterLabelSecond(register), "Fetching variable", Annotation);
                }

                if (register == (int)Register.STACK) 
                    scope.stackDepth -= 1;
            }
        }
    }

    
}
