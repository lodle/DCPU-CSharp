using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TriAxis.RunSharp;
using System.Diagnostics.SymbolStore;
using DCPUC;

namespace NotchCpu.CompilerTasks.misc
{
    public static class CodeGenEx
    {
        static Dictionary<String, ISymbolDocumentWriter> _DocDict = new Dictionary<string, ISymbolDocumentWriter>();

        private static ISymbolDocumentWriter GetDoc(AssemblyGen assemblyGen, string url)
        {
            if (!_DocDict.ContainsKey(url))
                _DocDict.Add(url, assemblyGen.DefineDocument(url, Guid.Empty, Guid.Empty, Guid.Empty));

            return _DocDict[url];
        }

        public static void MarkSequencePoint(this CodeGen codeGen, AssemblyGen assemblyGen, Annotation ano)
        {
            var loc = ano.span.Location;

            var last = ano.sourcetext.LastIndexOf('\n');

            if (last == -1)
                last = ano.sourcetext.Length;

            codeGen.MarkSequencePoint(CodeGenEx.GetDoc(assemblyGen, ano.file), loc.Line+1, loc.Column, loc.Line+1, last);
        }
    }
}
