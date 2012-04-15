using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NotchCpu.CompilerTasks;
using System.Reflection;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            NotchCpuCompilerTask compiler = new NotchCpuCompilerTask();

            //Directory.CreateDirectory("");

            compiler.SourceFiles = new String[] { "NotchTest.cs", "Console.cs" };

            compiler.OutputAssembly = "CompilerTest.exe";
            compiler.BinOutput = "CompilerTest.bin";
            compiler.AsmOutput = "CompilerTest.dcpu";

            compiler.ProjectPath = Directory.GetCurrentDirectory();

            compiler.WriteToConsole = true;
            compiler.Execute();


            //ushort[] _BinCode = new ushort[]
            //{
            //    0x8061,0x8071,0x7C11,0xF100,0x5801,0x001F,
            //    0x800C,0x7DC1,0x003B,0x7C0E,0x00FF,0x7DC1,
            //    0x0014,0x040A,0x0171,0x8000,0x8462,0x8472,
            //    0x7DC1,0x0004,0x0011,0x7C19,0x00FF,0xA017,
            //    0x7C0E,0x01FF,0x7C12,0x0080,0x8462,0x7DC1,
            //    0x0004,0x0170,0x0048,0x0065,0x006C,0x006C,
            //    0x006F,0x0020,0x0077,0x006F,0x0072,0x006C,
            //    0x0064,0x0170,0x002C,0x0020,0x0068,0x006F,
            //    0x0077,0x0020,0x0061,0x0072,0x0065,0x0020,
            //    0x0079,0x006F,0x0075,0x003F,0x0000,0x7DC1,
            //    0x0000,
            //};

            //byte[] byteCode = new byte[_BinCode.Length * 2];

            //for (int x = 0; x < byteCode.Length; x += 2)
            //{
            //    byteCode[x + 1] = (byte)(_BinCode[x / 2] >> 8);
            //    byteCode[x] = (byte)(_BinCode[x / 2] & 0xFF);
            //}

            //var root = new ClassNode();
            //root.CompileCli("abc.exe", byteCode, null);
        }
    }
}
