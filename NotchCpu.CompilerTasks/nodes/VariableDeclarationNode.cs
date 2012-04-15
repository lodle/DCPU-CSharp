using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using NotchCpu.CompilerTasks.misc;
using TriAxis.RunSharp;

namespace DCPUC
{
    public class VariableDeclarationNode : FunctionCompilableNode
    {

        protected override void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            AddChild("Value", treeNode.ChildNodes[3].FirstChild);
            AsString = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override void DoCompile(Scope scope, Register register)
        {
            var newVariable = new Variable();
            newVariable.name = AsString;
            newVariable.scope = scope;
            newVariable.stackOffset = scope.stackDepth;
            newVariable.location = (Register)scope.FindAndUseFreeRegister();

            if (newVariable.location == Register.I)
                newVariable.location = Register.STACK;

            (ChildNodes[0] as CompilableNode).DoCompile(scope, newVariable.location);
            scope.variables.Add(newVariable);
            //scope.stackDepth += 1;
        }
    }
}
