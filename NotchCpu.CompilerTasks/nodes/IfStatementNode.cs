using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using NotchCpu.CompilerTasks.misc;

namespace DCPUC
{
    public class IfStatementNode : FunctionCompilableNode
    {
        public enum ClauseOrder
        {
            ConstantPass,
            ConstantFail,
            PassFirst,
            FailFirst
        }

        protected override void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            AddChild("Expression", treeNode.ChildNodes[1]);
            AddChild("Block", treeNode.ChildNodes[2]);

            if (treeNode.ChildNodes.Count == 4 && treeNode.ChildNodes[3].ChildNodes.Count() > 0) 
                AddChild("Else", treeNode.ChildNodes[3].FirstChild);

            this.AsString = "If";
        }

        private void releaseRegister(Scope scope, int reg)
        {
            scope.FreeMaybeRegister(reg);

            if (reg == (int)Register.STACK)
                scope.stackDepth -= 1;
        }


        public override void DoCompile(Scope scope, Register target)
        {
            (ChildNodes[0] as CompilableNode).DoCompile(scope, Register.STACK);


            var label = Scope.GetLabel();

            AddInstruction("SET", "PUSH", "0x01");
            AddInstruction("IFN", "POP", "POP", "Check if result is false", Annotation);

            if (ChildNodes.Count() == 3)
                AddInstruction("SET", "PC", label + "_ELSE");
            else
                AddInstruction("SET", "PC", label + "_END");

            (ChildNodes[1] as CompilableNode).DoCompile(scope, Register.DISCARD);

            if (ChildNodes.Count() == 3)
            {
                AddInstruction("SET", "PC", label + "_END");
                AddInstruction(":" + label + "_ELSE", "", "");

                (ChildNodes[2] as CompilableNode).DoCompile(scope, Register.DISCARD);
            }

            AddInstruction("SET", "PC", label + "_END");
            AddInstruction(":" + label + "_END", "", "");
        }




        public ClauseOrder CompileConditional(Scope scope, CompilableNode conditionNode)
        {
            //if (conditionNode is ComparisonNode == false)
            //{
            //    var condTarget = scope.FindAndUseFreeRegister();
            //    conditionNode.Compile(scope, (Register)condTarget);
            //    AddInstruction("IFE", Scope.GetRegisterLabelSecond(condTarget), "0x0", "If from expression");
            //    scope.FreeMaybeRegister(condTarget);
            //    return ClauseOrder.PassFirst;
            //}
            //else
            {
                var op = conditionNode.AsString;
                ushort firstConstantValue = 0, secondConstantValue = 0;

                var firstIsConstant = (conditionNode.ChildNodes[0] as CompilableNode).IsConstant();
                var secondIsConstant = (conditionNode.ChildNodes[1] as CompilableNode).IsConstant();

                int firstRegister = (int)Register.STACK;
                int secondRegister = (int)Register.STACK;

                GetRegister(scope, conditionNode, ref firstConstantValue, firstIsConstant, ref firstRegister);
                GetRegister(scope, conditionNode, ref secondConstantValue, secondIsConstant, ref secondRegister);
                
                if (op == "==")
                    return CompileConditionEqual(scope, firstConstantValue, secondConstantValue, firstIsConstant, secondIsConstant, firstRegister, secondRegister);
                else if (op == "!=")
                    return CompileConditionNotEqual(scope, firstConstantValue, secondConstantValue, firstIsConstant, secondIsConstant, firstRegister, secondRegister);
                else if (op == ">")
                    return CompileConditionGreaterThan(scope, firstConstantValue, secondConstantValue, firstIsConstant, secondIsConstant, firstRegister, secondRegister);
            }

            throw new Exception("Impossible situation reached");
        }

        private static void GetRegister(Scope scope, CompilableNode conditionNode, ref ushort constValue, bool isConst, ref int regiester)
        {
            if (isConst)
            {
                constValue = (conditionNode.ChildNodes[1] as CompilableNode).GetConstantValue();
            }
            else
            {
                regiester = scope.FindAndUseFreeRegister();
                (conditionNode.ChildNodes[1] as CompilableNode).DoCompile(scope, (Register)regiester);
            }
        }

