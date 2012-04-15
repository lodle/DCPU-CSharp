using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using System.Reflection;
using NotchCpu.CompilerTasks;
using TriAxis.RunSharp;
using NotchCpu.CompilerTasks.misc;
using NotchCpu.Emulator;
using System.Diagnostics;


namespace DCPUC
{
    public class FunctionDeclarationNode : FunctionCompilableNode
    {
        Type _RetType;
        public List<Type> _ParamTypes = new List<Type>();
        public List<String> _ParamNames = new List<String>();
        MethodAttributes _FunAtts;

        public Scope LocalScope = new Scope();
        public String Label;
        public int ParameterCount { get { return _ParamTypes.Count(); } }
        public int References = 0;

        public bool Called { get; set; }

        public virtual bool IsUsed()
        {
            return References > 0;
        }

        public bool HasVoidReturn
        {
            get { return _RetType == typeof(void); }
        }

        public bool IsMain { get; protected set; }
        public ClassDeclarationNode ClassDecl { get; set; }

        protected List<Variable> variables = new List<Variable>();

        public TypeGen TypeGen { get; protected set; }
        public MethodGen MethodGen { get; protected set; }
        public CodeGen CodeGen { get; protected set; }
        public FieldGen IEmulator 
        {
            get
            {
                return ClassDecl.IEmulator;
            }
        }

        public AssemblyGen AssemblyGen
        {
            get
            {
                return ClassDecl.AssemblyGen;
            }
        }

        protected override void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            ParentFunct = this;

            AddChild("Block", treeNode.ChildNodes[5]);

            var parameters = treeNode.ChildNodes[4].ChildNodes;

            for (int i = 0; i < parameters.Count; ++i)
            {
                var variable = new Variable();
                variable.scope = LocalScope;
                variable.name = parameters[i].ChildNodes[1].FindTokenAndGetText();
                variable.type = parameters[i].ChildNodes[0].FindTokenAndGetText();

                LocalScope.variables.Add(variable);

                if (i < 3)
                {
                    variable.location = (Register)i;
                    LocalScope.UseRegister(i);
                }
                else
                {
                    variable.location = Register.STACK;
                    variable.stackOffset = LocalScope.stackDepth;
                    LocalScope.stackDepth += 1;
                }

                variables.Add(variable);
            }

            this.AsString = treeNode.ChildNodes[1].FindTokenAndGetText();
            Label = Scope.GetLabel() + "_" + AsString;
            LocalScope.activeFunction = this;

            Program.RegFunction(this);

            foreach (var v in variables)
            {
                _ParamTypes.Add(Program.GetType(v.type));
                _ParamNames.Add(v.name);
            }

            switch (treeNode.ChildNodes[0].FindTokenAndGetText())
            {
                case "private":
                    _FunAtts |= MethodAttributes.Private;
                    break;

                case "public":
                    _FunAtts |= MethodAttributes.Public;
                    break;

                case "protected":
                    _FunAtts |= MethodAttributes.Family;
                    break;
            }

            _RetType = Program.GetType(treeNode.ChildNodes[2].FindTokenAndGetText());

            bool isStatic = treeNode.ChildNodes[1].FindTokenAndGetText() == "static";

            if (isStatic)
                _FunAtts |= MethodAttributes.Static;

            this.AsString = treeNode.ChildNodes[3].FindTokenAndGetText();
            Label = Scope.GetLabel() + "_{0}_" + AsString;
            LocalScope.activeFunction = this;

            if (AsString == "Main" && isStatic)
            {
                Program.SetMain(this);
                IsMain = true;
                Label = Label.Split('_').First() + "_MAIN";

                References++;

                AsString = "DCPU_" + AsString;
            }
        }

        public override void DoPreCompile()
        {
            base.DoPreCompile();
        }

        public override void DoCompile(Scope scope, Register target)
        {
            CodeGen.MarkSequencePoint(AssemblyGen, Annotation);

            for (int i = 0; i < ParameterCount; ++i)
            {
                var param = Program.GetDefaultValue(_CodeGen, _ParamTypes[i]);
                param.SetLocalSymInfo(_ParamNames[i]);

                var val = Program.GetValue(_CodeGen, _IEmulator, _ParamTypes[i], i);
                CodeGen.Assign(param, val, true);
            }

            if (IsMain)
                CodeGen.Invoke(IEmulator, "RunOnce", 0);

            AddInstruction(":" + Label, "", "", new Annotation(Annotation, AnotationType.Function));

            BlockStart();

            var lScope = LocalScope.Push(new Scope());

            lScope.stackDepth += 1; //account for return address

            foreach (var child in ChildNodes)
            {
                Debug.Assert(child is FunctionCompilableNode);

                var c = child as FunctionCompilableNode;

                c.ParentFunct = this;
                c.DoCompile(lScope, Register.DISCARD);
            }

            CompileReturn(lScope);
            BlockEnd();
        }

        internal void DoPreCompile(TypeGen klass)
        {
            TypeGen = klass;

            if (klass != null)
            {
                MethodGen = klass.Method(_FunAtts, typeof(void), AsString, Type.EmptyTypes);
                CodeGen = MethodGen.Code;
            }
        }

        internal void CompileReturn(Scope lScope)
        {
            if (lScope.stackDepth - LocalScope.stackDepth > 1)
                AddInstruction("ADD", "SP", Util.hex(lScope.stackDepth - LocalScope.stackDepth - 1), "Cleanup stack", null);

            AddInstruction("SET", "PC", "POP", "Return", new Annotation(Annotation, AnotationType.FunctionReturn));

            if (CodeGen != null && !HasVoidReturn)
                CodeGen.Return();
        }
    }
}
