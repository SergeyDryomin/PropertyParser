namespace PropertiesGenerator;

class Program
{
    private static readonly string inputFilePath = "inputDefinitions.txt";
    private static readonly string outputDirectoryPath = "outputProperties";

    static void Main()
    {
        //var tmp = Directory.GetCurrentDirectory();
        Console.WriteLine("1 - Definitions to DTO C# properties (from buffer into buffer)");
        Console.WriteLine("2 - Definitions to Details C# properties (from buffer into buffer)");
        Console.WriteLine("3 - Definitions to DetailsData C# properties (from buffer into buffer)");
        Console.WriteLine("4 - Map Definitions (from buffer into buffer)");
        Console.WriteLine($"5 - All above (from {inputFilePath} file to {outputDirectoryPath} folder)");
        int choice =  int.Parse(Console.ReadLine());
        if (choice < 5)
        {
            Clipboard.WriteToClipboard(
                PropertyParser.ParseProperties(
                    Clipboard.ReadFromClipboard(), choice));
        }
        else
        {
            if(!File.Exists(inputFilePath))
                throw new FileNotFoundException($"File {inputFilePath} not found");
            if(!Directory.Exists(outputDirectoryPath))
                Directory.CreateDirectory(outputDirectoryPath);
            var dataFromFile = string.Join("/r/n", File.ReadAllText(inputFilePath));
            for (int i = 1; i < 5; i++)
            {
                string parsedData = PropertyParser.ParseProperties(dataFromFile, i);
                switch (i)
                {
                    case 1:                       
                        File.WriteAllText(Path.Combine(outputDirectoryPath, "DtoProperties.cs"), parsedData);
                        break;
                    case 2:
                        File.WriteAllText(Path.Combine(outputDirectoryPath, "DetailsProperties.cs"), parsedData);
                        break;
                    case 3:
                        File.WriteAllText(Path.Combine(outputDirectoryPath, "DetailsDataProperties.cs"), parsedData);
                        break;
                    case 4:
                        File.WriteAllText(Path.Combine(outputDirectoryPath, "Map.cs"), parsedData);
                        break;
                }
            }
        }
    }
}