        private ClauseOrder CompileConditionEqual(Scope scope, ushort firstConstantValue, ushort secondConstantValue, bool firstIsConstant, bool secondIsConstant, int firstRegister, int secondRegister)
        {
            if (firstIsConstant && secondIsConstant)
            {
                if (firstConstantValue == secondConstantValue) { return ClauseOrder.ConstantPass; }
                else { return ClauseOrder.ConstantFail; }
            }
            else if (firstIsConstant)
            {
                AddInstruction("IFE", Util.hex(firstConstantValue), Scope.GetRegisterLabelSecond(secondRegister));
                releaseRegister(scope, secondRegister);
                return ClauseOrder.FailFirst;
            }
            else if (secondIsConstant)
            {
                AddInstruction("IFE", Scope.GetRegisterLabelSecond(firstRegister), Util.hex(secondConstantValue));
                releaseRegister(scope, firstRegister);
                return ClauseOrder.FailFirst;
            }
            else
            {
                AddInstruction("IFE", Scope.GetRegisterLabelSecond(firstRegister), Scope.GetRegisterLabelSecond(secondRegister));
                releaseRegister(scope, firstRegister);
                releaseRegister(scope, secondRegister);
                return ClauseOrder.FailFirst;
            }
        }

        private ClauseOrder CompileConditionNotEqual(Scope scope, ushort firstConstantValue, ushort secondConstantValue, bool firstIsConstant, bool secondIsConstant, int firstRegister, int secondRegister)
        {
            if (firstIsConstant && secondIsConstant)
            {
                if (firstConstantValue != secondConstantValue) { return ClauseOrder.ConstantPass; }
                else { return ClauseOrder.ConstantFail; }
            }
            else if (firstIsConstant)
            {
                AddInstruction("IFN", Util.hex(firstConstantValue), Scope.GetRegisterLabelSecond(secondRegister));
                releaseRegister(scope, secondRegister);
                return ClauseOrder.FailFirst;
            }
            else if (secondIsConstant)
            {
                AddInstruction("IFN", Scope.GetRegisterLabelSecond(firstRegister), Util.hex(secondConstantValue));
                releaseRegister(scope, firstRegister);
                return ClauseOrder.FailFirst;
            }
            else
            {
                AddInstruction("IFN", Scope.GetRegisterLabelSecond(firstRegister), Scope.GetRegisterLabelSecond(secondRegister));
                releaseRegister(scope, firstRegister);
                releaseRegister(scope, secondRegister);
                return ClauseOrder.FailFirst;
            }
        }

        private ClauseOrder CompileConditionGreaterThan(Scope scope, ushort firstConstantValue, ushort secondConstantValue, bool firstIsConstant, bool secondIsConstant, int firstRegister, int secondRegister)
        {
            if (firstIsConstant && secondIsConstant)
            {
                if (firstConstantValue > secondConstantValue) { return ClauseOrder.ConstantPass; }
                else { return ClauseOrder.ConstantFail; }
            }
            else if (firstIsConstant)
            {
                AddInstruction("IFG", Util.hex(firstConstantValue), Scope.GetRegisterLabelSecond(secondRegister));
                releaseRegister(scope, secondRegister);
                return ClauseOrder.FailFirst;
            }
            else if (secondIsConstant)
            {
                AddInstruction("IFG", Scope.GetRegisterLabelSecond(firstRegister), Util.hex(secondConstantValue));
                releaseRegister(scope, firstRegister);
                return ClauseOrder.FailFirst;
            }
            else
            {
                AddInstruction("IFG", Scope.GetRegisterLabelSecond(firstRegister), Scope.GetRegisterLabelSecond(secondRegister));
                releaseRegister(scope, firstRegister);
                releaseRegister(scope, secondRegister);
                return ClauseOrder.FailFirst;
            }
        }

        private void CompilePassFirst(Scope scope)
        {
            var elseLabel = Scope.GetLabel() + "ELSE";
            var endLabel = Scope.GetLabel() + "END";

            AddInstruction("SET", "PC", elseLabel);
            CompileBlock(scope, ChildNodes[1]);

            if (ChildNodes.Count == 3)
            {
                AddInstruction("SET", "PC", endLabel);
                AddInstruction(":" + elseLabel, "", "");
                CompileBlock(scope, ChildNodes[2]);
            }

            AddInstruction(":" + endLabel, "", "");
        }

        private void CompileFailFirst(Scope scope)
        {
            var elseLabel = Scope.GetLabel() + "ELSE";
            var endLabel = Scope.GetLabel() + "END";

            if (ChildNodes.Count == 3)
            {
                AddInstruction("SET", "PC", elseLabel);
                CompileBlock(scope, ChildNodes[2]);
                AddInstruction("SET", "PC", endLabel);
                AddInstruction(":" + elseLabel, "", "");
            }
            else
            {
                AddInstruction("SET", "PC", elseLabel);
                AddInstruction("SET", "PC", endLabel);
                AddInstruction(":" + elseLabel, "", "");
            }

            CompileBlock(scope, ChildNodes[1]);
            AddInstruction(":" + endLabel, "", "");
        }

        public void CompileBlock(Scope scope, AstNode block)
        {
            var blockScope = BlockStart(scope);
            (block as CompilableNode).DoCompile(blockScope, Register.DISCARD);
            BlockEnd(blockScope);
        }
    }
}
