using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace DCPUC
{
    public enum AnotationType
    {
        Barrier,
        OpenScope,
        CloseScope,
        FuncParamater,
        Memory,
        Register,
        FunctionCall,
        Function,
        FunctionReturn,
        Assignment,
        DataLabel,
        While,
    }

    public class Annotation
    {
        public String name;
        public String sourcetext;
        public SourceSpan span;
        public AnotationType type;
        public ushort location;
        public String file;

        public int register { get; set; }

        public bool Ignore
        {
            get
            {
                return type == AnotationType.Barrier && span.Location.Line == -1;
            }
        }

        public Annotation()
        {
            span = new SourceSpan(new SourceLocation(-1, -1, -1), -1);
        }

        public Annotation(ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            this.file = context.CurrentParseTree.FileName;
            this.sourcetext = context.CurrentParseTree.SourceText.Substring(treeNode.Span.Location.Position, treeNode.Span.Length);
            this.span = treeNode.Span;
        }

        public Annotation(String file, SourceSpan span, AnotationType type = AnotationType.Barrier, String name = "", ushort location = 0)
        {
            this.span = span;
            this.location = location;
            this.type = type;
            this.name = name;
        }

        public Annotation(Annotation anotation)
        {
            if (anotation == null)
                return;

            if (anotation != null)
                this.span = anotation.span;
            else
                this.span = new SourceSpan(new SourceLocation(-1, -1, -1), -1);

            this.location = anotation.location;
            this.type = anotation.type;
            this.name = anotation.name;
            this.sourcetext = anotation.sourcetext;
            this.file = anotation.file;
        }

        public Annotation(Annotation anotation, AnotationType anotationType)
        {
            if (anotation == null)
                return;

            if (anotation != null)
                this.span = anotation.span;
            else
                this.span = new SourceSpan(new SourceLocation(-1, -1, -1), -1);

            this.location = anotation.location;
            this.type = anotationType;
            this.name = anotation.name;
            this.sourcetext = anotation.sourcetext;
            this.file = anotation.file;
        }

        public override string ToString()
        {
            if (span.Location.Line == 0)
                return null;

            var ret = String.Format(":{0} {1}-{2}", span.Location.Line+1, span.Location.Column+1, span.Location.Column + span.Length+1);

            if (sourcetext != null && sourcetext.Length > 0)
            {
                var pos = sourcetext.IndexOf("\n");
                pos--;

                if (pos < 0)
                    pos = sourcetext.Length;

                ret += String.Format(" [{0}]", sourcetext.Substring(0, pos));
            }

            return ret;
        }
    }
}
