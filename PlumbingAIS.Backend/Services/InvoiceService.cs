using Microsoft.EntityFrameworkCore;
using PlumbingAIS.Backend.Data;
using PlumbingAIS.Backend.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ModelUnit = PlumbingAIS.Backend.Models.Unit;

namespace PlumbingAIS.Backend.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly AppDbContext _context;

        public InvoiceService(AppDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateInvoicePdfAsync(int transactionId)
        {
            var transaction = await _context.Transactions
                .Include(t => t.User)
                .Include(t => t.Contractor)
                .Include(t => t.TransactionItems)
                    .ThenInclude(ti => ti.Product)
                        .ThenInclude(p => p != null ? p.Unit : null)
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction == null) return Array.Empty<byte>();

            var isMove = transaction.Type.Equals("Move", StringComparison.OrdinalIgnoreCase);
            var displayItems = isMove
                ? transaction.TransactionItems.GroupBy(ti => ti.ProductId).Select(g => g.First()).ToList()
                : transaction.TransactionItems.ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, QuestPDF.Infrastructure.Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Plumbing AIS - Склад").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text($"{GetDocumentTitle(transaction.Type)} № {transaction.DocumentNumber}").FontSize(14).SemiBold();
                            col.Item().Text($"Дата: {transaction.Date:dd.MM.yyyy HH:mm}");
                            if (!string.IsNullOrEmpty(transaction.Description))
                                col.Item().Text(transaction.Description).Italic().FontSize(9);
                        });
                    });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        if (!isMove)
                        {
                            col.Item().BorderBottom(1).PaddingBottom(5).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Відправник:").SemiBold();
                                    c.Item().Text(transaction.Type == "In" ? transaction.Contractor?.Name ?? "Постачальник" : "Головний Склад");
                                });
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Отримувач:").SemiBold();
                                    c.Item().Text(transaction.Type == "In" ? "Головний Склад" : transaction.Contractor?.Name ?? "Отримувач");
                                });
                            });
                        }

                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(50);
                                if (!isMove)
                                {
                                    columns.ConstantColumn(70);
                                    columns.ConstantColumn(80);
                                }
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("№");
                                header.Cell().Element(CellStyle).Text("Товар (Артикул)");
                                header.Cell().Element(CellStyle).Text("Кількість");
                                header.Cell().Element(CellStyle).Text("Од.");
                                if (!isMove)
                                {
                                    header.Cell().Element(CellStyle).Text("Ціна");
                                    header.Cell().Element(CellStyle).Text("Сума");
                                }

                                static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1);
                            });

                            int index = 1;
                            foreach (var item in displayItems)
                            {
                                table.Cell().Element(CellStyle).Text(index++.ToString());
                                table.Cell().Element(CellStyle).Text($"{item.Product?.Name} ({item.Product?.SKU})");
                                table.Cell().Element(CellStyle).Text(item.Quantity.ToString("F2"));
                                table.Cell().Element(CellStyle).Text(item.Product?.Unit?.Name ?? "шт");

                                if (!isMove)
                                {
                                    table.Cell().Element(CellStyle).Text(item.PriceAtTime.ToString("F2"));
                                    table.Cell().Element(CellStyle).Text((item.Quantity * item.PriceAtTime).ToString("F2"));
                                }

                                static IContainer CellStyle(IContainer container) => container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                            }
                        });

                        if (!isMove)
                        {
                            col.Item().AlignRight().PaddingTop(10).Text(x =>
                            {
                                var total = displayItems.Sum(ti => ti.Quantity * ti.PriceAtTime);
                                x.Span("Разом: ").FontSize(12).SemiBold();
                                x.Span($"{total:F2} грн").FontSize(12).SemiBold();
                            });
                        }
                    });

                    page.Footer().PaddingTop(20).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Відпустив: ___________");
                            var userName = transaction.User != null
                                ? $"{transaction.User.FirstName} {transaction.User.LastName}".Trim()
                                : "Відповідальна особа";
                            c.Item().Text(userName).FontSize(8);
                        });
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("Прийняв: ___________");
                            c.Item().Text(isMove ? "Комірник (Отримувач)" : "Отримувач").FontSize(8);
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        private string GetDocumentTitle(string type) => type switch
        {
            "In" => "Прибуткова накладна",
            "Out" => "Видаткова накладна",
            "Move" => "Акт внутрішнього переміщення",
            _ => "Накладна"
        };
    }
}