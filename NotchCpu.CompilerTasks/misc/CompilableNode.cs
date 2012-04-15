using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using TriAxis.RunSharp;
using NotchCpu.Emulator;
using System.Diagnostics;
using NotchCpu.CompilerTasks;

namespace DCPUC
{
    public class Util
    {
        private static string hexDigits = "0123456789ABCDEF";

        public static String htoa(int x)
        {
            var s = "";
            while (x > 0)
            {
                s = hexDigits[x % 16] + s;
                x /= 16;
            }
            while (s.Length < 4) s = '0' + s;
            return s;
        }

        public static ushort atoh(string s)
        {
            ushort h = 0;
            s = s.ToUpper();
            for (int i = 0; i < s.Length; ++i)
            {
                h <<= 4;
                ushort d = (ushort)hexDigits.IndexOf(s[i]);
                h += d;
            }
            return h;
        }

        public static String hex(int x) 
        { 
            return "0x" + htoa(x); 
        }

        public static String hex(string x) 
        { 
            return "0x" + htoa(Convert.ToInt16(x)); 
        }
    }



    public abstract class CompilableNode : AstNode
    {
        protected Program Program { get; private set; }

        public Annotation Annotation;
        protected Assembly _Assembly;

        protected abstract void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode);
        public abstract void DoCompile(Scope scope, Register target);


        public virtual void DoPreCompile()
        {
            foreach (var child in ChildNodes)
            {
                var c = child as CompilableNode;

                if (c != null)
                    c.DoPreCompile();
            }  
        }

        public virtual void DoPostCompile()
        {
            foreach (var child in ChildNodes)
            {
                var c = child as CompilableNode;

                if (c != null)
                    c.DoPostCompile();
            }  
        }

        public virtual bool IsConstant() 
        { 
            return false; 
        }

        public virtual bool IsUsed()
        {
            return true;
        }

        public virtual ushort GetConstantValue() 
        { 
            return 0; 
        }

        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            Program = (context.Language.Grammar as CSharpGrammar).ProgramInfo;

            base.Init(context, treeNode);

            Annotation = new Annotation(context, treeNode);
            DoInit(context, treeNode);
        }

        public void PreCompile()
        {
            DoPreCompile();
        }

        public void Compile()
        {
            DoCompile(new Scope(), Register.DISCARD);
        }

        public void PostCompile()
        {
            DoPostCompile();
        }

        public void SetAssembly(Assembly asm)
        {
            _Assembly = asm;
            SetAssembly(asm, ChildNodes);
        }

        protected virtual void SetAssembly(Assembly asm, AstNodeList children)
        {
            foreach (var child in children)
            {
                var c = child as CompilableNode;

                if (c != null)
                    c.SetAssembly(asm);
                else
                    SetAssembly(asm, child.ChildNodes);
            }
        }

        protected virtual AsmInfo AddInstruction(string ins, string a, string b)
        {
            return AddInstruction(ins, a, b, "", null);
        }

        protected virtual AsmInfo AddInstruction(string ins, string a, string b, Annotation ano)
        {
            return AddInstruction(ins, a, b, "", ano);
        }

        protected virtual AsmInfo AddInstruction(string ins, string a, string b, string c, Annotation ano)
        {
            var ret = new AsmInfo(new Instruction(ins, a, b, c), ano);
            _Assembly.Add(ret);
            return ret;
        }

        protected void BlockStart()
        {
            var ano = new Annotation();
            ano.type = AnotationType.OpenScope;
            AddInstruction("NOP", "", "", ano);
        }

        protected Scope BlockStart(Scope scope)
        {
            BlockStart();
            return scope.Push(new Scope());
        }

        protected void BlockEnd()
        {
            var ano = new Annotation();
            ano.type = AnotationType.CloseScope;
            AddInstruction("NOP", "", "", ano);
        }

        protected void BlockEnd(Scope scope)
        {
            if (scope.stackDepth - scope.parentDepth > 0)
                AddInstruction("ADD", "SP", Util.hex(scope.stackDepth - scope.parentDepth), "End block", null);

            BlockEnd();
        }


    }
}
