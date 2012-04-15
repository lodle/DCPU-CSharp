using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;
using Irony.Parsing;
using System.Reflection;

namespace NotchCpu.CompilerTasks
{
    public class NotchCpuCompilerTask : Task
    {
        /// <summary>
        /// List of C# source files that should be compiled into the assembly
        /// </summary>
        [Required()]
        public string[] SourceFiles { get; set; }

        /// <summary>
        /// Output Assembly (including extension)
        /// </summary>
        [Required()]
        public string OutputAssembly { get; set; }

        public string AsmOutput { get; set; }

        public string BinOutput { get; set; }

        /// <summary>
        /// This should be set to $(MSBuildProjectDirectory)
        /// </summary>
        public string ProjectPath { get; set; }

        public bool WriteToConsole { get; set; }

        public bool ShowSourceInAsm { get; set; }

        public NotchCpuCompilerTask()
        {
            ShowSourceInAsm = true;
        }

        public override bool Execute()
        {
            var prog = new Program();
            List<ClassNode> nodes = new List<ClassNode>();

            foreach (var file in SourceFiles)
            {
                var ext = Path.GetExtension(file);

                if (ext != ".cs")
                    continue;

                var parser = new Irony.Parsing.Parser(new CSharpGrammar(prog));
                ParseTree tree = parser.Parse(File.ReadAllText(file), file);

                if (tree.HasErrors())
                {
                    foreach (var msg in tree.ParserMessages)
                        LogError(msg.Level.ToString(), "", "", file, msg.Location.Line, msg.Location.Column, msg.Location.Line, msg.Location.Column + 10, msg.Message);

                    return false;
                }

                nodes.Add(tree.Root.AstNode as ClassNode);
            }

            var assembly = new DCPUC.Assembly();

            try
            {
                Compile(prog, nodes, assembly);
                return true;
            }
            catch (Exception c)
            {
                LogError(c.Message);
                return false;
            }
        }

        private void Compile(Program prog, List<ClassNode> nodes, DCPUC.Assembly assembly)
        {
            if (String.IsNullOrEmpty(AsmOutput))
                AsmOutput = OutputAssembly.Replace(".exe", ".dcpu");

            if (String.IsNullOrEmpty(BinOutput))
                BinOutput = OutputAssembly.Replace(".exe", ".bin");

            DoCompile(prog, nodes, assembly);

            String[] asm = assembly.GetAsmCode();
            SaveAsm(asm);

            byte[] bin = assembly.GetByteCode();
            SaveBin(bin);

            LogMessage("Asm file location: " + AsmOutput);
            LogMessage("Bin file location: " + BinOutput);
        }

        private void DoCompile(Program prog, List<ClassNode> nodes, DCPUC.Assembly assembly)
        {
            String fileName = Path.GetFileName(OutputAssembly);
            String workingDir = Directory.GetCurrentDirectory();
            String newDir = workingDir + "\\" + Path.GetDirectoryName(OutputAssembly);

            Directory.SetCurrentDirectory(newDir);

            prog.Compile(nodes, OutputAssembly, assembly);

            Directory.SetCurrentDirectory(workingDir);
        }

        private void SaveAsm(String[] asm)
        {
            File.WriteAllLines(AsmOutput, asm);
        }

        public void SaveBin(byte[] bin)
        {
            MemoryStream outfile = new MemoryStream();

            foreach (byte b in bin)
                outfile.WriteByte(b);

            File.WriteAllBytes(BinOutput, outfile.ToArray());
        }

        void LogMessage(string message)
        {
            if (WriteToConsole)
                Console.WriteLine(message);
            else
                Log.LogMessage(message);
        }

        void LogError(string message)
        {
            if (WriteToConsole)
                Console.WriteLine(message);
            else
                Log.LogError(message);
        }

        void LogError(string subcategory, string errorCode, string helpKeyword, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, params object[] messageArgs)
        {
            if (WriteToConsole)
                Console.WriteLine("ERR: " + message);
            else
                Log.LogError(subcategory, errorCode, helpKeyword, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, messageArgs);
        }
      
    }
}
