using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUC;
using TriAxis.RunSharp;
using System.IO;
using System.Reflection.Emit;
using System.Reflection;
using System.Diagnostics;

namespace NotchCpu.CompilerTasks
{
    public partial class ClassNode : CompilableNode
    {
        protected override void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            foreach (var f in treeNode.ChildNodes)
                AddChild("Class Declaration", f);
        }

        public override void DoPreCompile()
        {
            foreach (var child in ChildNodes)
            {
                Debug.Assert(child is ClassDeclarationNode);
                (child as ClassDeclarationNode).DoPreCompile(Program.AssemblyGen);
            }
        }

        public override void DoCompile(Scope scope, Register target)
        {
            if (Program.EmitMainJump)
                AddInstruction("SET", "PC", Program.MainFunction.Label);

            foreach (var child in ChildNodes)
            {
                Debug.Assert(child is ClassDeclarationNode);
                (child as ClassDeclarationNode).DoCompile(scope, target);
            }
        }
    }
}
