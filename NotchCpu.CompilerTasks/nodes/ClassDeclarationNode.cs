using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUC;
using System.Diagnostics;
using TriAxis.RunSharp;
using System.Reflection;
using NotchCpu.Emulator;

namespace NotchCpu.CompilerTasks
{
    public class ClassDeclarationNode : CompilableNode
    {
        TypeAttributes _ClassAtts;

        public TypeGen TypeGen { get; protected set; }
        public FieldGen IEmulator { get; protected set; }
        public AssemblyGen AssemblyGen { get; protected set; }

        protected override void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            switch (treeNode.ChildNodes[0].FindTokenAndGetText())
            {
                default:
                case "private":
                    _ClassAtts |= TypeAttributes.NotPublic;
                    break;

                case "public":
                    _ClassAtts |= TypeAttributes.Public;
                    break;
            }

            AsString = treeNode.ChildNodes[1].FindTokenAndGetText(); 
            Class.RegClass(AsString, this);
        }

        public override void DoCompile(Scope scope, Register target)
        {
            BlockStart();

            foreach (var child in ChildNodes)
                (child as CompilableNode).DoCompile(scope, Register.DISCARD);

            BlockEnd();
        }

        internal void DoPreCompile(AssemblyGen assemblyGen)
        {
            base.DoPreCompile();

            if (assemblyGen == null)
                return;

            AssemblyGen = assemblyGen;

            TypeGen = assemblyGen.Class(AsString, _ClassAtts);
            IEmulator = TypeGen.Private.Static.Field(typeof(IEmulator), "_Emulator");

            CodeGen con = TypeGen.StaticConstructor();
            {
                con.Assign(IEmulator, Static.Invoke(typeof(Mef), "GetEmulator"));
            }

            foreach (var child in ChildNodes)
            {
                Debug.Assert(child is FunctionDeclarationNode);
                (child as FunctionDeclarationNode).DoPreCompile(TypeGen);
            }

        }

        
    }
}
