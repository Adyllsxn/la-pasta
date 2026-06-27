namespace LaPDF.Web.Models.DTOs;

public class CompressRequest
{
    public IFormFile? File { get; set; }
    
    public CompressionQuality Quality { get; set; } = CompressionQuality.Medium;
}