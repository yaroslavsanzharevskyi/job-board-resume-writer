using iText.Html2pdf;
using iText.Kernel.Pdf;
using Markdig;

namespace DatabricksJobsFunction;

public class PdfService
{
    private static readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    public byte[] ConvertMarkdownToPdf(string markdown)
    {
        var html = """
            <!DOCTYPE html>
            <html>
            <head>
            <meta charset="utf-8"/>
            <style>
              body { font-family: Arial, sans-serif; font-size: 11pt; margin: 40px; color: #1a1a1a; }
              h1 { font-size: 20pt; margin-bottom: 4px; }
              h2 { font-size: 14pt; margin-top: 18px; margin-bottom: 4px; border-bottom: 1px solid #ccc; padding-bottom: 2px; }
              h3 { font-size: 12pt; margin-top: 12px; margin-bottom: 2px; }
              ul { margin: 4px 0; padding-left: 20px; }
              li { margin-bottom: 2px; }
              p { margin: 4px 0; }
              strong { font-weight: bold; }
              em { font-style: italic; }
            </style>
            </head>
            <body>
            """ + Markdown.ToHtml(markdown, _pipeline) + """
            </body>
            </html>
            """;

        using var output = new MemoryStream();
        using var writer = new PdfWriter(output);
        using var pdf = new PdfDocument(writer);
        HtmlConverter.ConvertToPdf(html, pdf, new ConverterProperties());
        pdf.Close();
        return output.ToArray();
    }
}
