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
    public void IsValidPdf_WithEmptyBytes_ShouldReturnFalse()
    {
        // Arrange
        var emptyBytes = Array.Empty<byte>();

        // Act
        var result = _pdfService.IsValidPdf(emptyBytes);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetFileSizeFormatted_ShouldReturnCorrectFormat()
    {
        // Arrange
        var testCases = new[]
        {
            new { Bytes = 1024L, Expected = "1 KB" },
            new { Bytes = 1048576L, Expected = "1 MB" },
            new { Bytes = 1073741824L, Expected = "1 GB" },
            new { Bytes = 500L, Expected = "500 B" },
            new { Bytes = 1536L, Expected = "1.5 KB" }
        };

        foreach (var test in testCases)
        {
            // Act
            var result = _pdfService.GetFileSizeFormatted(test.Bytes);

            // Assert - usar Contains para ignorar diferença de cultura
            Assert.Contains(test.Expected.Replace(".", ","), result.Replace(".", ","));
        }
    }

    [Fact]
    public async Task CompressAsync_WithNullFile_ShouldReturnError()
    {
        // Arrange
        var request = new CompressRequest
        {
            File = null!,
            Quality = CompressionQuality.Medium
        };

        // Act
        var result = await _pdfService.CompressAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Nenhum arquivo foi selecionado", result.ErrorMessage);
    }

    [Fact]
    public async Task CompressAsync_WithEmptyFile_ShouldReturnError()
    {
        // Arrange
        var fileMock = CreateMockFormFile(Array.Empty<byte>(), "vazio.pdf");
        
        var request = new CompressRequest
        {
            File = fileMock,
            Quality = CompressionQuality.Medium
        };

        // Act
        var result = await _pdfService.CompressAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Nenhum arquivo foi selecionado", result.ErrorMessage);
    }

    [Fact(Skip = "GhostScript precisa estar instalado para este teste")]
    public async Task CompressAsync_WithValidPdf_ShouldCompressSuccessfully()
    {
        // Arrange
        var pdfBytes = CreateSamplePdf();
        var fileMock = CreateMockFormFile(pdfBytes, "test.pdf");
        
        var request = new CompressRequest
        {
            File = fileMock,
            Quality = CompressionQuality.Medium
        };

        // Act
        var result = await _pdfService.CompressAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.CompressedFile);
        Assert.Equal("test.pdf", result.FileName);
        Assert.Equal(pdfBytes.Length, result.OriginalSize);
        Assert.True(result.CompressedSize > 0);
        Assert.True(result.ReductionPercentage >= 0);
    }

    [Fact]
    public async Task MergeAsync_WithNullFiles_ShouldThrowException()
    {
        // Arrange
        var request = new MergeRequest
        {
            Files = null!
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _pdfService.MergeAsync(request));
        Assert.Contains("Selecione pelo menos 2 arquivos", exception.Message);
    }

    [Fact]
    public async Task MergeAsync_WithLessThan2Files_ShouldThrowException()
    {
        // Arrange
        var request = new MergeRequest
        {
            Files = new List<IFormFile>
            {
                CreateMockFormFile(new byte[] { 0x25, 0x50, 0x44, 0x46 }, "test1.pdf")
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _pdfService.MergeAsync(request));
        Assert.Contains("Selecione pelo menos 2 arquivos", exception.Message);
    }

    [Fact]
    public async Task SplitAsync_WithNullFile_ShouldThrowException()
    {
        // Arrange
        var request = new SplitRequest
        {
            File = null!,
            SplitAllPages = true
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _pdfService.SplitAsync(request));
        Assert.Contains("Nenhum arquivo foi selecionado", exception.Message);
    }

    [Fact]
    public async Task SplitAsync_WithNoPageSelection_ShouldThrowException()
    {
        // Arrange
        var pdfBytes = CreateSamplePdf();
        var fileMock = CreateMockFormFile(pdfBytes, "test.pdf");
        
        var request = new SplitRequest
        {
            File = fileMock,
            SplitAllPages = false,
            PageNumbersInput = ""  // String vazia para não ter páginas selecionadas
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _pdfService.SplitAsync(request));
        Assert.Contains("Selecione pelo menos uma página", exception.Message);
    }

    [Fact]
    public async Task ConvertAsync_WithNullFile_ShouldThrowException()
    {
        // Arrange
        var request = new ConvertRequest
        {
            File = null!,
            TargetFormat = ConvertFormat.JPG
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _pdfService.ConvertAsync(request));
        Assert.Contains("Nenhum arquivo foi selecionado", exception.Message);
    }

    private byte[] CreateSamplePdf()
    {
        // PDF mínimo válido (%PDF-1.4)
        var pdfContent = new byte[]
        {
            0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34, 0x0A, 0x25, 0xE2, 0xE3, 0xCF, 0xD3, 0x0A, 0x31,
            0x20, 0x30, 0x20, 0x6F, 0x62, 0x6A, 0x0A, 0x3C, 0x3C, 0x0A, 0x2F, 0x54, 0x79, 0x70, 0x65, 0x20,
            0x2F, 0x43, 0x61, 0x74, 0x61, 0x6C, 0x6F, 0x67, 0x0A, 0x2F, 0x50, 0x61, 0x67, 0x65, 0x73, 0x20,
            0x32, 0x20, 0x30, 0x20, 0x52, 0x0A, 0x3E, 0x3E, 0x0A, 0x65, 0x6E, 0x64, 0x6F, 0x62, 0x6A, 0x0A,
            0x32, 0x20, 0x30, 0x20, 0x6F, 0x62, 0x6A, 0x0A, 0x3C, 0x3C, 0x0A, 0x2F, 0x54, 0x79, 0x70, 0x65,
            0x20, 0x2F, 0x50, 0x61, 0x67, 0x65, 0x73, 0x0A, 0x2F, 0x4B, 0x69, 0x64, 0x73, 0x20, 0x5B, 0x33,
            0x20, 0x30, 0x20, 0x52, 0x5D, 0x0A, 0x2F, 0x43, 0x6F, 0x75, 0x6E, 0x74, 0x20, 0x31, 0x0A, 0x3E,
            0x3E, 0x0A, 0x65, 0x6E, 0x64, 0x6F, 0x62, 0x6A, 0x0A, 0x33, 0x20, 0x30, 0x20, 0x6F, 0x62, 0x6A,
            0x0A, 0x3C, 0x3C, 0x0A, 0x2F, 0x54, 0x79, 0x70, 0x65, 0x20, 0x2F, 0x50, 0x61, 0x67, 0x65, 0x0A,
            0x2F, 0x50, 0x61, 0x72, 0x65, 0x6E, 0x74, 0x20, 0x32, 0x20, 0x30, 0x20, 0x52, 0x0A, 0x2F, 0x43,
            0x6F, 0x6E, 0x74, 0x65, 0x6E, 0x74, 0x73, 0x20, 0x34, 0x20, 0x30, 0x20, 0x52, 0x0A, 0x3E, 0x3E,
            0x0A, 0x65, 0x6E, 0x64, 0x6F, 0x62, 0x6A, 0x0A, 0x34, 0x20, 0x30, 0x20, 0x6F, 0x62, 0x6A, 0x0A,
            0x3C, 0x3C, 0x0A, 0x2F, 0x54, 0x79, 0x70, 0x65, 0x20, 0x2F, 0x50, 0x61, 0x67, 0x65, 0x73, 0x0A,
            0x2F, 0x54, 0x65, 0x78, 0x74, 0x20, 0x5B, 0x28, 0x54, 0x65, 0x73, 0x74, 0x65, 0x20, 0x50, 0x44,
            0x46, 0x29, 0x5D, 0x0A, 0x3E, 0x3E, 0x0A, 0x65, 0x6E, 0x64, 0x6F, 0x62, 0x6A, 0x0A, 0x74, 0x72,
            0x61, 0x69, 0x6C, 0x65, 0x72, 0x0A, 0x3C, 0x3C, 0x0A, 0x2F, 0x52, 0x6F, 0x6F, 0x74, 0x20, 0x31,
            0x20, 0x30, 0x20, 0x52, 0x0A, 0x3E, 0x3E, 0x0A, 0x25, 0x25, 0x45, 0x4F, 0x46
        };
        
        return pdfContent;
    }

    private IFormFile CreateMockFormFile(byte[] content, string fileName)
    {
        var stream = new MemoryStream(content);
        var fileMock = new Mock<IFormFile>();
        
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(content.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Callback<Stream, CancellationToken>((s, _) =>
            {
                stream.Position = 0;
                stream.CopyTo(s);
            })
            .Returns(Task.CompletedTask);

        return fileMock.Object;
    }
}