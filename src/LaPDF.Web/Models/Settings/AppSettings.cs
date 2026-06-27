namespace LaPDF.Web.Models.Settings;

public class AppSettings
{
    public PdfSettings? Pdf { get; set; }
    public FileLimits? Limits { get; set; }
}

public class PdfSettings
{
    public int MaxFileSizeMB { get; set; } = 50;
    public int MaxPagesToProcess { get; set; } = 100;
}

public class FileLimits
{
    public int MaxFilesToMerge { get; set; } = 10;
    public int MaxFilesPerRequest { get; set; } = 5;
}