using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumbingAIS.Backend.Data;
using PlumbingAIS.Backend.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly IStockService _stockService;
        private readonly AppDbContext _context;

        public ReportsController(IStockService stockService, AppDbContext context)
        {
            _stockService = stockService;
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        [HttpGet("total-value")]
        public async Task<IActionResult> GetTotalValue()
        {
            var value = await _stockService.GetTotalStockValueAsync();
            return Ok(new { totalValue = value, currency = "UAH" });
        }

        [HttpGet("inventory-check")]
        public async Task<IActionResult> GetInventoryReport()
        {
            var criticalItems = await _context.Products
                .Where(p => _context.Stocks.Any(s => s.ProductId == p.Id))
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.SKU,
                    Quantity = _context.Stocks
                        .Where(s => s.ProductId == p.Id)
                        .Sum(s => (int?)s.Quantity) ?? 0
                })
                .Where(x => x.Quantity < 5)
                .ToListAsync();

            return Ok(new
            {
                reportDate = DateTime.Now,
                criticalItemsCount = criticalItems.Count,
                items = criticalItems
            });
        }

        [HttpGet("export-csv")]
        public async Task<IActionResult> ExportCsv()
        {
            var items = await _context.Products
                .Where(p => _context.Stocks.Any(s => s.ProductId == p.Id))
                .Select(p => new
                {
                    p.SKU,
                    p.Name,
                    Quantity = _context.Stocks
                        .Where(s => s.ProductId == p.Id)
                        .Sum(s => (int?)s.Quantity) ?? 0
                })
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Артикул;Назва товару;Залишок;Статус");

            foreach (var item in items)
            {
                string status = item.Quantity < 5 ? "ДЕФІЦИТ" : "В НАЯВНОСТІ";
                csv.AppendLine($"{item.SKU};{item.Name};{item.Quantity};{status}");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
            return File(bytes, "text/csv", $"inventory_{DateTime.Now:yyyyMMdd}.csv");
        }

        [HttpGet("export-pdf")]
        public async Task<IActionResult> ExportPdf()
        {
            var items = await _context.Products
                .Where(p => _context.Stocks.Any(s => s.ProductId == p.Id))
                .Select(p => new
                {
                    p.SKU,
                    p.Name,
                    Quantity = _context.Stocks
                        .Where(s => s.ProductId == p.Id)
                        .Sum(s => (int?)s.Quantity) ?? 0
                })
                .ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("ВІДОМІСТЬ ІНВЕНТАРИЗАЦІЇ").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text($"Дата формування: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(10).FontColor(Colors.Grey.Medium);
                        });
                    });

                    page.Content().PaddingVertical(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(100);
                            columns.RelativeColumn();
                            columns.ConstantColumn(80);
                            columns.ConstantColumn(100);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Артикул");
                            header.Cell().Element(CellStyle).Text("Назва товару");
                            header.Cell().Element(CellStyle).Text("Залишок");
                            header.Cell().Element(CellStyle).Text("Статус");

                            static IContainer CellStyle(IContainer container) =>
                                container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                        });

                        foreach (var item in items)
                        {
                            table.Cell().Element(RowStyle).Text(item.SKU);
                            table.Cell().Element(RowStyle).Text(item.Name);
                            table.Cell().Element(RowStyle).Text(item.Quantity.ToString());

                            var isLow = item.Quantity < 5;
                            table.Cell().Element(RowStyle).Text(isLow ? "ДЕФІЦИТ" : "ОК")
                                 .FontColor(isLow ? Colors.Red.Medium : Colors.Green.Medium).Bold();

                            static IContainer RowStyle(IContainer container) =>
                                container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Сторінка ");
                        x.CurrentPageNumber();
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"inventory_report_{DateTime.Now:yyyyMMdd}.pdf");
        }
    }
}