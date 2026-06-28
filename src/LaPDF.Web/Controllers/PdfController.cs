namespace LaPDF.Web.Controllers;

public class PdfController : Controller
{
    private readonly IPdfService _pdfService;
    private readonly ILogger<PdfController> _logger;

    public PdfController(IPdfService pdfService, ILogger<PdfController> logger)
    {
        _pdfService = pdfService;
        _logger = logger;
    }

    #region Compress

    [HttpGet]
    public IActionResult Compress()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Compress(CompressRequest request)
    {
        try
        {
            if (request.File == null || request.File.Length == 0)
            {
                TempData["Error"] = "Por favor, selecione um arquivo PDF.";
                return RedirectToAction("Compress");
            }

            if (!PdfHelper.IsPdfFile(request.File))
            {
                TempData["Error"] = "O arquivo deve ser um PDF válido.";
                return RedirectToAction("Compress");
            }

            var result = await _pdfService.CompressAsync(request);

            if (result.Success && result.CompressedFile != null)
            {
                TempData["Success"] = true;
                TempData["OriginalSize"] = _pdfService.GetFileSizeFormatted(result.OriginalSize);
                TempData["CompressedSize"] = _pdfService.GetFileSizeFormatted(result.CompressedSize);
                TempData["Reduction"] = result.ReductionPercentage.ToString("F2") + "%";

                return File(
                    result.CompressedFile, 
                    "application/pdf", 
                    $"comprimido_{result.FileName}"
                );
            }

            TempData["Error"] = result.ErrorMessage ?? "Erro ao comprimir o PDF.";
            return RedirectToAction("Compress");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao comprimir PDF");
            TempData["Error"] = $"Erro ao processar: {ex.Message}";
            return RedirectToAction("Compress");
        }
    }

    #endregion

    #region Merge

    [HttpGet]
    public IActionResult Merge()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Merge(MergeRequest request)
    {
        try
        {
            if (request.Files == null || request.Files.Count < 2)
            {
                TempData["Error"] = "Selecione pelo menos 2 arquivos PDF para mesclar.";
                return RedirectToAction("Merge");
            }

            foreach (var file in request.Files)
            {
                if (!PdfHelper.IsPdfFile(file))
                {
                    TempData["Error"] = $"O arquivo {file.FileName} não é um PDF válido.";
                    return RedirectToAction("Merge");
                }
            }

            var result = await _pdfService.MergeAsync(request);
            
            var fileName = string.IsNullOrEmpty(request.MergedFileName) 
                ? "mesclado.pdf" 
                : $"{request.MergedFileName}.pdf";

            TempData["Success"] = $"✅ {request.Files.Count} PDFs mesclados com sucesso!";
            return File(result, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao mesclar PDFs");
            TempData["Error"] = $"Erro ao mesclar: {ex.Message}";
            return RedirectToAction("Merge");
        }
    }

    #endregion

    #region Split

    [HttpGet]
    public IActionResult Split()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Split(SplitRequest request)
    {
        try
        {
            if (request.File == null || request.File.Length == 0)
            {
                TempData["Error"] = "Por favor, selecione um arquivo PDF.";
                return RedirectToAction("Split");
            }

            if (!PdfHelper.IsPdfFile(request.File))
            {
                TempData["Error"] = "O arquivo deve ser um PDF válido.";
                return RedirectToAction("Split");
            }

            if (!request.SplitAllPages)
            {
                var pageNumbers = request.PageNumbers;
                if (pageNumbers == null || !pageNumbers.Any())
                {
                    TempData["Error"] = "Digite pelo menos um número de página ou marque 'Dividir todas'.";
                    return RedirectToAction("Split");
                }
            }

            var results = await _pdfService.SplitAsync(request);

            if (results == null || results.Count == 0)
            {
                TempData["Error"] = "Nenhuma página foi extraída. Verifique os números informados.";
                return RedirectToAction("Split");
            }

            if (results.Count == 1)
            {
                TempData["Success"] = "✅ Página extraída com sucesso!";
                var pageNum = request.PageNumbers?.FirstOrDefault() ?? 1;
                return File(results[0], "application/pdf", $"pagina_{pageNum}.pdf");
            }

            TempData["Success"] = $"✅ PDF dividido em {results.Count} páginas!";
            return File(results[0], "application/pdf", "dividido.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao dividir PDF");
            TempData["Error"] = $"Erro ao dividir: {ex.Message}";
            return RedirectToAction("Split");
        }
    }

    #endregion

    #region Convert

    [HttpGet]
    public IActionResult Convert()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Convert(ConvertRequest request)
    {
        try
        {
            if (request.File == null || request.File.Length == 0)
            {
                TempData["Error"] = "Por favor, selecione um arquivo PDF.";
                return RedirectToAction("Convert");
            }

            if (!PdfHelper.IsPdfFile(request.File))
            {
                TempData["Error"] = "O arquivo deve ser um PDF válido.";
                return RedirectToAction("Convert");
            }

            var result = await _pdfService.ConvertAsync(request);
            
            var extension = request.TargetFormat.ToString().ToLower();
            var fileName = $"convertido.{extension}";

            TempData["Success"] = "✅ PDF convertido com sucesso!";
            return File(result, "application/octet-stream", fileName);
        }
        catch (NotImplementedException)
        {
            TempData["Error"] = "🚧 Esta conversão ainda está em desenvolvimento. Em breve estará disponível!";
            return RedirectToAction("Convert");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao converter PDF");
            TempData["Error"] = $"Erro ao converter: {ex.Message}";
            return RedirectToAction("Convert");
        }
    }

    #endregion
}