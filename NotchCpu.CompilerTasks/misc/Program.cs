using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUC;
using TriAxis.RunSharp;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using NotchCpu.Emulator;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using Irony.Interpreter.Ast;

namespace NotchCpu.CompilerTasks
{
    public class Program
    {
        
        Stack<String> _Stack = new Stack<String>();
        List<FunctionDeclarationNode> _FunctionList = new List<FunctionDeclarationNode>();
        Dictionary<String, ClassDeclarationNode> _ClassLookup = new Dictionary<string, ClassDeclarationNode>();

        bool _EmitMainJump;

        public AssemblyGen AssemblyGen { get; protected set; }
        public FunctionDeclarationNode MainFunction { get; protected set; }
        public bool EmitMainJump 
        { 
            get
            {
                if (_EmitMainJump)
                {
                    _EmitMainJump = false;
                    return true;
                }

                return false;
            }
        }

        internal String GetClassLabel()
        {
            if (_Stack.Count() == 0)
                return "global";

            return _Stack.Peek();
        }

        internal void PushClass(string name)
        {
            _Stack.Push(name);
        }

        internal void PopClass()
        {
            _Stack.Pop();
        }

        internal void RegFunction(FunctionDeclarationNode functionDeclarationNodeCSharp)
        {
            _FunctionList.Add(functionDeclarationNodeCSharp);
        }

        internal void RegClass(String name, ClassDeclarationNode c)
        {
            foreach (var funct in _FunctionList)
            {
                c.ChildNodes.Add(funct);
                funct.ClassDecl = c;
            }

            _ClassLookup.Add(name, c);
            _FunctionList.Clear();
        }

        internal AstNode FindClass(string p)
        {
            if (_ClassLookup.ContainsKey(p))
                return _ClassLookup[p];

            return null;
        }

        internal void SetMain(FunctionDeclarationNode mainFunction)
        {
            if (MainFunction != null)
                throw new Exception("Can only have one static function called main");

            _EmitMainJump = true;
            MainFunction = mainFunction;
        }

        internal void Reset()
        {
            MainFunction = null;
            _FunctionList.Clear();
            _ClassLookup.Clear();
            _Stack.Clear();
        }

        internal Type GetType(String type)
        {
            Type bType = TryTranslateBasicType(type);

            if (bType != null)
                return bType;

            return null;
        }

        private Type TryTranslateBasicType(string type)
        {
            switch (type)
            {
                case "short":
                    return typeof(short);
                    
                case "void":
                    return typeof(void);

                case "ushort":
                    return typeof(ushort);

                case "string":
                    return typeof(string);
            }

            return null;
        }

        internal Operand GetDefaultValue(CodeGen c, Type type)
        {
            if (type == typeof(ushort))
                return c.Local((ushort)0);

            if (type == typeof(short))
                return c.Local((short)0);

            if (type == typeof(void))
                return c.Local();

            if (type == typeof(string))
                return c.Local("");

            return c.Local();
        }

        public void Compile(List<ClassNode> classes, String exePath, DCPUC.Assembly assembly)
        {
            if (MainFunction == null)
                throw new Exception("Failed to find main function");

            var exeName = Path.GetFileName(exePath);

            AssemblyGen = new AssemblyGen(AppDomain.CurrentDomain, new AssemblyName(Path.GetFileNameWithoutExtension(exeName)), AssemblyBuilderAccess.RunAndSave, exeName, true, true);
            AssemblyGen.PEFileKind = PEFileKinds.WindowApplication;

            assembly.Clear();

            foreach (var klass in classes)
            {
                klass.SetAssembly(assembly);
                klass.PreCompile();
            }

            foreach (var klass in classes)
                klass.Compile();

            foreach (var klass in classes)
            {
                klass.PostCompile();
                klass.SetAssembly(null);
            }

            assembly.Finalise();

            var bin = assembly.GetByteCode();

            //Swap them for cli as it stores them backwards
            for (int x = 0; x < bin.Length; x += 2)
            {
                byte a = bin[x];
                bin[x] = bin[x + 1];
                bin[x + 1] = a;
            }

            CompileCliMainClass(bin);

            AssemblyGen.Save(exeName, PortableExecutableKinds.Required32Bit, ImageFileMachine.I386);
            AssemblyGen = null;
        }

        private void CompileCliMainClass(byte[] byteCode)
        {
            var progClass = AssemblyGen.Public.Class("Program", typeof(IDCPUProgram));
            {
                progClass.Attribute(typeof(ExportAttribute), new Type[] { typeof(Type) }, typeof(IDCPUProgram));

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
                    run.Invoke(MainFunction.ClassDecl.TypeGen, "DCPU_Main");
                }
            }
        }

        internal static Operand GetValue(CodeGen _CodeGen, FieldGen _IEmulator, Type type, int reg)
        {
            var funct = "GetIntValue";

            if (type == typeof(string))
                funct = "GetStringValue";

            //return Static.Invoke(typeof(IEmulator), _IEmulator, funct, reg);
            return _IEmulator.Invoke(funct, reg);
        }
    }
}
