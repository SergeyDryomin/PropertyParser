namespace PropertiesGenerator;

class Program
{
    static void Main()
    {
        Clipboard.WriteToClipboard(
            PropertyParser.ParseProperties(
                Clipboard.ReadFromClipboard()));
    }
}