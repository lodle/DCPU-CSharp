using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCPUC;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace NotchCpu.CompilerTasks
{
    class FunctionCallNodeEx : DCPUC.FunctionCallNode
    {
        FunctionCallNodeEx _RealFunctionCall;

        protected override FunctionDeclarationNode FindFunction(AstNode node, string name)
        {
            String [] parts = name.Split('.');

            FunctionDeclarationNode ret = null;

            if (parts.Count() == 1)
                ret = base.FindFunction(node, name);
            else
                ret = base.FindFunction(Program.FindClass(parts[0]), parts[1]);

            if (ret != null)
                ret.Label = String.Format(ret.Label, (ret as FunctionDeclarationNode).ClassDecl.AsString).ToUpper();

            return ret;
        }

        protected override void SetAssembly(Assembly asm, AstNodeList children)
        {
            if (_RealFunctionCall != null)
                _RealFunctionCall.SetAssembly(asm);

            base.SetAssembly(asm, children);
        }

        protected override void DoInit(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            if (treeNode.ChildNodes[1].AstNode is FunctionCallNodeEx)
            {
                _RealFunctionCall = treeNode.ChildNodes[1].AstNode as FunctionCallNodeEx;
                _RealFunctionCall.AsString = treeNode.ChildNodes[0].FindTokenAndGetText() + "." + _RealFunctionCall.AsString;
                _RealFunctionCall.Parent = this;


                this.AsString = "Proxy -> " + _RealFunctionCall.AsString;
            }
            else
            {
                base.DoInit(context, treeNode);
            }
        }

        public override void DoPreCompile()
        {
            if (_RealFunctionCall != null)
            {
                _RealFunctionCall.DoPreCompile();

                var old = _RealFunctionCall.Annotation.span.Location;
                var loc = new SourceLocation(Annotation.span.Location.Position, old.Line, Annotation.span.Location.Column);

                _RealFunctionCall.Annotation.sourcetext = Annotation.sourcetext + _RealFunctionCall.Annotation.sourcetext;
                _RealFunctionCall.Annotation.span = new SourceSpan(loc, _RealFunctionCall.Annotation.span.Length + Annotation.span.Length);
            }
            else
            {
                base.DoPreCompile();
            }
        }

        public override void DoCompile(Scope scope, Register target)
        {
            if (_RealFunctionCall != null)
                _RealFunctionCall.DoCompile(scope, target);
            else
                base.DoCompile(scope, target);
        }
    }
}
