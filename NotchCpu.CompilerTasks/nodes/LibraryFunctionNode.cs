using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    public class LibraryFunctionNode : FunctionDeclarationNode
    {
        public List<string> code;

        public override void DoCompile(Scope scope, Register target)
        {
            if (References == 0)
                return;

            BlockStart();

            foreach (var line in code)
                AddInstruction(line, "", "");

            BlockEnd();
        }

    }
}
