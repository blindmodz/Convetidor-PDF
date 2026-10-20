using iTextSharp.text.pdf;

internal static class PdfConverter
{
    public static char ParseVersion(string version)
    {
        return version switch
        {
            "1.2" => PdfWriter.VERSION_1_2,
            "1.3" => PdfWriter.VERSION_1_3,
            "1.4" => PdfWriter.VERSION_1_4,
            "1.5" => PdfWriter.VERSION_1_5,
            "1.6" => PdfWriter.VERSION_1_6,
            "1.7" => PdfWriter.VERSION_1_7,
            _ => throw new ArgumentException("La versión debe ser 1.2, 1.3, 1.4, 1.5, 1.6 o 1.7."),
        };
    }

    public static void Convert(string inputPath, string outputPath, char version)
    {
        var reader = new PdfReader(inputPath);

        try
        {
            using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var stamper = new PdfStamper(reader, output, version);
        }
        finally
        {
            reader.Close();
        }
    }
}
