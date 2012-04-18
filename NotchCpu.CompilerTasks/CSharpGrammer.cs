using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;
using System.Globalization;
using DCPUC;
using NotchCpu.CompilerTasks.nodes.loops;

namespace NotchCpu.CompilerTasks
{
    [Language("c#", "3.5", "Sample c# grammar")]
    public class CSharpGrammar : Irony.Parsing.Grammar
    {
        public Program ProgramInfo { get; protected set; }

        TerminalSet _skipTokensInPreview = new TerminalSet(); //used in token preview for conflict resolution

        public CSharpGrammar()
            : this(new Program())
        {
        }

        public CSharpGrammar(Program programInfo)
        {
            ProgramInfo = programInfo;


            this.LanguageFlags |= Irony.Parsing.LanguageFlags.CreateAst;

            var comment = new CommentTerminal("comment", "//", "\n", "\r\n");
            NonGrammarTerminals.Add(comment);

            var integerLiteral = new NumberLiteral("integer", NumberOptions.IntOnly);
            integerLiteral.AddPrefix("0x", NumberOptions.Hex);

            var identifier = TerminalFactory.CreateCSharpIdentifier("identifier");
            identifier.AstNodeType = typeof(VariableNameNode);

            var stringLiteral = new StringLiteral("string", "\"");

            var inlineASM = new NonTerminal("inline", typeof(InlineASMNode));

            var numberLiteral = new NonTerminal("Number", typeof(NumberLiteralNode));
            var dataLiteral = new NonTerminal("Data", typeof(DataLiteralNode));
            var expression = new NonTerminal("Expression");
            var parenExpression = new NonTerminal("Paren Expression");
            var binaryOperation = new NonTerminal("Binary Operation", typeof(BinaryOperationNode));
            var comparison = new NonTerminal("Comparison", typeof(ComparisonNode));
            var @operator = ToTerm("+") | "-" | "*" | "/" | "%" | "&" | "|" | "^";
            var comparisonOperator = ToTerm("==") | "!=" | ">" | "<" | ">=" | "<=" | "&&" | "||";
            var variableDeclaration = new NonTerminal("Variable Declaration", typeof(VariableDeclarationNode));
            var statement = new NonTerminal("Statement");

            var statementList = new NonTerminal("Statement List", typeof(BlockNode));
            var functionList = new NonTerminal("Function List", typeof(FunctionNode));
            var classList = new NonTerminal("Class List", typeof(ClassNode));

            var assignment = new NonTerminal("Assignment", typeof(AssignmentNode));
            var ifStatement = new NonTerminal("If", typeof(IfStatementNode));
            var block = new NonTerminal("Block");
            var functionblock = new NonTerminal("FunctionBlock");
            var parameterList = new NonTerminal("Parameter List");
            var classDeclaration = new NonTerminal("Class Declaration", typeof(ClassDeclarationNode));
            var functionDeclaration = new NonTerminal("Function Declaration", typeof(FunctionDeclarationNode));
            
            var parameterDeclaration = new NonTerminal("Parameter Declaration");
            var parameterListDeclaration = new NonTerminal("Parameter Declaration List");
            var returnStatement = new NonTerminal("Return", typeof(ReturnStatementNode));

            var continueStatement = new NonTerminal("Continue", typeof(ContinueStatementNode));
            var breakStatement = new NonTerminal("Break", typeof(BreakStatementNode));
            var forLoop = new NonTerminal("For", typeof(ForLoopNode));

            var whileLoop = new NonTerminal("While", typeof(WhileLoopNode));
            var doWhileLoop = new NonTerminal("Do While", typeof(DoWhileLoopNode));

            var functionCall = new NonTerminal("Function Call", typeof(FunctionCallNodeEx));
            var staticFunctionCall = new NonTerminal("Static Function Call", typeof(FunctionCallNodeEx));

            var types = ToTerm("short") | "char" | "int" | "string";
            var retTypes = ToTerm("void") | types;
            var protection = ToTerm("public") | "protected" | "private";

            numberLiteral.Rule = integerLiteral;
            dataLiteral.Rule = MakePlusRule(dataLiteral, (numberLiteral | stringLiteral));

            expression.Rule = numberLiteral 
                | binaryOperation 
                | parenExpression 
                | identifier
                | staticFunctionCall
                | functionCall 
                | dataLiteral
                | comparison;

            assignment.Rule = identifier + "=" + expression;
            binaryOperation.Rule = expression + @operator + expression;
            comparison.Rule = expression + comparisonOperator + expression;
            parenExpression.Rule = ToTerm("(") + expression + ")";
            variableDeclaration.Rule = types + identifier + "=" + (expression | dataLiteral);

            statement.Rule = inlineASM
                | (variableDeclaration + ";")
                | (assignment + ";")
                | block
                | functionDeclaration
                | (staticFunctionCall + ";")
                | ifStatement
                | (functionCall + ";")
                | (returnStatement + ";")
                | (expression + ";")
                | (doWhileLoop + ";")
                | whileLoop
                | forLoop
                | (breakStatement + ";")
                | (continueStatement + ";");

            block.Rule = ToTerm("{") + statementList + "}";
            functionblock.Rule = ToTerm("{") + functionList + "}";

            inlineASM.Rule = ToTerm("asm") + "{" + new FreeTextLiteral("inline asm", "}") + "}";
            ifStatement.Rule = ToTerm("if") + "(" + expression + ")" + statement + (Empty | (PreferShiftHere() + "else" + statement));
      
            parameterList.Rule = MakeStarRule(parameterList, ToTerm(","), expression);
            functionCall.Rule = identifier + "(" + parameterList + ")";
            staticFunctionCall.Rule = identifier + "." + functionCall;
            
            parameterDeclaration.Rule = types + identifier;
            parameterListDeclaration.Rule = MakeStarRule(parameterListDeclaration, ToTerm(","), parameterDeclaration);

            continueStatement.Rule = ToTerm("continue");
            breakStatement.Rule = ToTerm("break");
            forLoop.Rule = ToTerm("for") + "(" + (variableDeclaration | assignment | Empty) + ";" + (expression | Empty) + ";" + (assignment | Empty) + ")" + statement;
            whileLoop.Rule = ToTerm("while") + "(" + expression + ")" + statement;
            doWhileLoop.Rule = ToTerm("do") + statement + "while" + "(" + expression + ")";

            functionDeclaration.Rule = protection + ToTerm("static") + retTypes + identifier + "(" + parameterListDeclaration + ")" + block;
            returnStatement.Rule = ToTerm("return") + expression;

            classDeclaration.Rule = protection  + ToTerm("class") + identifier + functionblock;

            functionList.Rule = MakeStarRule(functionList, functionDeclaration);
            statementList.Rule = MakeStarRule(statementList, statement);
            classList.Rule = MakeStarRule(classList, classDeclaration);

            this.Root = classList;

            this.RegisterBracePair("[", "]");
            this.Delimiters = "{}[](),:;+-*/%&|^!~<>=.";
            this.MarkPunctuation(";", ",", "(", ")", "{", "}", "[", "]", ":", "class", ".", "else", "while", "do", "for", "break", "continue");
            this.MarkTransient(expression, parenExpression, statement, block, functionblock);//, parameterList);

            this.RegisterOperators(1, Associativity.Right, "==", "!=");
            this.RegisterOperators(2, Associativity.Right, "=");
            this.RegisterOperators(3, Associativity.Left, "+", "-");
            this.RegisterOperators(4, Associativity.Left, "*", "/");

            this.RegisterOperators(6, Associativity.Left, "[", "]", "<", ">");
        }
    }
}
