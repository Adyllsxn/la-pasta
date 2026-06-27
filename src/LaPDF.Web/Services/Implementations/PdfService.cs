namespace LaPDF.Web.Services.Implementations;

public class PdfService : IPdfService
{
    #region Fields & Constructor

    private readonly ILogger<PdfService> _logger;

    public PdfService(ILogger<PdfService> logger)
    {
        _logger = logger;
    }

    #endregion

    #region Public Methods

    public async Task<CompressResponse> CompressAsync(CompressRequest request)
    {
        try
        {
            // Validação de null
            if (request.File == null || request.File.Length == 0)
            {
                return new CompressResponse
                {
                    Success = false,
                    ErrorMessage = "Nenhum arquivo foi selecionado"
                };
            }

            var fileBytes = await GetFileBytesAsync(request.File);
            
            if (!IsValidPdf(fileBytes))
            {
                return new CompressResponse
                {
                    Success = false,
                    ErrorMessage = "Arquivo PDF inválido"
                };
            }

            var originalSize = fileBytes.Length;
            var compressedBytes = await CompressPdfAsync(fileBytes, request.Quality);

            return new CompressResponse
            {
                Success = true,
                CompressedFile = compressedBytes,
                FileName = request.File.FileName,
                OriginalSize = originalSize,
                CompressedSize = compressedBytes.Length,
                ReductionPercentage = CalculateReduction(originalSize, compressedBytes.Length)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao comprimir PDF");
            return new CompressResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<byte[]> MergeAsync(MergeRequest request)
    {
        try
        {
            // Validação de null
            if (request.Files == null || request.Files.Count < 2)
            {
                throw new Exception("Selecione pelo menos 2 arquivos PDF para mesclar");
            }

            var documents = new List<byte[]>();
            
            foreach (var file in request.Files)
            {
                if (file == null || file.Length == 0)
                    throw new Exception("Um dos arquivos está vazio ou inválido");

                var bytes = await GetFileBytesAsync(file);
                if (!IsValidPdf(bytes))
                    throw new Exception($"Arquivo {file.FileName} não é um PDF válido");
                documents.Add(bytes);
            }

            return await MergePdfsAsync(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao mesclar PDFs");
            throw;
        }
    }

    public async Task<List<byte[]>> SplitAsync(SplitRequest request)
    {
        try
        {
            // Validação de null
            if (request.File == null || request.File.Length == 0)
            {
                throw new Exception("Nenhum arquivo foi selecionado");
            }

            var fileBytes = await GetFileBytesAsync(request.File);
            
            if (!IsValidPdf(fileBytes))
                throw new Exception("Arquivo PDF inválido");

            // Validar se tem páginas para dividir
            if (!request.SplitAllPages && (request.PageNumbers == null || !request.PageNumbers.Any()))
            {
                throw new Exception("Selecione pelo menos uma página para dividir ou marque 'Dividir todas'");
            }

            return await SplitPdfAsync(fileBytes, request.PageNumbers, request.SplitAllPages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao dividir PDF");
            throw;
        }
    }

    public async Task<byte[]> ConvertAsync(ConvertRequest request)
    {
        throw new NotImplementedException("Funcionalidade em desenvolvimento");
    }

    public bool IsValidPdf(byte[] fileBytes)
    {
        try
        {
            using var stream = new MemoryStream(fileBytes);
            using var reader = new PdfReader(stream);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GetFileSizeFormatted(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    #endregion

    #region Private Helper Methods

    private async Task<byte[]> GetFileBytesAsync(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    private async Task<byte[]> CompressPdfAsync(byte[] fileBytes, CompressionQuality quality)
    {
        return await Task.Run(() =>
        {
            using var inputStream = new MemoryStream(fileBytes);
            using var outputStream = new MemoryStream();
            
            var reader = new PdfReader(inputStream);
            var document = new Document();
            var writer = PdfWriter.GetInstance(document, outputStream);
            
            // Configurar compressão
            writer.CompressionLevel = quality switch
            {
                CompressionQuality.Low => 0,
                CompressionQuality.Medium => 1,
                CompressionQuality.High => 9,
                _ => 1
            };

            document.Open();
            
            var cb = writer.DirectContent;
            
            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                document.NewPage();
                var page = writer.GetImportedPage(reader, i);
                cb.AddTemplate(page, 0, 0);
            }
            
            document.Close();
            reader.Close();
            
            return outputStream.ToArray();
        });
    }

    private async Task<byte[]> MergePdfsAsync(List<byte[]> documents)
    {
        return await Task.Run(() =>
        {
            using var outputStream = new MemoryStream();
            var document = new Document();
            var writer = PdfWriter.GetInstance(document, outputStream);
            
            document.Open();
            var cb = writer.DirectContent;
            
            foreach (var pdfBytes in documents)
            {
                using var inputStream = new MemoryStream(pdfBytes);
                var reader = new PdfReader(inputStream);
                
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    document.NewPage();
                    var page = writer.GetImportedPage(reader, i);
                    cb.AddTemplate(page, 0, 0);
                }
                
                reader.Close();
            }
            
            document.Close();
            return outputStream.ToArray();
        });
    }

    private async Task<List<byte[]>> SplitPdfAsync(byte[] fileBytes, List<int>? pageNumbers, bool splitAllPages)
    {
        return await Task.Run(() =>
        {
            var result = new List<byte[]>();
            
            using var inputStream = new MemoryStream(fileBytes);
            var reader = new PdfReader(inputStream);
            var totalPages = reader.NumberOfPages;

            if (splitAllPages)
            {
                for (int i = 1; i <= totalPages; i++)
                {
                    result.Add(ExtractPage(reader, i));
                }
            }
            else if (pageNumbers != null && pageNumbers.Any())
            {
                foreach (var pageNum in pageNumbers.Where(p => p >= 1 && p <= totalPages))
                {
                    result.Add(ExtractPage(reader, pageNum));
                }
            }

            reader.Close();
            return result;
        });
    }

    private byte[] ExtractPage(PdfReader reader, int pageNumber)
    {
        using var outputStream = new MemoryStream();
        var document = new Document();
        var writer = PdfWriter.GetInstance(document, outputStream);
        
        document.Open();
        var cb = writer.DirectContent;
        var page = writer.GetImportedPage(reader, pageNumber);
        
        document.SetPageSize(reader.GetPageSize(pageNumber));
        document.NewPage();
        cb.AddTemplate(page, 0, 0);
        
        document.Close();
        
        return outputStream.ToArray();
    }

    private double CalculateReduction(long original, long compressed)
    {
        if (original == 0) return 0;
        return Math.Round((1 - (double)compressed / original) * 100, 2);
    }

    #endregion
}