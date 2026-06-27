namespace LaPDF.Web.Models.DTOs;

public class CompressResponse
{
    public bool Success { get; set; }
    
    public byte[]? CompressedFile { get; set; }
    
    public string? FileName { get; set; }
    
    public long OriginalSize { get; set; }
    
    public long CompressedSize { get; set; }
    
    public double ReductionPercentage { get; set; }
    
    public string? ErrorMessage { get; set; }
}