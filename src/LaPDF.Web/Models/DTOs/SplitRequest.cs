namespace LaPDF.Web.Models.DTOs;

public class SplitRequest
{
    public IFormFile? File { get; set; }
    
    public string? PageNumbersInput { get; set; } 
    
    public List<int>? PageNumbers 
    { 
        get
        {
            if (string.IsNullOrEmpty(PageNumbersInput))
                return null;
                
            return PageNumbersInput
                .Split(',')
                .Select(p => p.Trim())
                .Where(p => int.TryParse(p, out _))
                .Select(int.Parse)
                .ToList();
        }
    }
    
    public bool SplitAllPages { get; set; }
}