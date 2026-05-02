using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;
using System.Security.Claims;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public class ReportsController : ControllerBase
    {
        private readonly IStockService _stockService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerService _logger;

        public ReportsController(IStockService stockService, IUnitOfWork unitOfWork, ILoggerService logger)
        {
            _stockService = stockService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("total-value")]
        public async Task<IActionResult> GetTotalValue() =>
            Ok(new { totalValue = await _stockService.GetTotalStockValueAsync(), currency = "UAH" });

        [HttpGet("inventory-check")]
        public async Task<IActionResult> GetInventoryReport()
        {
            var critical = await _stockService.GetCriticalStocksAsync();
            var products = await _unitOfWork.Products.GetAllAsync();

            var result = critical.Select(i => {
                var product = products.FirstOrDefault(p => p.SKU.Trim().ToLower() == i.SKU.Trim().ToLower());
                return new
                {
                    productId = product?.Id,
                    id = product?.Id,
                    name = i.Name,
                    sku = i.SKU,
                    minThreshold = i.MinThreshold,
                    quantity = i.CurrentQuantity,
                    currentQuantity = i.CurrentQuantity
                };
            });

            return Ok(new { reportDate = DateTime.Now, items = result });
        }

        [HttpGet("export-csv")]
        public async Task<IActionResult> ExportCsv()
        {
            var items = await _stockService.GetCriticalStocksAsync();
            var csv = new StringBuilder();
            csv.AppendLine("Артикул;Назва товару;Залишок;Поріг;Статус");

            foreach (var item in items)
            {
                string status = item.CurrentQuantity < item.MinThreshold ? "ДЕФІЦИТ" : "ОК";
                csv.AppendLine($"{item.SKU};{item.Name};{item.CurrentQuantity};{item.MinThreshold};{status}");
            }

            await _logger.LogActionAsync("Експорт звіту інвентаризації (CSV)", GetUserId());
            return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(),
                "text/csv", $"inventory_{DateTime.Now:yyyyMMdd}.csv");
        }

        [HttpGet("export-pdf")]
        public async Task<IActionResult> ExportPdf()
        {
            var items = await _stockService.GetCriticalStocksAsync();

            var document = Document.Create(container => {
                container.Page(page => {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().Text("ВІДОМІСТЬ ІНВЕНТАРИЗАЦІЇ").FontSize(20).SemiBold();

                    page.Content().PaddingVertical(10).Table(table => {
                        table.ColumnsDefinition(c => {
                            c.ConstantColumn(100);
                            c.RelativeColumn();
                            c.ConstantColumn(60);
                            c.ConstantColumn(60);
                            c.ConstantColumn(80);
                        });

                        table.Header(h => {
                            h.Cell().Text("Артикул").Bold();
                            h.Cell().Text("Назва").Bold();
                            h.Cell().Text("Залишок").Bold();
                            h.Cell().Text("Поріг").Bold();
                            h.Cell().Text("Статус").Bold();
                        });

                        foreach (var i in items)
                        {
                            bool isLow = i.CurrentQuantity < i.MinThreshold;
                            table.Cell().Text(i.SKU);
                            table.Cell().Text(i.Name);
                            table.Cell().Text(i.CurrentQuantity.ToString());
                            table.Cell().Text(i.MinThreshold.ToString());
                            table.Cell().Text(isLow ? "ДЕФІЦИТ" : "ОК")
                                .FontColor(isLow ? Colors.Red.Medium : Colors.Green.Medium).Bold();
                        }
                    });
                });
            });

            await _logger.LogActionAsync("Експорт звіту інвентаризації (PDF)", GetUserId());
            return File(document.GeneratePdf(), "application/pdf", $"inventory_{DateTime.Now:yyyyMMdd}.pdf");
        }
    }
}