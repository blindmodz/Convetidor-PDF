internal static class CommandLineRunner
{
    public static int Run(string[] commandLineArgs)
    {
        if (commandLineArgs.Length < 1 || commandLineArgs.Length > 3)
        {
            Console.WriteLine("Uso:");
            Console.WriteLine("  Convertidor PDF.exe <entrada.pdf> [salida.pdf] [version]");
            Console.WriteLine("  version: 1.2, 1.3, 1.4, 1.5, 1.6 o 1.7");
            return 1;
        }

        var inputPath = Path.GetFullPath(commandLineArgs[0]);
        var outputPath =
            commandLineArgs.Length >= 2
                ? Path.GetFullPath(commandLineArgs[1])
                : Path.Combine(
                    Path.GetDirectoryName(inputPath) ?? Environment.CurrentDirectory,
                    $"{Path.GetFileNameWithoutExtension(inputPath)}_compatible.pdf"
                );
        var versionText = commandLineArgs.Length == 3 ? commandLineArgs[2] : "1.4";

        if (!File.Exists(inputPath))
        {
            Console.Error.WriteLine($"No existe el archivo de entrada: {inputPath}");
            return 2;
        }

        if (string.Equals(inputPath, outputPath, StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("La salida debe ser un archivo distinto de la entrada.");
            return 3;
        }

        try
        {
            PdfConverter.Convert(inputPath, outputPath, PdfConverter.ParseVersion(versionText));
            Console.WriteLine($"PDF convertido correctamente: {outputPath}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"No se pudo convertir el PDF: {exception.Message}");
            return 4;
        }
    }
}
