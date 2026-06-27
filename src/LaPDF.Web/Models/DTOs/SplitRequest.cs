namespace LaPDF.Web.Models.DTOs;

public class SplitRequest
{
    public IFormFile? File { get; set; }
    
    public List<int>? PageNumbers { get; set; }
    
    public bool SplitAllPages { get; set; }
}