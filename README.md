# PropertyParser
Parse PUT &amp; POST definition data to C# properties

Brief instructions for a couple of minutes: https://drive.google.com/file/d/17cTv6wHcXW_FDNe6h3eIvlhsm0bK9zZY/view?usp=sharing
Text version of the instruction:

0) Run PropertiesGenerator.exe
1) Copy the definitions to the clipboard or file inputDefinitions.txt (in the same folder as PropertiesGenerator.exe)
2) Select options 1-4 for working with the buffer and parsing into specific data:
1 - DTO, 2 -Details, 3 - DetailsData 4 - Map (definitions data from the buffer will be written to the buffer as C# properties or mapper)
Or 5 - to read definitions from a file and save all of the above to cs-files in the outputProperties directory (it will be created if it does not exist)
PS If some abbreviations are missing to change the names, then they can be added to PropertyParser.cs:
<code>
private static readonly Dictionary<string, string> Abbreviations = new()
    {
        { "cust", "customer" },
        { "num", "number" },
        { "prod", "product" },
        { "equip", "equipment" },
        { "est", "estimate" },
        { "descr", "description" }
    };
</code>    
Or add a type to:
<code>
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
</code>  
