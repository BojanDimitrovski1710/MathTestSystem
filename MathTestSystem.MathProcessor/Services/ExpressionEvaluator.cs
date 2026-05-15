using MathTestSystem.MathProcessor.Interfaces;
using MathTestSystem.MathProcessor.Models;
using System.Collections;
using System.Globalization;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MathTestSystem.MathProcessor.Services;

/// <summary>
/// Evaluates arithmetic expressions using the Shunting-Yard algorithm.
/// Supports +, -, *, / with correct operator precedence and parentheses.
/// 
/// The algorythm works as following:
///  While there are tokens to be read:
///       Read a token
///       ->If it's a number add it to queue
///       ->If it's an operator
///               While there's an operator on the top of the stack with greater precedence:
///                       Pop operators from the stack onto the output queue
///               Push the current operator onto the stack
///        ->If it's a left bracket push it onto the stack
///        ->If it's a right bracket 
///            While there's not a left bracket at the top of the stack:
///                     Pop operators from the stack onto the output queue.
///             Pop the left bracket from the stack and discard it
///  While there are operators on the stack, pop them to the queue
/// </summary>
public class ExpressionEvaluator : IExpressionEvaluator
{
    private static readonly Dictionary<char, int> Precedence = new()
    {
        ['+'] = 1,
        ['-'] = 1,
        ['*'] = 2,
        ['/'] = 2,
    };

    public EvaluationResult Evaluate(string expression)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(expression))
                return EvaluationResult.Fail("Expression cannot be empty.");

            List<string> tokens = Tokenize(expression);
            Queue<string> postfix = ToPostfix(tokens);
            decimal result = EvaluatePostfix(postfix);
            return EvaluationResult.Ok(result);
        }
        catch (DivideByZeroException)
        {
            return EvaluationResult.Fail("Division by zero.");
        }
        catch (Exception ex)
        {
            return EvaluationResult.Fail($"Invalid expression: {ex.Message}");
        }
    }

    // -------------------------------------------------------------------------
    // Step 1: Tokenize
    // Breaks the raw string into a list of number tokens and operator tokens.
    // Handles unary minus (e.g. -5 or -(2+3)).
    // -------------------------------------------------------------------------
    private static List<string> Tokenize(string expression)
    {
        List<string> tokens = new();
        string expr = expression.Replace(" ", "");
        int i = 0;

        while (i < expr.Length)
        {
            char c = expr[i];

            if (char.IsDigit(c) || c == '.')
            {
                int start = i;
                while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.'))
                    i++;
                tokens.Add(expr[start..i]);
            }
            else if (c == '-' && IsUnaryPosition(tokens))
            {
                // Unary minus before a digit: read as a negative number token
                if (i + 1 < expr.Length && char.IsDigit(expr[i + 1]))
                {
                    i++; // skip the minus sign
                    int start = i;
                    while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.'))
                        i++;
                    tokens.Add("-" + expr[start..i]);
                }
                else
                {
                    // Unary minus before a parenthesis e.g. -(2+3): insert implicit 0
                    tokens.Add("0");
                    tokens.Add("-");
                    i++;
                }
            }
            else if (Precedence.ContainsKey(c) || c == '(' || c == ')')
            {
                tokens.Add(c.ToString());
                i++;
            }
            else
            {
                throw new InvalidOperationException($"Unexpected character '{c}' at position {i}.");
            }
        }

        return tokens;
    }

    /// <summary>
    /// A minus is unary when it appears at the start of the expression
    /// or immediately after another operator or an opening parenthesis.
    /// </summary>
    private static bool IsUnaryPosition(List<string> tokens)
    {
        if (tokens.Count == 0) return true;
        string last = tokens[^1];
        return last is "+" or "-" or "*" or "/" or "(";
    }

    // -------------------------------------------------------------------------
    // Step 2: Shunting-Yard — convert infix tokens to postfix (RPN)
    // -------------------------------------------------------------------------
    private static Queue<string> ToPostfix(List<string> tokens)
    {
        Queue<string> output = new();
        Stack<string> operators = new();

        foreach (string token in tokens)
        {
            if (IsNumber(token))
            {
                output.Enqueue(token);
            }
            else if (token == "(")
            {
                operators.Push(token);
            }
            else if (token == ")")
            {
                while (operators.Count > 0 && operators.Peek() != "(")
                    output.Enqueue(operators.Pop());

                if (operators.Count == 0)
                    throw new InvalidOperationException("Mismatched parentheses.");

                operators.Pop(); // discard the "("
            }
            else if (IsOperator(token))
            {
                while (operators.Count > 0 &&
                       operators.Peek() != "(" &&
                       IsOperator(operators.Peek()) &&
                       Precedence[operators.Peek()[0]] >= Precedence[token[0]])
                {
                    output.Enqueue(operators.Pop());
                }
                operators.Push(token);
            }
            else
            {
                throw new InvalidOperationException($"Unknown token '{token}'.");
            }
        }

        while (operators.Count > 0)
        {
            if (operators.Peek() == "(")
                throw new InvalidOperationException("Mismatched parentheses.");
            output.Enqueue(operators.Pop());
        }

        return output;
    }

    // -------------------------------------------------------------------------
    // Step 3: Evaluate the postfix (RPN) queue
    // -------------------------------------------------------------------------
    private static decimal EvaluatePostfix(Queue<string> postfix)
    {
        Stack<decimal> stack = new();

        while (postfix.Count > 0)
        {
            string token = postfix.Dequeue();

            if (IsNumber(token))
            {
                stack.Push(decimal.Parse(token, CultureInfo.InvariantCulture));
            }
            else
            {
                if (stack.Count < 2)
                    throw new InvalidOperationException("Insufficient operands for operator.");

                decimal right = stack.Pop();
                decimal left = stack.Pop();

                decimal result = token switch
                {
                    "+" => left + right,
                    "-" => left - right,
                    "*" => left * right,
                    "/" => right == 0
                        ? throw new DivideByZeroException()
                        : left / right,
                    _ => throw new InvalidOperationException($"Unknown operator '{token}'.")
                };

                stack.Push(result);
            }
        }

        if (stack.Count != 1)
            throw new InvalidOperationException("Invalid expression structure.");

        return stack.Pop();
    }

    private static bool IsNumber(string token) =>
        decimal.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out _);

    private static bool IsOperator(string token) =>
        token.Length == 1 && Precedence.ContainsKey(token[0]);
}
