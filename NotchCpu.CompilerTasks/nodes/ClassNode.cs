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
        AssemblyGen _AssemblyGen;

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
                (child as ClassDeclarationNode).DoPreCompile(_AssemblyGen);
            }
        }

        public override void DoCompile(Scope scope, Register target)
        {
            AddInstruction("SET", "PC", Class.MainFunction.Label);

            foreach (var child in ChildNodes)
            {
                Debug.Assert(child is ClassDeclarationNode);
                (child as ClassDeclarationNode).DoCompile(scope, target);
            }
        }

        public void Compile(String exePath, DCPUC.Assembly assembly)
        {
            var mainFunction = Class.MainFunction;

            if (mainFunction == null)
                throw new Exception("Failed to find main function");

            var exeName = Path.GetFileName(exePath);

            _AssemblyGen = new AssemblyGen(AppDomain.CurrentDomain, new AssemblyName(Path.GetFileNameWithoutExtension(exeName)), AssemblyBuilderAccess.RunAndSave, exeName, true);
            _AssemblyGen.PEFileKind = PEFileKinds.WindowApplication;

            base.Compile(assembly);

            var bin = assembly.GetByteCode();

            //Swap them for cli as it stores them backwards
            for (int x = 0; x < bin.Length; x += 2)
            {
                byte a = bin[x];
                bin[x] = bin[x + 1];
                bin[x + 1] = a;
            }

            CompileCliMainClass(bin);

            _AssemblyGen.Save(exePath, PortableExecutableKinds.Required32Bit, ImageFileMachine.I386);
            _AssemblyGen = null;
        }
    }
}
