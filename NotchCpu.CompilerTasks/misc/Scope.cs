﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using NotchCpu.CompilerTasks;
using NotchCpu.CompilerTasks.misc;
using NotchCpu.CompilerTasks.nodes.loops;

namespace DCPUC
{
    public class Variable
    {
        public String name { get; set; }
        public Scope scope { get; set; }
        public int stackOffset { get; set; }
        public Register location { get; set; }
        public String type { get; set; }
    }

    public enum Register
    {
        A = 0,
        B = 1,
        C = 2,
        X = 3,
        Y = 4,
        Z = 5,
        I = 6,
        J = 7,
        STACK = 8,
        DISCARD = 9,
    }

    public enum RegisterState
    {
        Free = 0,
        Used = 1
    }


    public class Scope
    {
        private static int nextLabelID = 0;
        public static String GetLabel()
        {
            return "LABEL" + nextLabelID++;
        }

        private static int nextDataLabelID = 0;
        public static string GetDataLabel()
        {
            return "DAT" + nextDataLabelID++;
        }

        internal const string TempRegister = "J";

        internal Scope parent = null;
        internal int parentDepth = 0;
        public List<Variable> variables = new List<Variable>();
        public int stackDepth = 0;
        public List<FunctionDeclarationNode> pendingFunctions = new List<FunctionDeclarationNode>();
        public FunctionDeclarationNode activeFunction = null;
        internal RegisterState[] registers = new RegisterState[] { RegisterState.Free, 0, 0, 0, 0, 0, 0, RegisterState.Used };

        public static void Reset()
        {
            nextLabelID = 0;
            dataElements.Clear();
        }

        internal Scope Push(Scope child)
        {
            child.parent = this;
            child.stackDepth = stackDepth;
            child.parentDepth = stackDepth;
            child.activeFunction = activeFunction;
            for (int i = 0; i < 8; ++i) child.registers[i] = registers[i];
            return child;
        }

        internal Variable FindVariable(string name)
        {
            foreach (var variable in variables)
                if (variable.name == name) return variable;
            if (parent != null) return parent.FindVariable(name);
            return null;
        }

        internal int FindFreeRegister()
        {
            for (int i = 0; i < 8; ++i) if (registers[i] == RegisterState.Free) return i;
            return (int)Register.STACK;
        }

        internal static string GetRegisterLabelFirst(int r) 
        { 
            if (r == (int)Register.STACK) 
                return "PUSH"; 
            else 
                return ((Register)r).ToString(); 
        }

        internal static string GetRegisterLabelSecond(int r) 
        { 
            if (r == (int)Register.STACK) 
                return "POP"; 
            else 
                return ((Register)r).ToString(); 
        }

        internal void FreeRegister(int r) 
        { 
            registers[r] = RegisterState.Free; 
        }

        public void UseRegister(int r) 
        { 
            registers[r] = RegisterState.Used; 
        }

        internal static bool IsRegister(Register r) 
        { 
            return (int)(r) <= 7; 
        }

        internal RegisterState[] SaveRegisterState()
        {
            var r = new RegisterState[8];
            for (int i = 0; i < 8; ++i) r[i] = registers[i];
            return r;
        }

        internal void RestoreRegisterState(RegisterState[] state)
        {
            registers = state;
        }

        internal int FindAndUseFreeRegister()
        {
            var r = FindFreeRegister();
            if (IsRegister((Register)r)) UseRegister(r);
            return r;
        }

        internal void FreeMaybeRegister(int r) 
        { 
            if (IsRegister((Register)r)) 
                FreeRegister(r); 
        }

        public static List<Tuple<string, List<ushort>>> dataElements = new List<Tuple<string, List<ushort>>>();

        public static void AddData(string label, List<ushort> data)
        {
            dataElements.Add(new Tuple<string, List<ushort>>(label, data));
        }


        Stack<LoopFunctionCompilableNode> _LoopStack = new Stack<LoopFunctionCompilableNode>();

        internal LoopFunctionCompilableNode PeekLoop()
        {
            if (_LoopStack.Count == 0 && parent != null)
                return parent.PeekLoop();

            return _LoopStack.Peek();
        }

        internal void PushLoop(LoopFunctionCompilableNode loopNode)
        {
            _LoopStack.Push(loopNode);
        }

        internal void PopLoop()
        {
            _LoopStack.Pop();
        }
    }
}
