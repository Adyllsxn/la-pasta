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
            if (request.File == null || request.File.Length == 0)
            {
                return new CompressResponse
                {
                    Success = false,
                    ErrorMessage = "Nenhum arquivo foi selecionado"
                };
            }

            var fileBytes = await GetFileBytesAsync(request.File);
            
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
                ErrorMessage = $"Erro ao comprimir: {ex.Message}"
            };
        }
    }

    public async Task<byte[]> MergeAsync(MergeRequest request)
    {
        try
        {
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
                documents.Add(bytes);
            }

            return await MergePdfsWithGhostScriptAsync(documents);
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
            if (request.File == null || request.File.Length == 0)
            {
                throw new Exception("Nenhum arquivo foi selecionado");
            }

            var fileBytes = await GetFileBytesAsync(request.File);

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
        try
        {
            if (request.File == null || request.File.Length == 0)
                throw new Exception("Nenhum arquivo foi selecionado");

            var fileBytes = await GetFileBytesAsync(request.File);

            var result = request.TargetFormat switch
            {
                ConvertFormat.JPG => await ConvertPdfToJpgAsync(fileBytes),
                ConvertFormat.PNG => await ConvertPdfToPngAsync(fileBytes),
                _ => throw new NotImplementedException($"Conversão para {request.TargetFormat} em desenvolvimento")
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao converter PDF");
            throw;
        }
    }

    public bool IsValidPdf(byte[] fileBytes)
    {
        try
        {
            return fileBytes.Length > 0 && 
                   fileBytes[0] == 0x25 && 
                   fileBytes[1] == 0x50 && 
                   fileBytes[2] == 0x44 && 
                   fileBytes[3] == 0x46;
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
        var tempInput = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");
        var tempOutput = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");
        
        try
        {
            await System.IO.File.WriteAllBytesAsync(tempInput, fileBytes);
            
            var qualitySettings = quality switch
            {
                CompressionQuality.Low => "/screen",
                CompressionQuality.Medium => "/ebook",
                CompressionQuality.High => "/printer",
                _ => "/ebook"
            };
            
            var args = $"-dNOPAUSE -dBATCH -sDEVICE=pdfwrite -dCompatibilityLevel=1.4 -dPDFSETTINGS={qualitySettings} -sOutputFile={tempOutput} {tempInput}";
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/usr/bin/gs",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            
            if (process.ExitCode != 0)
            {
                _logger.LogError($"GhostScript erro: {error}");
                throw new Exception($"GhostScript erro: {error}");
            }
            
            if (!System.IO.File.Exists(tempOutput))
            {
                throw new Exception("Arquivo de saída não foi criado");
            }
            
            var compressedBytes = await System.IO.File.ReadAllBytesAsync(tempOutput);
            
            if (compressedBytes.Length == 0)
            {
                throw new Exception("Arquivo comprimido está vazio");
            }
            
            return compressedBytes;
        }
        finally
        {
            if (System.IO.File.Exists(tempInput)) System.IO.File.Delete(tempInput);
            if (System.IO.File.Exists(tempOutput)) System.IO.File.Delete(tempOutput);
        }
    }

    private async Task<byte[]> MergePdfsWithGhostScriptAsync(List<byte[]> documents)
    {
        var tempInputs = new List<string>();
        var tempOutput = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");
        
        try
        {
            for (int i = 0; i < documents.Count; i++)
            {
                var tempInput = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");
                await System.IO.File.WriteAllBytesAsync(tempInput, documents[i]);
                tempInputs.Add(tempInput);
            }
            
            var inputFiles = string.Join(" ", tempInputs.Select(f => $"\"{f}\""));
            var args = $"-dNOPAUSE -dBATCH -sDEVICE=pdfwrite -dCompatibilityLevel=1.4 -sOutputFile={tempOutput} {inputFiles}";
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/usr/bin/gs",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            await process.WaitForExitAsync();
            
            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync();
                throw new Exception($"GhostScript erro ao mesclar: {error}");
            }
            
            return await System.IO.File.ReadAllBytesAsync(tempOutput);
        }
        finally
        {
            foreach (var tempInput in tempInputs)
            {
                if (System.IO.File.Exists(tempInput)) System.IO.File.Delete(tempInput);
            }
            if (System.IO.File.Exists(tempOutput)) System.IO.File.Delete(tempOutput);
        }
    }

    private async Task<List<byte[]>> SplitPdfAsync(byte[] fileBytes, List<int>? pageNumbers, bool splitAllPages)
    {
        var result = new List<byte[]>();
        var tempInput = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");
        
        try
        {
            await System.IO.File.WriteAllBytesAsync(tempInput, fileBytes);
            
            var totalPages = await GetTotalPagesAsync(tempInput);
            
            var pagesToExtract = splitAllPages 
                ? Enumerable.Range(1, totalPages).ToList() 
                : pageNumbers ?? new List<int>();
            
            foreach (var pageNum in pagesToExtract.Where(p => p >= 1 && p <= totalPages))
            {
                var tempOutput = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");
                var args = $"-dNOPAUSE -dBATCH -sDEVICE=pdfwrite -dFirstPage={pageNum} -dLastPage={pageNum} -sOutputFile={tempOutput} {tempInput}";
                
                try
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "/usr/bin/gs",
                            Arguments = args,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    
                    process.Start();
                    await process.WaitForExitAsync();
                    
                    if (process.ExitCode == 0 && System.IO.File.Exists(tempOutput))
                    {
                        var bytes = await System.IO.File.ReadAllBytesAsync(tempOutput);
                        if (bytes.Length > 0)
                            result.Add(bytes);
                    }
                }
                finally
                {
                    if (System.IO.File.Exists(tempOutput)) System.IO.File.Delete(tempOutput);
                }
            }
        }
        finally
        {
            if (System.IO.File.Exists(tempInput)) System.IO.File.Delete(tempInput);
        }
        
        return result;
    }

    private async Task<int> GetTotalPagesAsync(string pdfPath)
    {
        try
        {
            var args = $"-dNODISPLAY -dNOSAFER -dBATCH -c \"({pdfPath}) (r) file runpdfbegin pdfpagecount = quit\"";
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/usr/bin/gs",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            
            var match = Regex.Match(output, @"(\d+)");
            if (match.Success)
                return int.Parse(match.Groups[1].Value);
            
            return 1;
        }
        catch
        {
            return 1;
        }
    }

    private async Task<byte[]> ConvertPdfToJpgAsync(byte[] fileBytes)
    {
        return await Task.Run(() =>
        {
            var tempInput = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");
            var tempOutput = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".jpg");
            
            try
            {
                System.IO.File.WriteAllBytes(tempInput, fileBytes);
                
                var args = $"-dNOPAUSE -dBATCH -sDEVICE=jpeg -dFirstPage=1 -dLastPage=1 -dJPEGQ=90 -sOutputFile={tempOutput} {tempInput}";
                
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/usr/bin/gs",
                        Arguments = args,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                
                process.Start();
                process.WaitForExit();
                
                if (process.ExitCode != 0)
                    throw new Exception("Falha na conversão para JPG");
                
                return System.IO.File.ReadAllBytes(tempOutput);
            }
            finally
            {
                if (System.IO.File.Exists(tempInput)) System.IO.File.Delete(tempInput);
                if (System.IO.File.Exists(tempOutput)) System.IO.File.Delete(tempOutput);
            }
        });
    }

    private async Task<byte[]> ConvertPdfToPngAsync(byte[] fileBytes)
    {
        return await Task.Run(() =>
        {
            var tempInput = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");
            var tempOutput = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");
            
            try
            {
                System.IO.File.WriteAllBytes(tempInput, fileBytes);
                
                var args = $"-dNOPAUSE -dBATCH -sDEVICE=png16m -dFirstPage=1 -dLastPage=1 -sOutputFile={tempOutput} {tempInput}";
                
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/usr/bin/gs",
                        Arguments = args,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                
                process.Start();
                process.WaitForExit();
                
                if (process.ExitCode != 0)
                    throw new Exception("Falha na conversão para PNG");
                
                return System.IO.File.ReadAllBytes(tempOutput);
            }
            finally
            {
                if (System.IO.File.Exists(tempInput)) System.IO.File.Delete(tempInput);
                if (System.IO.File.Exists(tempOutput)) System.IO.File.Delete(tempOutput);
            }
        });
    }

    private double CalculateReduction(long original, long compressed)
    {
        if (original == 0) return 0;
        return Math.Round((1 - (double)compressed / original) * 100, 2);
    }

    #endregion
}