using System.Runtime.CompilerServices;

namespace Extensions.Configuration.Database;

/// <summary>
/// attempt to validate a simple "identifier = value" SQL clause for safety and correctness
/// </summary>
public static class SimpleSqlClauseValidator
{
    /// <summary>
    /// Validates that a SQL snippet is of the simple form "identifier (=|&lt;|&gt;|!=) value".
    /// <c>identifier</c> can be delimited by [square brackets] or "quotes".
    /// </summary>
    /// <param name="input"></param>
    /// <returns>
    /// true if the given input can sanely be used as a simple "identifier = value" SQL where clause
    /// false if there is a problem.
    /// </returns>
    public static BoolBecause IsASimpleIdentifierOperatorValueExpression(ReadOnlySpan<char> input)
    {
        int i = 0;

        SkipWhitespace(input, ref i);

        if (!TryParseIdentifier(input, ref i))
            return new(false,"Invalid identifier");

        SkipWhitespace(input, ref i);

        if (i >= input.Length 
            || (input[i] is not '=' and not '<' and not '>' and not '!')
            || (input[i] is '!' && ( i+1 == input.Length || (input[i+1] is not '=') ))
            )
            return new(false,"No comparison operator");
        
        if (input[i] is '!' && input[i+1] is '=') i++;
        i++;

        SkipWhitespace(input, ref i);

        if (!TryParseValue(input, ref i))
            return new(false,"Invalid value");

        SkipWhitespace(input, ref i);

        return i == input.Length 
            ? new(true) 
            : new(false, "unexpected characters after value");
    }

    // ---------------- Identifier ----------------

    static bool TryParseIdentifier(ReadOnlySpan<char> s, ref int i)
    {
        if (i >= s.Length)
            return false;

        if (s[i] == '[')
            return ParseBracketedIdentifier(s, ref i);

        if (s[i] == '"')
            return ParseQuotedIdentifier(s, ref i);

        // Unquoted identifier
        if (!IsIdentStart(s[i]))
            return false;

        i++;
        while (i < s.Length && IsIdentPart(s[i]))
            i++;

        return true;
    }

    static bool ParseBracketedIdentifier(ReadOnlySpan<char> s, ref int i)
    {
        i++; // skip '['

        while (i < s.Length)
        {
            if (s[i] == ']')
            {
                // Escaped ]]
                if (i + 1 < s.Length && s[i + 1] == ']')
                {
                    i += 2;
                    continue;
                }

                i++; // closing ]
                return true;
            }
            i++;
        }

        return false;
    }

    static bool ParseQuotedIdentifier(ReadOnlySpan<char> s, ref int i)
    {
        i++; // skip "

        while (i < s.Length)
        {
            if (s[i] == '"')
            {
                // Escaped ""
                if (i + 1 < s.Length && s[i + 1] == '"')
                {
                    i += 2;
                    continue;
                }

                i++; // closing "
                return true;
            }
            i++;
        }

        return false;
    }

    // ---------------- Value ----------------

    static bool TryParseValue(ReadOnlySpan<char> s, ref int i)
    {
        if (i >= s.Length)
            return false;

        // String literal
        if (s[i] == '\'')
            return ParseStringLiteral(s, ref i);

        // Numeric literal
        return ParseNumber(s, ref i);
    }

    static bool ParseStringLiteral(ReadOnlySpan<char> s, ref int i)
    {
        i++; // skip '

        while (i < s.Length)
        {
            if (s[i] == '\'')
            {
                // Escaped ''
                if (i + 1 < s.Length && s[i + 1] == '\'')
                {
                    i += 2;
                    continue;
                }

                i++; // closing '
                return true;
            }
            i++;
        }

        return false;
    }

    static bool ParseNumber(ReadOnlySpan<char> s, ref int i)
    {
        int start = i;

        if (s[i] == '+' || s[i] == '-')
            i++;

        bool hasDigits = false;

        while (i < s.Length && char.IsDigit(s[i]))
        {
            hasDigits = true;
            i++;
        }

        if (i < s.Length && s[i] == '.')
        {
            i++;
            while (i < s.Length && char.IsDigit(s[i]))
            {
                hasDigits = true;
                i++;
            }
        }

        return hasDigits && i > start;
    }

    // ---------------- Helpers ----------------

    static void SkipWhitespace(ReadOnlySpan<char> s, ref int i)
    {
        while (i < s.Length && char.IsWhiteSpace(s[i]))
            i++;
    }

    static bool IsIdentStart(char c)
        => char.IsLetter(c) || c == '_';

    static bool IsIdentPart(char c)
        => char.IsLetterOrDigit(c) || c == '_';
}

public readonly record struct BoolBecause(bool Value, string Reason = "")
{
    public static implicit operator bool(BoolBecause bws) => bws.Value;
    public static implicit operator BoolBecause(bool boolValue) => new(boolValue);

    public override string ToString() => string.IsNullOrEmpty(Reason)
                                        ? Value.ToString()
                                        : $"{Value} because {Reason}";

    public static BoolBecause operator &(BoolBecause left, BoolBecause right)
        => new(left.Value && right.Value, Combine(left.Reason, "and", right.Reason));

    // OR (used by ||)
    public static BoolBecause operator |(BoolBecause left, BoolBecause right)
        => new(left.Value || right.Value, Combine(left.Reason, "or", right.Reason)
        );

    public static bool operator true(BoolBecause value) => value.Value;

    public static bool operator false(BoolBecause value) => !value.Value;

    static string Combine(string left, string op, string right)
    {
        if (string.IsNullOrEmpty(left)) return right;
        if (string.IsNullOrEmpty(right)) return left;
        return $"{left} {op} {right}";
    }
    
    /// <summary>
    /// Return a new BoolBecause. If <paramref name="Value"/> is true, then throw away the Reason.
    /// </summary>
    /// <param name="Value"></param>
    /// <param name="Reason"></param>
    /// <returns></returns>
    public static BoolBecause Requires(bool Value, [CallerArgumentExpression("Value")]string Reason = "")
        => new(Value, Value ? "" : Reason);
}