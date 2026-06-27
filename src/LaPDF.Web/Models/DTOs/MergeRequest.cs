namespace LaPDF.Web.Models.DTOs;

public class MergeRequest
{
    public List<IFormFile> Files { get; set; } = new();
    
    public string? MergedFileName { get; set; }
}