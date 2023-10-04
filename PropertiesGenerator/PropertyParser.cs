using System.Text;
using System.Text.RegularExpressions;

namespace PropertiesGenerator;

public static class PropertyParser
{
	public static string ParseProperties(string input, int choice)
	{
		char[] delimiters = { '\r', '\n' };
		string[] strings = input.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
		var output = new StringBuilder();
		foreach (var str in strings)
		{
			try
			{
				output.Append(ParseProperty(str, choice));
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error with {str} : {e.Message}");
			}
		}

		var result = output.ToString().Trim();
		return choice == 4 ?
			result.Remove(result.Length - 1) :
			result;
	}

	public static string ParseProperty(string input, int choice)
	{
		var variable = GetDataFromInput(input, out string? type);
		return choice switch
		{
			1 => GetDtoProperty(variable, type),
			2 => GetDetailsProperty(variable, type),
			3 => GetDetailsDataProperty(variable, type),
			4 => GetMapDefinition(variable),
			_ => throw new Exception("Invalid choice")
		};
	}

	private static string GetDtoProperty(string variable, string? type)
	{
		var (pascalCaseComment, propertyType, pascalCaseVariable) =
			GetParsedFromVariableAndType(variable, type);

		return $@"/// <summary>
/// {pascalCaseComment}.
/// </summary>
public {propertyType} {pascalCaseVariable} {{ get; set; }}{GetDefaultString(propertyType)}

";
	}

	private static string GetDetailsProperty(string variable, string? type)
	{
		var (_, propertyType, pascalCaseVariable) =
			GetParsedFromVariableAndType(variable, type);

		return $@"public {propertyType} {pascalCaseVariable} {{ get; set; }}{GetDefaultString(propertyType)}

";
	}

	private static string GetDetailsDataProperty(string variable, string? type)
	{
		var (_, propertyType, pascalCaseVariable) =
			GetParsedFromVariableAndType(variable, type);
		return $@"[JsonProperty(""{variable}"")]
public {propertyType} {pascalCaseVariable} {{ get; set; }}{GetDefaultString(propertyType)}

";
	}

	private static string GetMapDefinition(string variable)
	{
		var (_, _, pascalCaseVariable) =
		   GetParsedFromVariableAndType(variable);

		return $@"{pascalCaseVariable} = source.{pascalCaseVariable},
";
	}

	private static string GetDefaultString(string? propertyType)
	{
		return propertyType == "string" ? " = string.Empty;" : string.Empty;
	}

	private static (string, string, string) GetParsedFromVariableAndType(string variable, string? type = null)
	{
		var pascalCaseComment = GetComment(variable);
		string propertyType = GetType(type, pascalCaseComment);
		string pascalCaseVariable = GetVariable(pascalCaseComment);

		return (pascalCaseComment, propertyType, pascalCaseVariable);
	}

	private static string GetDataFromInput(string input, out string? type)
	{
		int closingBracketIndex = input.IndexOf(']');
		string variable;
		try
		{
			if (closingBracketIndex != -1)
			{
				int separatorIndex = input.IndexOfAny(new[] { ' ', '-' }, closingBracketIndex + 1);
				if (separatorIndex == -1)
					separatorIndex = input.Length;

				variable = input.Substring(closingBracketIndex + 1, separatorIndex - closingBracketIndex - 1).Trim();
				type = null;

				int openingParenIndex = input.LastIndexOf('(');
				int closingParenIndex = input.LastIndexOf(')');

				if (openingParenIndex != -1 && closingParenIndex != -1)
				{
					type = input.Substring(openingParenIndex + 1, closingParenIndex - openingParenIndex - 1).Trim();
				}
			}
			else
			{
				char[] whitespaceChars = { ' ', '\t', '\n', '\r', '\f', '\v' };

				string[] words = input.Split(whitespaceChars, StringSplitOptions.RemoveEmptyEntries);
				type = words[1];
				variable = GetParsedWord(words[0], out _);
			}
		}
		catch
		{
			throw new Exception($"Invalid input format {input}");
		}

		return variable;
	}

	private static string GetComment(string input)
	{
		var result = new StringBuilder();
		var word = new StringBuilder();
		bool capitalizedPrev = true;
		bool makeNextUppercase = false;

		input = Regex.Replace(input, @"\[.*?\]", "");

		foreach (char currentChar in input)
		{
			if (currentChar is '-' or '_')
			{
				makeNextUppercase = true;
				continue;
			}

			var tmpCurrentChar = currentChar;
			if (makeNextUppercase)
			{
				makeNextUppercase = false;
				tmpCurrentChar = char.ToUpper(tmpCurrentChar);
			}

			if (!capitalizedPrev && char.IsUpper(tmpCurrentChar) ||
				(char.IsLower(tmpCurrentChar) && !word.ToString().Skip(1).Any(char.IsLower) && word.Length > 1))
			{
				var parsedWord = GetParsedWord(word.ToString(), out bool areAllCapital);
				if (areAllCapital)
				{
					result.Append(parsedWord[..^1]);
					word = new StringBuilder(parsedWord[^1] + char.ToLower(tmpCurrentChar).ToString());
				}
				else
				{
					result.Append(parsedWord);
					word = new StringBuilder(char.ToLower(tmpCurrentChar).ToString());
				}

				result.Append(' ');

			}
			else
			{
				word.Append(tmpCurrentChar);
			}

			capitalizedPrev = char.IsUpper(tmpCurrentChar);
		}

		result.Append(GetParsedWord(word.ToString(), out _));
		return result.ToString();
	}

	private static string GetVariable(string pascalCaseComment)
	{
		var result = new StringBuilder();
		var words = pascalCaseComment.Split(' ', StringSplitOptions.RemoveEmptyEntries);

		foreach (var word in words)
		{
			result.Append(word[0].ToString().ToUpper() + word[1..]);
		}

		return result.ToString();
	}

	private static string GetParsedWord(string input, out bool areAllCapital)
	{
		string? result;
		var endingNumbers = input[^input.Reverse().TakeWhile(char.IsDigit).Count()..];
		if (string.IsNullOrWhiteSpace(endingNumbers))
		{
			Abbreviations.TryGetValue(input.ToLower(), out result);
			result ??= input;
		}
		else
		{
			Abbreviations.TryGetValue(input[..^endingNumbers.Length].ToLower(), out result);
			result = string.IsNullOrEmpty(result) ? input : result + endingNumbers;
		}


		if (result.Skip(1).Any(char.IsLower) || result.Length is 1)
		{
			areAllCapital = false;
		}
		else
		{
			result = result.ToUpper();
			areAllCapital = true;
		}

		return result;
	}

	private static string GetType(string? comment, string variable)
	{
		if (comment != null)
		{
			return comment.ToLower() switch
			{
				"alpha" => "string",
				"text" => "string",
				"real" => "float",
				"integer" => "int",
				"long" => "long",
				"boolean" => "bool",
				"10" => "string", //alpha
				"8" => "DateOnly", //date
				"4" => "long", //long integer
				"6" => "float", //real
				"9" => "long", //time
				"1" => "bool", //boolean
				_ => "string"
			};
		}

		if (variable.Contains("Number"))
			return "int";
		if (variable.Contains("Width"))
			return "float";

		return "string";
	}

	private static readonly Dictionary<string, string> Abbreviations = new()
	{
		{ "cust", "Customer" },
		{ "num", "Number" },
		{ "prod", "Product" },
		{ "equip", "Equipment" },
		{ "est", "Estimate" },
		{ "descr", "Description" },
		{ "desc", "Description" }
	};
}