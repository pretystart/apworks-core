﻿using Irony.Interpreter.Ast;
using Irony.Parsing;
using System;

namespace Apworks.Querying.Parsers.Irony
{
    [Language("Apworks Query Condition Language", "1.0", "The language for a query condition in the Apworks framework.")]
    internal sealed class QueryConditionGrammar : Grammar
    {
        public QueryConditionGrammar()
            : base(false)
        {
            var number = new NumberLiteral("number")
            {
                DefaultIntTypes = new TypeCode[] { TypeCode.Int16, TypeCode.Int32, TypeCode.Int64 },
                DefaultFloatType = TypeCode.Decimal
            };

            var identifier = new IdentifierTerminal("identifier");

            var stringLiteral = new StringLiteral("string", "\"");

            // Defines the property reference on the given target object.
            var propertyReference = new NonTerminal("property");
            propertyReference.Rule = identifier;

            // Defines the logical operator for AND and OR
            var logicalOperator = ToTerm("AND") | "OR";
            logicalOperator.Name = "logical-operator";

            // Defines the unary logical operator for NOT
            var notLogicalOperator = ToTerm("NOT");
            notLogicalOperator.Name = "not-operator";

            // Defines the relation operations
            var relationOperator = ToTerm("EQ") | "NE" | "GE" | "GT" | "LT" | "LE";
            relationOperator.Name = "relation-operator";

            // Defines the binary operators like + - * /
            var binaryOperator = ToTerm("+") | "-" | "*" | "/";
            binaryOperator.Name = "binary-operator";

            var term = new NonTerminal("term");
            term.Rule = number | identifier | stringLiteral;

            // Expressions
            var binaryExpression = new NonTerminal("binary-expression", typeof(BinaryOperationNode));
            var expression = new NonTerminal("expression");
            var parenthesisExpression = new NonTerminal("parenthesis-expression");
            binaryExpression.Rule = expression + binaryOperator + expression;
            parenthesisExpression.Rule = "(" + expression + ")";
            expression.Rule = term | binaryExpression | parenthesisExpression;

            // Relational operations
            var singleRelationalOperation = new NonTerminal("relational-operation");
            var parenthesisRelationOperation = new NonTerminal("parenthesis-relational-operation");
            singleRelationalOperation.Rule = expression + relationOperator + expression;

            // Logical operations and condition
            var logicalOperation = new NonTerminal("logical-operation", typeof(BinaryOperationNode));
            var notLogicalOperation = new NonTerminal("not-operation", typeof(UnaryOperationNode));
            var parenthesisOperation = new NonTerminal("parenthesis-operation");
            var condition = new NonTerminal("condition");
            notLogicalOperation.Rule = notLogicalOperator + "(" + condition + ")";
            logicalOperation.Rule = condition + logicalOperator + condition;
            parenthesisOperation.Rule = "(" + condition + ")";
            condition.Rule = singleRelationalOperation | notLogicalOperation | logicalOperation | parenthesisOperation;

            RegisterOperators(10, "+", "-");
            RegisterOperators(20, "*", "/");

            RegisterOperators(10, "AND", "OR");
            RegisterOperators(20, "NOT");

            MarkPunctuation("(", ")");
            RegisterBracePair("(", ")");
            MarkTransient(condition, expression, term, parenthesisExpression, parenthesisOperation);

            this.Root = condition;
        }
    }
}
