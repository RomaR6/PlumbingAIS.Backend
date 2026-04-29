using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.Interfaces;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet("download/{transactionId}")]
        public async Task<IActionResult> DownloadInvoice(int transactionId)
        {
            var pdfBytes = await _invoiceService.GenerateInvoicePdfAsync(transactionId);

            if (pdfBytes == null)
                return NotFound(new { message = "Накладну не знайдено" });

            return File(pdfBytes, "application/pdf", $"invoice_{transactionId}.pdf");
        }
    }
}