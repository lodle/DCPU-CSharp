using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using NotchCpu.CompilerTasks.misc;
using TriAxis.RunSharp;

namespace DCPUC
{
    public class FunctionCallNode : FunctionCompilableNode
    {
        FunctionDeclarationNode _Func;

        protected override void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            AsString = treeNode.ChildNodes[0].FindTokenAndGetText();

            foreach (var parameter in treeNode.ChildNodes[1].ChildNodes)
                AddChild("parameter" + parameter.ToString(), parameter);
        }

        protected virtual FunctionDeclarationNode FindFunction(AstNode node, string name)
        {
            if (node == null)
                return null;

            foreach (var child in node.ChildNodes)
                if (child is FunctionDeclarationNode && (child as FunctionDeclarationNode).AsString == name)
                    return child as FunctionDeclarationNode;
            if (node.Parent != null) return FindFunction(node.Parent, name);
            return null;
        }

        private void PushRegister(Scope scope, Register r)
        {
            AddInstruction("SET", "PUSH", Scope.GetRegisterLabelSecond((int)r), "Saving register", null);
            scope.FreeRegister(0);
            scope.stackDepth += 1;
        }

        public override void DoPreCompile()
        {
            base.DoPreCompile();

            _Func = FindFunction(this, AsString);

            if (_Func == null)
                throw new Exception("Can't find function - " + AsString);

            if (_Func.ParameterCount != ChildNodes.Count)
                throw new Exception("Incorrect number of arguments - " + AsString);

            _Func.References += 1;
        }

        public override void DoCompile(Scope scope, Register target)
        {
            if (_Func == null)
                throw new Exception("Can't find function - " + AsString);

            if (!IsUsed())
                return;

            //Marshall registers
            var startingRegisterState = scope.SaveRegisterState();

            for (int i = 0; i < 3; ++i)
            {
                if (startingRegisterState[i] == RegisterState.Used)
                {
                    PushRegister(scope, (Register)i);

                    if (scope.activeFunction != null && scope.activeFunction.ParameterCount > i)
                    {
                        scope.activeFunction.LocalScope.variables[i].location = Register.STACK;
                        scope.activeFunction.LocalScope.variables[i].stackOffset = scope.stackDepth - 1;
                    }
                }

                if (_Func.ParameterCount > i)
                {
                    scope.UseRegister(i);
                    (ChildNodes[i] as CompilableNode).DoCompile(scope, (Register)i);
                }
            }

            for (int i = 3; i < 7; ++i)
            {
                if (startingRegisterState[i] == RegisterState.Used)
                {
                    PushRegister(scope, (Register)i);
                }
            }

            if (_Func.ParameterCount > 3)
            {
                for (int i = 3; i < _Func.ParameterCount; ++i)
                {
                    (ChildNodes[i] as CompilableNode).DoCompile(scope, Register.STACK);
                }
            }

            _Annotation.type = AnotationType.FunctionCall;
            AddInstruction("JSR", _Func.Label, "", "Calling function", _Annotation);

            if (_CodeGen != null)
            {
                _CodeGen.MarkSequencePoint(_AssemblyGen, _Annotation);
                _CodeGen.Invoke(_Func.ClassDecl.TypeGen, _Func.AsString.Split('.').Last());
            }

            if (_Func.ParameterCount > 3) //Need to remove parameters from stack
            {
                AddInstruction("ADD", "SP", Util.hex(_Func.ParameterCount - 3), "Remove parameters", null);
                scope.stackDepth -= (_Func.ParameterCount - 3);
            }

            if (scope.activeFunction != null)
            {
                for (int i = 0; i < 3 && i < scope.activeFunction.ParameterCount; ++i)
                {
                    scope.activeFunction.LocalScope.variables[i].location = (Register)i;
                }
            }

            var saveA = startingRegisterState[0] == RegisterState.Used;

            if (saveA && target != Register.DISCARD)
                AddInstruction("SET", Scope.TempRegister, "A", "Save return value from being overwritten by stored register", null);

            for (int i = 6; i >= 0; --i)
            {
                if (startingRegisterState[i] == RegisterState.Used)
                {
                    AddInstruction("SET", Scope.GetRegisterLabelFirst(i), "POP", "Restoring register", null);
                    scope.UseRegister(i);
                    scope.stackDepth -= 1;
                }
            }

            if (target == Register.A && !saveA)
            {
                return;
            }
            else if (Scope.IsRegister(target))
            {
                AddInstruction("SET", Scope.GetRegisterLabelFirst((int)target), saveA ? Scope.TempRegister : "A");
            }
            else if (target == Register.STACK)
            {
                AddInstruction("SET", "PUSH", saveA ? Scope.TempRegister : "A", "Put return value on stack", null);
                scope.stackDepth += 1;
            }
        }
    }
}
