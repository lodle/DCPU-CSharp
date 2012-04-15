using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class DereferenceNode : CompilableNode
    {
        protected override void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.DoInit(context, treeNode);
            AddChild("Expression", treeNode.ChildNodes[1]);
            
        }

        protected override void DoCompileAssembly assembly, Scope scope, Register target)
        {
            var destRegister = target == Register.STACK ? scope.FindAndUseFreeRegister() : (int)target;
            (ChildNodes[0] as CompilableNode).DoCompile(assembly, scope, (Register)destRegister);
            if (target == Register.STACK)
            {
                if (destRegister == (int)Register.STACK)
                    assembly.Add("SET", "PEEK", "[PEEK]");
                else
                {
                    assembly.Add("SET", "PUSH", "[" + Scope.GetRegisterLabelSecond(destRegister) + "]");
                    scope.stackDepth += 1;
                    scope.FreeMaybeRegister(destRegister);
                }
            }
            else
            {
                assembly.Add("SET", Scope.GetRegisterLabelFirst((int)target), "[" + Scope.GetRegisterLabelSecond(destRegister) + "]");
                if (destRegister == (int)Register.STACK)
                    scope.stackDepth -= 1;
                else if (destRegister != (int)target)
                    scope.FreeMaybeRegister(destRegister);
            }

        }
    }

    
}
