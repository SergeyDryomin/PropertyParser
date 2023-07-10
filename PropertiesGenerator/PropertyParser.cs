using System.Text;
using System.Text.RegularExpressions;

namespace PropertiesGenerator;

public static class PropertyParser
{
    public static string ParseProperties(string input)
    {
        char[] delimiters = { '\r', '\n' };
        string[] strings = input.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        var output = new StringBuilder();
        foreach (var str in strings)
        {
            try
            {
                output.Append( ParseProperty(str));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error with {str} : {e.Message}");
            }
        }

        return output.ToString().Trim();
    }

    public static string ParseProperty(string input)
    {
        var variable = GetDataFromInput(input, out string? type);

        var pascalCaseComment = GetComment(variable);
        string propertyType = GetType(type, pascalCaseComment);
        string pascalCaseVariable = GetVariable(pascalCaseComment);

        return $@"/// <summary>
/// {pascalCaseComment}.
/// </summary>
public {propertyType}? {pascalCaseVariable} {{ get; set; }}

";
    }
    static string GetDataFromInput(string input, out string? type)
    {
        int closingBracketIndex = input.IndexOf(']');

        if (closingBracketIndex != -1)
        {
            int separatorIndex = input.IndexOfAny(new[] { ' ', '-' }, closingBracketIndex + 1);
            if (separatorIndex == -1)
                separatorIndex = input.Length;

            string variable = input.Substring(closingBracketIndex + 1, separatorIndex - closingBracketIndex - 1).Trim();
            type = null;

            int openingParenIndex = input.LastIndexOf('(');
            int closingParenIndex = input.LastIndexOf(')');

            if (openingParenIndex != -1 && closingParenIndex != -1)
            {
                type = input.Substring(openingParenIndex + 1, closingParenIndex - openingParenIndex - 1).Trim();
            }

            return variable;
        }

        throw new Exception("Invalid input format");
    }

    static string GetComment(string input)
    {
        var result = new StringBuilder();
        var word = new StringBuilder();
        bool capitalizedPrev = true;


        input = Regex.Replace(input, @"\[.*?\]", "");

        foreach (char currentChar in input)
        {
            if (currentChar is '-' or '_')
            {
                continue;
            }

            if (!capitalizedPrev && char.IsUpper(currentChar) ||
                (char.IsLower(currentChar) && !word.ToString().Skip(1).Any(char.IsLower) && word.Length > 1))
            {
                var parsedWord = GetParsedWord(word.ToString(), out bool areAllCapital);
                if (areAllCapital)
                {
                    result.Append(parsedWord[..^1]);
                    word = new StringBuilder(parsedWord[^1] + char.ToLower(currentChar).ToString());
                }
                else
                {
                    result.Append(parsedWord);
                    word = new StringBuilder(char.ToLower(currentChar).ToString());
                }
                
                result.Append(' ');
                
            }
            else
            {
                word.Append(currentChar);
            }

            capitalizedPrev = char.IsUpper(currentChar);
        }

        result.Append(GetParsedWord(word.ToString(), out _));
        return result.ToString();
    }

    static string GetVariable(string pascalCaseComment)
    {
        var result = new StringBuilder();
        var words = pascalCaseComment.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            result.Append(word[0].ToString().ToUpper() + word[1..]);
        }

        return result.ToString();
    }

    static string GetParsedWord(string input, out bool areAllCapital)
    {
        Abbreviations.TryGetValue(input.ToLower(), out var result);
        result ??= input;
        if (result.Skip(1).Any(char.IsLower) || result.Length is 1)
        {
            areAllCapital = false; 
        }
        else
        {
            result = result.ToUpper();
            areAllCapital = true;
        }

        return  result;
    }

    static string GetType(string? comment, string variable)
    {
        if (comment != null)
        {
            return comment.ToLower() switch
            {
                "alpha" => "string",
                "text" => "string",
                "real" => "float",
                "integer" => "int",
                "boolean" => "bool",
                _ => "string"
            };
        }

        if(variable.Contains("Number"))
            return "int";
        if(variable.Contains("Width"))
            return "float";

        return "string";
    }

    static readonly Dictionary<string, string> Abbreviations = new()
    {
        { "cust", "customer" },
        { "num", "number" },
        { "prod", "product" },
        { "equip", "equipment" },
        { "est", "estimate" }
    };
}