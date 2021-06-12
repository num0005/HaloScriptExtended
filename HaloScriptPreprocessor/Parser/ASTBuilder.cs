using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloScriptPreprocessor.Parser
{
    class ASTBuilder
    {
        public ASTBuilder(ParsedExpressions parsedExpressions)
        {
            _parsed = parsedExpressions;
            build();
        }
        private readonly ParsedExpressions _parsed;

        private void build()
        {
            ReadOnlySpan<char> globalSpan      = "global".AsSpan();
            ReadOnlySpan<char> constglobalSpan = "constglobal".AsSpan();
            ReadOnlySpan<char> scriptSpan      = "script".AsSpan();
            foreach (Expression expression in _parsed.Expressions)
            {
                if (expression.Values.Count == 0)
                    throw new UnexpectedExpression(expression.Source, "Unexpected empty expression!");
                Value expressionType = expression.Values[0];
                if (expressionType is not Atom)
                    throw new UnexpectedExpression(expressionType.Source, "Expecting \"global\", \"script\" or \"constglobal\" but got an expression!");
                ReadOnlySpan<char> typeSpan = (expressionType as Atom).Source.Span;
                if (typeSpan.SequenceEqual(globalSpan))
                {
                    // build global AST
                } else if (typeSpan.SequenceEqual(scriptSpan))
                {
                    // build script AST
                } else if (typeSpan.SequenceEqual(constglobalSpan))
                {
                    // build constant global AST
                } else
                {
                    // error
                }
            }
        }

        private void buildScript(Expression expression)
        {
            //ReadOnlySpan<char> 
        }
    }
}
