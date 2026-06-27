namespace LaPDF.Web.Services.Interfaces;

public interface IPdfService
{
    Task<CompressResponse> CompressAsync(CompressRequest request);
    
    Task<byte[]> MergeAsync(MergeRequest request);
    
    Task<List<byte[]>> SplitAsync(SplitRequest request);
    
    Task<byte[]> ConvertAsync(ConvertRequest request);
    
    bool IsValidPdf(byte[] fileBytes);
    
    string GetFileSizeFormatted(long bytes);
}