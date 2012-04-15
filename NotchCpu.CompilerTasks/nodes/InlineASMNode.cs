using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;
using TriAxis.RunSharp;
using NotchCpu.CompilerTasks.misc;

namespace DCPUC
{
    public class InlineASMNode : FunctionCompilableNode
    {
        protected List<String> _Lines = new List<string>();

        protected override void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            var lines = treeNode.ChildNodes[1].FindTokenAndGetText().Split(new String[2] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            for (int x = 0; x < lines.Count(); x++)
            {
                var line = lines[x];
                line = line.TrimStart();

                var parts = line.Split(';');
                line = "";

                foreach (var part in parts)
                {
                    if (line.Count() > 0)
                        line += ";";

                    line += part;
                }

                if (!String.IsNullOrEmpty(line))
                    _Lines.Add(line);
            }

            if (_Lines.Count() > 0)
                this.AsString = _Lines[0] + "...";
        }

        public override void DoCompile(Scope lScope, Register register)
        {
            BlockStart();

            foreach (var str in _Lines)
            {
                if (str.StartsWith(":"))
                    AddInstruction(str.ToUpper(), "", "", "", _Annotation);
                else
                    ProcessAsmLine(str);

                _Annotation = null;
            }

            BlockEnd();
        }

        private void ProcessAsmLine(string str)
        {
            var op = "";
            var a = "";
            var b = "";
            var c = "";

            var parts = str.Split(' ');

            if (parts.Length < 2)
                throw new Exception("Asm needs at lease a opcode and arg");

            op = parts[0].ToUpper();
            var sub = parts[1].Split(',');

            a = sub[0];
            int rem = 2;

            if (sub.Length > 1 && !String.IsNullOrEmpty(sub[1]))
            {
                b = sub[1];
            }
            else if (parts.Length > 2)
            {
                rem++;
                b = parts[2];
            }

            for (int x = rem; x < parts.Length; x++)
            {
                if (c.Length != 0)
                    c += " ";

                c += parts[x];
            }

            c = c.TrimStart();
            c = c.TrimStart(';');
            c = c.TrimStart();

            AddInstruction(op, MakeUpper(a), MakeUpper(b), c, _Annotation);
        }

        private string MakeUpper(string a)
        {
            var parts = a.Split(new String[]{"0x"}, StringSplitOptions.None);

            var ret = "";
            bool first = true;

            foreach (var p in parts)
            {
                if (!first)
                    ret += "0x";

                ret += p.ToUpper();
                first = false;
            }

            return ret;
        }
    }

    
}
