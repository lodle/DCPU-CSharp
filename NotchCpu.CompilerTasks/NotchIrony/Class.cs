using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUC;
using TriAxis.RunSharp;

namespace NotchCpu.CompilerTasks
{
    class Class
    {
        static Stack<String> _Stack = new Stack<String>();

        public static FunctionDeclarationNode MainFunction { get; set; }

        internal static String GetLabel()
        {
            if (_Stack.Count() == 0)
                return "global";

            return _Stack.Peek();
        }

        internal static void Push(string name)
        {
            _Stack.Push(name);
        }

        internal static void Pop()
        {
            _Stack.Pop();
        }

        static List<FunctionDeclarationNode> _FunctionList = new List<FunctionDeclarationNode>();
        static Dictionary<String, ClassDeclarationNode> _ClassLookup = new Dictionary<string, ClassDeclarationNode>();

        internal static void RegFunction(FunctionDeclarationNode functionDeclarationNodeCSharp)
        {
            _FunctionList.Add(functionDeclarationNodeCSharp);
        }

        internal static void RegClass(String name, ClassDeclarationNode c)
        {
            foreach (var funct in _FunctionList)
            {
                c.ChildNodes.Add(funct);
                funct.ClassDecl = c;
            }

            _ClassLookup.Add(name, c);
            _FunctionList.Clear();
        }

        internal static Irony.Interpreter.Ast.AstNode FindClass(string p)
        {
            if (_ClassLookup.ContainsKey(p))
                return _ClassLookup[p];

            return null;
        }

        internal static void SetMain(FunctionDeclarationNode mainFunction)
        {
            if (MainFunction != null)
                throw new Exception("Can only have one static function called main");

            MainFunction = mainFunction;
        }

        internal static void Clear()
        {
            MainFunction = null;
            _FunctionList.Clear();
            _ClassLookup.Clear();
            _Stack.Clear();
        }

        internal static Type GetType(String type)
        {
            Type bType = TryTranslateBasicType(type);

            if (bType != null)
                return bType;

            return null;
        }

        private static Type TryTranslateBasicType(string type)
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

        internal static Operand GetDefaultValue(CodeGen c, Type type)
        {
            if (type == typeof(ushort) || type == typeof(short))
                return c.Local((short)0);

            if (type == typeof(void))
                return c.Local();

            if (type == typeof(string))
                return c.Local("");

            return c.Local();
        }
    }
}
