using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using NotchCpu.CompilerTasks.misc;

namespace DCPUC
{
    public class ComparisonNode : FunctionCompilableNode
    {
        static Dictionary<String, String> opcodes = null;

        Dictionary<String, String> _OpCodeLookup = new Dictionary<string, string>
        {
            {"==", "IFE"},
            {"!=", "IFN"},
            {">", "IFG"},
        };

        protected override void DoInit(ParsingContext context, ParseTreeNode treeNode)
        {
            this.AsString = treeNode.ChildNodes[1].FindTokenAndGetText();

            AddChild("Expression", treeNode.ChildNodes[0]);
            AddChild("Expression", treeNode.ChildNodes[2]);
        }


        public void CompileBasicExpression(String op, Scope scope, Register target, int secondTarget, bool inverse)
        {
            if (target == Register.STACK)
            {
                AddInstruction("SET", Scope.TempRegister, inverse ? "0x01" : "0x0", "Equality onto stack");
                AddInstruction(_OpCodeLookup[op], "POP", Scope.GetRegisterLabelSecond(secondTarget), Annotation);
                AddInstruction("SET", Scope.TempRegister, inverse ? "0x00" : "0x1");
                AddInstruction("SET", "PUSH", Scope.TempRegister);
            }
            else
            {
                AddInstruction("SET", Scope.GetRegisterLabelFirst((int)target), inverse ? "0x01" : "0x0", "Equality into register");
                AddInstruction(_OpCodeLookup[op], "POP", Scope.GetRegisterLabelSecond(secondTarget), Annotation);
                AddInstruction("SET", Scope.GetRegisterLabelFirst((int)target), inverse ? "0x00" : "0x1");
            }
        }


        public override void DoCompile(Scope scope, Register target)
        {
            //Evaluate in reverse in case both need to go on the stack
            var secondTarget = scope.FindFreeRegister();

            if (Scope.IsRegister((Register)secondTarget)) 
                scope.UseRegister(secondTarget);

            var a = ChildNodes[0] as CompilableNode;
            var b = ChildNodes[1] as CompilableNode;
            var op = AsString;

            bool inverse = (op == ">=" || op == "<=");

            if (op == "<=")
            {
                op = ">";
            }
            else if (op == "<" || op == ">=")
            {
                var t = a;
                a = b;
                b = t;

                op = ">";
            }


            if (op == "&&")
            {
                var label = Scope.GetLabel();

                var temp = scope.FindFreeRegister();

                if (Scope.IsRegister((Register)temp)) 
                    scope.UseRegister(temp);

                a.DoCompile(scope, Register.STACK);
                AddInstruction("SET", "PUSH", "0x20");
                AddInstruction("IFE", "POP", "POP", "Check if a is false", Annotation);
                AddInstruction("SET", "PC", label + "_END");

                b.DoCompile(scope, Register.STACK);
                AddInstruction("SET", "PUSH", "0x20");
                AddInstruction("IFE", "POP", "POP", "Check if b is false");
                AddInstruction("SET", "PC", label + "_END");

                PushResult(target, inverse ? "0x00" : "0x1");
                AddInstruction("ADD", "PC", "1");

                AddInstruction(":" + label + "_END", "", "");
                PushResult(target, inverse?"0x01":"0x0");
            }
            else if (op == "||")
            {
                var label = Scope.GetLabel();

                a.DoCompile(scope, Register.STACK);
                AddInstruction("SET", "PUSH", "0x20");
                AddInstruction("IFN", "POP", "POP", "Check if a is not false", Annotation);
                AddInstruction("SET", "PC", label + "_END");

                b.DoCompile(scope, Register.STACK);
                AddInstruction("SET", "PUSH", "0x20");
                AddInstruction("IFN", "POP", "POP", "Check if b is not false");
                AddInstruction("SET", "PC", label + "_END");

                PushResult(target, inverse ? "0x01" : "0x0");
                AddInstruction("ADD", "PC", "1");

                AddInstruction(":" + label + "_END", "", "");
                PushResult(target, inverse ? "0x00" : "0x1");
            }
            else
            {
                b.DoCompile(scope, (Register)secondTarget);
                a.DoCompile(scope, Register.STACK);
                CompileBasicExpression(op, scope, target, secondTarget, inverse);
            }

            if (secondTarget == (int)Register.STACK)
                scope.stackDepth -= 1;
            else
                scope.FreeRegister(secondTarget);
        }

        private void PushResult(Register target, String value)
        {
            if (target == Register.DISCARD)
                AddInstruction("NOP", "", "", "Discarding value");
            else if (target == Register.STACK)
                AddInstruction("SET", "PUSH", value);
            else
                AddInstruction("SET", Scope.GetRegisterLabelFirst((int)target), value);
        }
    }

    
}
