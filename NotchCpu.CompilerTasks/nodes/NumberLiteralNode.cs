using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class NumberLiteralNode : CompilableNode
    {
        protected override void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            AsString = "";
            foreach (var child in treeNode.ChildNodes)
                AsString += child.FindTokenAndGetText();
        }

        public override bool IsConstant()
        {
            return true;
        }

        public override ushort GetConstantValue()
        {
            if (AsString.StartsWith("0x"))
                return Util.atoh(AsString.Substring(2));
            else
                return Convert.ToUInt16(AsString);
        }

        public override void DoCompile(Scope scope, Register target) 
        {
            if (AsString.StartsWith("0x"))
            {
                var hexPart = AsString.Substring(2).ToUpper();

                while (hexPart.Length < 4) 
                    hexPart = "0" + hexPart;

                AddInstruction("SET", Scope.GetRegisterLabelFirst((int)target), "0x" + hexPart, "Literal", null);
            }
            else
            {
                AddInstruction("SET", Scope.GetRegisterLabelFirst((int)target), Util.hex(AsString), "Literal", null);
            }

            if (target == Register.STACK) 
                scope.stackDepth += 1;
        }
    }
}
