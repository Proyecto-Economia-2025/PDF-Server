using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PDF_Server.Domain.Models;
using System.Globalization;

namespace PDF_Server.Infrastructure.PDFs
{
    public class TopProductsPdfDocument : IDocument
    {
        private readonly TopProductsRequest _request;
        private readonly List<ProductSale> _products;

        private const string ContactName = "Antony Monge Lopez";
        private const string ContactPhone = "(+506) 8545-6150";
        private const string ContactEmail = "antonyml2016@gmail.com";

        public TopProductsPdfDocument(List<ProductSale> products, TopProductsRequest request)
        {
            _products = products ?? throw new ArgumentNullException(nameof(products));
            _request = request ?? throw new ArgumentNullException(nameof(request));

            Console.WriteLine($"🔹 TopProductsPdfDocument creado con {_products.Count} productos.");
        }

        public void Compose(IDocumentContainer container)
        {
            try
            {
                Console.WriteLine("🔹 Iniciando Compose...");
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(10);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial").FontColor(Colors.Grey.Darken3));

                    try { page.Header().Element(ComposeHeader); } catch (Exception ex) { Console.WriteLine($"⚠️ Error Header: {ex}"); }
                    try { page.Content().Element(ComposeContent); } catch (Exception ex) { Console.WriteLine($"⚠️ Error Content: {ex}"); }
                    try { page.Footer().Element(ComposeFooter); } catch (Exception ex) { Console.WriteLine($"⚠️ Error Footer: {ex}"); }
                });
                Console.WriteLine("🔹 Compose completado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Excepción en Compose: {ex}");
                throw;
            }
        }


        private void ComposeHeader(IContainer container)
        {
            Console.WriteLine("🔹 Componiendo Header...");
            container.Column(col =>
            {
                // Cambia Height(120) por MinHeight(100) y elimina el constraint conflictivo
                col.Item().Background(Colors.Blue.Darken2).MinHeight(100).Padding(20).Column(headerCol =>
                {
                    headerCol.Item().AlignCenter().Text("REPORTE DE VENTAS")
                        .FontSize(16).Bold().FontColor(Colors.White).LetterSpacing(1.2f);

                    headerCol.Item().PaddingTop(3).AlignCenter()
                        .Text("Productos Más Vendidos")
                        .FontSize(15).FontColor(Colors.White);


                });

                // Barra decorativa 
                col.Item().Row(row =>
                {
                    row.RelativeItem(3).Background(Colors.Purple.Medium).Height(4);
                    row.RelativeItem(2).Background(Colors.Orange.Medium).Height(4);
                    row.RelativeItem(1).Background(Colors.LightGreen.Darken1).Height(4);
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            Console.WriteLine("🔹 Iniciando ComposeContent...");

            container.PaddingHorizontal(40).PaddingVertical(20).Column(col =>
            {

                // Información general del reporte
                col.Item().PaddingBottom(25).Column(cardCol =>
                {
                    Console.WriteLine("🔹 Componiendo información general...");
                    ComposeInfoGrid(cardCol);
                });

                //información de productos
                col.Item().Column(productsCol =>
                {
                    Console.WriteLine($"🔹 Número de productos: {_products?.Count ?? 0}");

                    productsCol.Item().PaddingBottom(15).Row(row =>
                    {
                        row.AutoItem().Width(4).Height(20).Background(Colors.Purple.Medium);
                        row.RelativeItem().PaddingLeft(12).AlignMiddle().Row(titleRow =>
                        {
                            titleRow.AutoItem().Text("Top Productos")
                                .FontSize(16).Bold().FontColor(Colors.Grey.Darken3);
                            titleRow.AutoItem().PaddingLeft(10)
                                .PaddingHorizontal(12).PaddingVertical(4)
                                .Background(Colors.Orange.Medium)
                                .Text((_products?.Count ?? 0).ToString())
                                .FontSize(11).Bold().FontColor(Colors.White);
                        });
                    });

                    if (_products != null && _products.Any())
                    {
                        var maxSales = _products.Max(p => p.TotalSold);

                        // UNA SOLA TABLA para todos los productos
                        productsCol.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);   // Ranking (reducido de 60)
                                                              //  columns.ConstantColumn(70);   // ID (reducido de 90)
                                columns.RelativeColumn(4);    // Producto (aumentado para más espacio)
                                columns.RelativeColumn(2);    // Ventas
                                columns.RelativeColumn(3);    // Rendimiento
                            });

                            // Header de la tabla
                            table.Header(header =>
                            {
                                void HeaderCell(string text)
                                {
                                    header.Cell()
                                        .Background(Colors.Grey.Darken3)
                                        .Padding(8)
                                        .Text(text)
                                        .FontSize(10)
                                        .Bold()
                                        .FontColor(Colors.White);
                                }

                                HeaderCell("#");
                                //  HeaderCell("ID");
                                HeaderCell("PRODUCTO");
                                HeaderCell("VENTAS");
                                HeaderCell("RENDIMIENTO");
                            });

                            // Filas de productos
                            int ranking = 1;
                            foreach (var product in _products)
                            {
                                if (product == null)
                                {
                                    Console.WriteLine($"⚠️ Producto nulo en posición {ranking}");
                                    continue;
                                }

                                Console.WriteLine($"Renderizando producto #{ranking} -> ID: {product.ProductId}, Nombre: {product.Name}, TotalSold: {product.TotalSold}");

                                var rowColor = ranking % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                                var performance = maxSales > 0 ? (double)product.TotalSold / maxSales : 0;

                                table.Cell().Background(GetRankingColor(ranking)).Padding(10)
                                    .AlignCenter().AlignMiddle()
                                    .Text(ranking.ToString())
                                    .FontSize(14).Bold().FontColor(Colors.White);

                                table.Cell().Background(rowColor).Padding(8).AlignMiddle()
                                    .Text(product.Name ?? "-")
                                    .FontSize(10).FontColor(Colors.Grey.Darken3);

                                table.Cell().Background(rowColor).Padding(8).AlignMiddle()
                                    .Text(product.TotalSold.ToString("N0", CultureInfo.InvariantCulture))
                                    .FontSize(11).Bold().FontColor(Colors.Purple.Medium);

                                table.Cell().Background(rowColor).Padding(8).AlignMiddle()
                                    .Row(barRow =>
                                    {
                                        barRow.RelativeItem().Height(10).Background(Colors.Grey.Lighten2)
                                            .Row(innerBar =>
                                            {
                                                if (performance > 0)
                                                {
                                                    innerBar.RelativeItem((float)performance)
                                                        .Height(10)
                                                        .Background(GetPerformanceColor(performance));
                                                }
                                                else
                                                {
                                                    // Si performance es 0, mostrar solo un pixel mínimo
                                                    innerBar.ConstantItem(1)
                                                        .Height(10)
                                                        .Background(GetPerformanceColor(performance));
                                                }

                                                // Espacio vacío - CON PROTECCIÓN CONTRA CERO
                                                var emptySpace = 1 - performance;
                                                if (emptySpace > 0)
                                                {
                                                    innerBar.RelativeItem((float)emptySpace);
                                                }
                                                else
                                                {
                                                    innerBar.ConstantItem(0); // Sin espacio si está lleno
                                                }
                                            });

                                        barRow.AutoItem().PaddingLeft(8)
                                            .Text($"{(performance * 100):F0}%")
                                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                                    });

                                ranking++;
                            }
                        });
                    }
                    else
                    {
                        Console.WriteLine("⚠️ No hay productos para mostrar.");
                        productsCol.Item().PaddingVertical(40).AlignCenter()
                            .Text("No hay productos para mostrar")
                            .FontSize(12).Italic().FontColor(Colors.Grey.Medium);
                    }
                });


            });

            Console.WriteLine("🔹 ComposeContent completado.");
        }

        private void ComposeInfoGrid(ColumnDescriptor col)
        {
            Console.WriteLine("🔹 Componiendo InfoGrid...");
            var infoItems = new[]
            {
                ("🔑 Correlation ID", _request.CorrelationId ?? "-"),
                ("👤 Cliente", _request.Payload?.Metadata?.ClientType ?? "-")
            };

            for (int i = 0; i < infoItems.Length; i += 2)
            {
                col.Item().PaddingVertical(8).Row(row =>
                {
                    for (int j = 0; j < 2 && i + j < infoItems.Length; j++)
                    {
                        row.RelativeItem().PaddingRight(j == 0 ? 15 : 0).Column(itemCol =>
                        {
                            itemCol.Item().Text(infoItems[i + j].Item1)
                                .FontSize(9).FontColor(Colors.Grey.Darken1);
                            itemCol.Item().PaddingTop(3).Text(infoItems[i + j].Item2)
                                .FontSize(11).SemiBold().FontColor(Colors.Grey.Darken3);
                        });
                    }
                });

                if (i + 2 < infoItems.Length)
                {
                    col.Item().PaddingVertical(8).LineHorizontal(1)
                        .LineColor(Colors.Grey.Lighten2);
                }
            }
        }

        private void ComposeFooter(IContainer container)
        {
            Console.WriteLine("🔹 Componiendo Footer...");
            container.Background(Colors.Grey.Lighten4).Padding(20).Column(col =>
            {
                col.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(15);

                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(contactCol =>
                    {
                        contactCol.Item().Text(ContactName)
                            .FontSize(9).Bold().FontColor(Colors.Grey.Darken3);
                        contactCol.Item().Text($"{ContactPhone} • {ContactEmail}")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
                    });

                    row.AutoItem().AlignRight().Text(text =>
                    {
                        text.Span("Página ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                        text.Span(" de ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        text.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
                        text.Span($" • {DateTime.Now:dd/MM/yyyy hh:mm tt}").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                });
            });
        }

        private string GetRankingColor(int ranking)
        {
            return ranking switch
            {
                1 => Colors.Orange.Medium,
                2 => Colors.Grey.Medium,
                3 => Colors.Orange.Darken1,
                _ => Colors.Blue.Darken2
            };
        }

        private string GetPerformanceColor(double performance)
        {
            if (performance >= 0.8)
                return Colors.Green.Medium;
            else if (performance >= 0.5)
                return Colors.Orange.Medium;
            else
                return Colors.Red.Medium;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public byte[] GeneratePdfBytes()
        {
            try
            {
                Console.WriteLine($"🔹 Generando PDF en memoria...");
                Console.WriteLine($"🔹 Iniciando generación del PDF...");

                var pdfBytes = this.GeneratePdf();

                Console.WriteLine($"✅ PDF generado exitosamente en memoria ({pdfBytes.Length} bytes)");
                return pdfBytes;
            }
            catch (QuestPDF.Drawing.Exceptions.DocumentLayoutException ex)
            {
                Console.WriteLine($"❌ DocumentLayoutException: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error general al generar PDF: {ex.GetType().Name}");
                Console.WriteLine($"❌ Mensaje: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        private string FormatTimestamp(DateTime timestamp)
        {
            return timestamp.ToString("hh:mm tt dd/MM/yyyy", new CultureInfo("es-CR"));
        }
    }
}