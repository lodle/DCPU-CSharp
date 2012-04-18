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
            int val = 0;

            if (AsString.StartsWith("0x"))
                val = Util.atoh(AsString);
            else
                val = Int32.Parse(AsString);

            AddInstruction("SET", Scope.GetRegisterLabelFirst((int)target), Util.htoa(val), "Literal", null);

            if (target == Register.STACK) 
                scope.stackDepth += 1;
        }
    }
}
