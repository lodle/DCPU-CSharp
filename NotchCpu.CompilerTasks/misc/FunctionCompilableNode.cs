using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUC;
using TriAxis.RunSharp;
using System.Diagnostics;

namespace NotchCpu.CompilerTasks.misc
{
    public abstract class FunctionCompilableNode : CompilableNode
    {
        public FunctionDeclarationNode ParentFunct;

        protected FunctionDeclarationNode _ParentFunct
        {
            get
            {
                FunctionCompilableNode node = this;

                while (node.ParentFunct == null)
                {
                    if (node.Parent == null)
                        return null;

                    node = node.Parent as FunctionCompilableNode;
                }

                return node.ParentFunct;
            }
        }

        protected CodeGen _CodeGen
        {
            get
            {
                var par = _ParentFunct;

                if (par == null)
                    return null;

                return par.CodeGen;
            }
        }

        protected FieldGen _IEmulator
        {
            get
            {
                var par = _ParentFunct;

                if (par == null)
                    return null;

                return par.IEmulator;
            }
        }

        protected AssemblyGen _AssemblyGen
        {
            get
            {
                var par = _ParentFunct;

                if (par == null)
                    return null;

                return par.AssemblyGen;
            }
        }

        protected override AsmInfo AddInstruction(string ins, string a, string b)
        {
            return AddInstruction(ins, a, b, "", null);
        }

        protected override AsmInfo AddInstruction(string ins, string a, string b, Annotation ano = null)
        {
            return AddInstruction(ins, a, b, "", ano);
        }

        protected override AsmInfo AddInstruction(string ins, string a, string b, string c, Annotation ano)
        {
            var ret = base.AddInstruction(ins, a, b, c, ano);

            if (_CodeGen != null)
            {
                foreach (var pc in ret.BytesGeneratedPC)
                    _CodeGen.Invoke(_IEmulator, "RunOnce", pc);
            }
            else
            {
                Debug.Assert(false);
            }

            return ret;
        }
    }
}
