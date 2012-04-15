using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class BinaryOperationNode : CompilableNode
    {
        private static Dictionary<String, String> opcodes = new Dictionary<string, string>
        {
            {"+", "ADD"},
            {"-", "SUB"},
            {"*", "MUL"},
            {"/", "DIV"},
            {"%", "MOD"},
            {"<<", "SHL"},
            {">>", "SHR"},
            {"&", "AND"},
            {"|", "BOR"},
            {"^", "XOR"},
        };

        protected override void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            AddChild("Parameter", treeNode.ChildNodes[0]);
            AddChild("Parameter", treeNode.ChildNodes[2]);
            this.AsString = treeNode.ChildNodes[1].FindTokenAndGetText();
        }

        public override bool IsConstant()
        {
            return (ChildNodes[0] as CompilableNode).IsConstant() && (ChildNodes[1] as CompilableNode).IsConstant();
        }

        public override ushort GetConstantValue()
        {
            var a = (ChildNodes[0] as CompilableNode).GetConstantValue();
            var b = (ChildNodes[1] as CompilableNode).GetConstantValue();

            if (AsString == "+") 
                return (ushort)(a + b);

            if (AsString == "-") 
                return (ushort)(a - b);

            if (AsString == "*") 
                return (ushort)(a * b);

            if (AsString == "/") 
                return (ushort)(a / b);

            if (AsString == "%") 
                return (ushort)(a % b);

            if (AsString == "<<") 
                return (ushort)(a << b);

            if (AsString == ">>") 
                return (ushort)(a >> b);

            if (AsString == "&") 
                return (ushort)(a & b);

            if (AsString == "|") 
                return (ushort)(a | b);

            if (AsString == "^") 
                return (ushort)(a ^ b);

            return 0;
        }

        public override void DoCompile(Scope scope, Register target)
        {
            int secondTarget = (int)Register.STACK;

            var secondConstant = (ChildNodes[1] as CompilableNode).IsConstant();
            var firstConstant = (ChildNodes[0] as CompilableNode).IsConstant();

            if (firstConstant && secondConstant)
            {
                AddInstruction("SET", Scope.GetRegisterLabelFirst((int)target), Util.hex(GetConstantValue()));

                if (target == Register.STACK) 
                    scope.stackDepth += 1;

                return;
            }

            if (!secondConstant)
            {
                secondTarget = scope.FindAndUseFreeRegister();
                (ChildNodes[1] as CompilableNode).DoCompile(scope, (Register)secondTarget);
            }

            if (!firstConstant)
                (ChildNodes[0] as CompilableNode).DoCompile(scope, target);

            if (target == Register.STACK)
            {
                AddInstruction("SET", Scope.TempRegister,
                     firstConstant ? Util.hex((ChildNodes[0] as CompilableNode).GetConstantValue()) : "POP");
                AddInstruction(opcodes[AsString], Scope.TempRegister,
                    secondConstant ? Util.hex((ChildNodes[1] as CompilableNode).GetConstantValue()) : Scope.GetRegisterLabelSecond(secondTarget));
                AddInstruction("SET", "PUSH", Scope.TempRegister);
            }
            else
            {
                if (firstConstant)
                    AddInstruction("SET", Scope.GetRegisterLabelFirst((int)target), Util.hex((ChildNodes[0] as CompilableNode).GetConstantValue()));

                AddInstruction(opcodes[AsString], Scope.GetRegisterLabelFirst((int)target),
                    secondConstant ? Util.hex((ChildNodes[1] as CompilableNode).GetConstantValue()) : Scope.GetRegisterLabelSecond(secondTarget));
            }

            if (secondTarget == (int)Register.STACK && !secondConstant)
                scope.stackDepth -= 1;
            else
                scope.FreeMaybeRegister(secondTarget);
        }
    }

    
}
