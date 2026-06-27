namespace LaPDF.Web.Models.DTOs;

public class ConvertRequest
{
    public IFormFile? File { get; set; }
    
    public ConvertFormat TargetFormat { get; set; }
}