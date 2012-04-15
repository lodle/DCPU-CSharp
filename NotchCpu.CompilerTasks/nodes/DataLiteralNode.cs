using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class DataLiteralNode : CompilableNode
    {
        List<ushort> data = new List<ushort>();
        string dataLabel;

        protected override void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            foreach (var child in treeNode.ChildNodes)
            {
                var token = child.FindTokenAndGetText();

                if (token[0] == '\"')
                {
                    foreach (var c in token.Substring(1, token.Length - 2))
                        data.Add((ushort)c);

                    data.Add('\0');
                }
                else if (token.StartsWith("0x"))
                {
                    data.Add(Util.atoh(token.Substring(2)));
                }
                else
                {
                    data.Add(Convert.ToUInt16(token));
                }
            }

            dataLabel = Scope.GetDataLabel();

            Annotation = new Annotation(context, treeNode);
            Annotation.type = AnotationType.DataLabel;
        }

        public override bool IsConstant()
        {
            return data.Count == 1;
        }

        public override ushort GetConstantValue()
        {
            return data[0];
        }

        public override void DoCompile(Scope scope, Register target) 
        {
            AddInstruction("SET", Scope.GetRegisterLabelFirst((int)target), dataLabel, Annotation);
            Scope.AddData(dataLabel, data);

            if (target == Register.STACK) 
                scope.stackDepth += 1;
        }
    }

    
}
