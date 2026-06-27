namespace LaPDF.UnitTests.Services;

public class PdfServiceTests
{
    private readonly PdfService _pdfService;
    private readonly Mock<ILogger<PdfService>> _loggerMock;

    public PdfServiceTests()
    {
        _loggerMock = new Mock<ILogger<PdfService>>();
        _pdfService = new PdfService(_loggerMock.Object);
    }

    [Fact]
    public void IsValidPdf_WithValidPdf_ShouldReturnTrue()
    {
        // Arrange
        var pdfBytes = CreateSamplePdf();

        // Act
        var result = _pdfService.IsValidPdf(pdfBytes);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidPdf_WithInvalidPdf_ShouldReturnFalse()
    {
        // Arrange
        var invalidBytes = new byte[] { 0x00, 0x01, 0x02 };

        // Act
        var result = _pdfService.IsValidPdf(invalidBytes);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetFileSizeFormatted_ShouldReturnCorrectFormat()
    {
        // Act
        var result = _pdfService.GetFileSizeFormatted(1024);

        // Assert
        Assert.Equal("1 KB", result);
    }

    [Fact]
    public async Task CompressAsync_WithNullFile_ShouldReturnError()
    {
        // Arrange
        var request = new CompressRequest
        {
            File = null,
            Quality = CompressionQuality.Medium
        };

        // Act
        var result = await _pdfService.CompressAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Nenhum arquivo foi selecionado", result.ErrorMessage);
    }

    private byte[] CreateSamplePdf()
    {
        using var stream = new MemoryStream();
        using var document = new iTextSharp.text.Document();
        var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, stream);
        
        document.Open();
        document.Add(new iTextSharp.text.Paragraph("Teste PDF"));
        document.Close();
        
        return stream.ToArray();
    }
}