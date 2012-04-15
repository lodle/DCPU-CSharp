using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class VariableNameNode : CompilableNode
    {
        protected override void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            this.AsString = treeNode.FindTokenAndGetText();
        }

        public override void DoCompile(Scope scope, Register target)
        {
            _Annotation.register = (int)target;
            var variable = scope.FindVariable(AsString);

            if (variable == null) 
                throw new Exception("Could not find variable " + AsString);

            if (variable.location == Register.STACK)
            {
                if (scope.stackDepth - variable.stackOffset > 1)
                {
                    var ins = String.Format("[{0} + {1}]", Util.hex(scope.stackDepth - variable.stackOffset - 1), Scope.TempRegister);

                    AddInstruction("SET", Scope.TempRegister, "SP");
                    AddInstruction("SET", Scope.GetRegisterLabelFirst((int)target), ins, "Fetching variable", _Annotation);
                }
                else
                {
                    
                    AddInstruction("SET", Scope.GetRegisterLabelFirst((int)target), "PEEK", "Fetching variable", _Annotation);
                }
            }
            else
            {
                if (target == variable.location) 
                    return;

                _Annotation.register = (int)target;
                AddInstruction("SET", Scope.GetRegisterLabelFirst((int)target), Scope.GetRegisterLabelSecond((int)variable.location), "Fetching variable", _Annotation);
            }

            if (target == Register.STACK) 
                scope.stackDepth += 1;
        }
    }

    
}
