using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using System.Resources;
using System.IO;
using DCPUC;
using TriAxis.RunSharp;
using NotchCpu.Emulator;
using System.Diagnostics.SymbolStore;
using Irony.Parsing;
using System.ComponentModel.Composition;


namespace NotchCpu.CompilerTasks
{
    public partial class ClassNode
    {

        private void CompileCliMainClass(byte[] byteCode)
        {
            var progClass = _AssemblyGen.Public.Class("Program", typeof(IDCPUProgram));
            {
                progClass.Attribute(typeof(ExportAttribute), new Type[]{typeof(Type)}, typeof(IDCPUProgram));

                var binData = progClass.Field(typeof(ushort[]), "_binData");
                var staticBinData = progClass.StaticField(typeof(ushort[]), byteCode);

                var con = progClass.Public.Constructor();
                {
                    con.Code.Assign(binData, Exp.NewStaticInitializedArray(typeof(ushort), byteCode.Length / 2, staticBinData as StaticFieldGen));
                }

                MethodGen main = progClass.Public.Static.Method("Main");
                {
                    var g = main.Code;

                    g.Invoke(typeof(Application), "EnableVisualStyles");
                    g.Invoke(typeof(Application), "SetCompatibleTextRenderingDefault", true);
                    Operand emu = g.Local(Static.Invoke(typeof(Mef), "GetEmulator"));
                    Operand form = emu.Invoke("GetMainForm");
                    g.Invoke(typeof(Application), "Run", form);
                };

                main.Attribute(typeof(STAThreadAttribute));

                CodeGen getDebug = progClass.Private.Virtual.Method(typeof(ushort[]), "GetDebugBinary");
                {
                    getDebug.Return(binData);
                }

                CodeGen getRelease = progClass.Private.Virtual.Method(typeof(ushort[]), "GetReleaseBinary");
                {
                    getRelease.Return(binData);
                }

                CodeGen run = progClass.Private.Virtual.Method("Run");
                {
                    run.Invoke(Class.MainFunction.ClassDecl.TypeGen, "Main");
                }
            }
        }


    }
}
