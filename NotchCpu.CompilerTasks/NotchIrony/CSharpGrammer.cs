using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;
using System.Globalization;
using DCPUC;

namespace NotchCpu.CompilerTasks
{
    [Language("c#", "3.5", "Sample c# grammar")]
    public class CSharpGrammar : Irony.Parsing.Grammar
    {


        TerminalSet _skipTokensInPreview = new TerminalSet(); //used in token preview for conflict resolution
        public CSharpGrammar()
        {

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
            //var comparison = new NonTerminal("Comparison", typeof(ComparisonNode));
            var @operator = ToTerm("+") | "-" | "*" | "/" | "%" | "&" | "|" | "^";
            var comparisonOperator = ToTerm("==") | "!=" | ">";
            var variableDeclaration = new NonTerminal("Variable Declaration", typeof(VariableDeclarationNode));
            var statement = new NonTerminal("Statement");

            var statementList = new NonTerminal("Statement List", typeof(BlockNode));
            var functionList = new NonTerminal("Function List", typeof(FunctionNode));
            var classList = new NonTerminal("Class List", typeof(ClassNode));

            var assignment = new NonTerminal("Assignment", typeof(AssignmentNode));
            //var ifStatement = new NonTerminal("If", typeof(IfStatementNode));
            var block = new NonTerminal("Block");
            var functionblock = new NonTerminal("FunctionBlock");
            //var ifElseStatement = new NonTerminal("IfElse", typeof(IfStatementNode));
            var parameterList = new NonTerminal("Parameter List");
            var classDeclaration = new NonTerminal("Class Declaration", typeof(ClassDeclarationNode));
            var functionDeclaration = new NonTerminal("Function Declaration", typeof(FunctionDeclarationNode));
            
            var parameterDeclaration = new NonTerminal("Parameter Declaration");
            var parameterListDeclaration = new NonTerminal("Parameter Declaration List");
            var returnStatement = new NonTerminal("Return", typeof(ReturnStatementNode));

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
                | dataLiteral;

            assignment.Rule = identifier + "=" + expression;
            binaryOperation.Rule = expression + @operator + expression;
            //comparison.Rule = expression + comparisonOperator + expression;
            parenExpression.Rule = ToTerm("(") + expression + ")";
            variableDeclaration.Rule = types + identifier + "=" + (expression | dataLiteral);

            statement.Rule = inlineASM 
                | (variableDeclaration + ";")
                | (assignment + ";") 
                | block 
                | functionDeclaration
                | (staticFunctionCall + ";")
                | (functionCall + ";")
                | (returnStatement + ";");

            block.Rule = ToTerm("{") + statementList + "}";
            functionblock.Rule = ToTerm("{") + functionList + "}";

            inlineASM.Rule = ToTerm("asm") + "{" + new FreeTextLiteral("inline asm", "}") + "}";
            //ifStatement.Rule = ToTerm("if") + "(" + (expression | comparison) + ")" + statement;
            //ifElseStatement.Rule = ToTerm("if") + "(" + expression + ")" + statement + this.PreferShiftHere() + "else" + statement;
            
            parameterList.Rule = MakeStarRule(parameterList, ToTerm(","), expression);
            functionCall.Rule = identifier + "(" + parameterList + ")";
            staticFunctionCall.Rule = identifier + "." + functionCall;
            
            parameterDeclaration.Rule = types + identifier;
            parameterListDeclaration.Rule = MakeStarRule(parameterListDeclaration, ToTerm(","), parameterDeclaration);

            functionDeclaration.Rule = protection + (ToTerm("static")|"") + retTypes + identifier + "(" + parameterListDeclaration + ")" + block;
            

            returnStatement.Rule = ToTerm("return") + expression;

            classDeclaration.Rule = ( protection | "" ) + ToTerm("class") + identifier + functionblock;

            functionList.Rule = MakeStarRule(functionList, functionDeclaration);
            statementList.Rule = MakeStarRule(statementList, statement);
            classList.Rule = MakeStarRule(classList, classDeclaration);

            this.Root = classList;

            this.RegisterBracePair("[", "]");
            this.Delimiters = "{}[](),:;+-*/%&|^!~<>=.";
            this.MarkPunctuation(";", ",", "(", ")", "{", "}", "[", "]", ":", "class", ".");
            this.MarkTransient(expression, parenExpression, statement, block, functionblock);//, parameterList);

            this.RegisterOperators(1, Associativity.Right, "==", "!=");
            this.RegisterOperators(2, Associativity.Right, "=");
            this.RegisterOperators(3, Associativity.Left, "+", "-");
            this.RegisterOperators(4, Associativity.Left, "*", "/");

            this.RegisterOperators(6, Associativity.Left, "[", "]", "<", ">");
        }
    }
}
