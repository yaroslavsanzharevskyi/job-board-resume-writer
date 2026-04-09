using iText.Html2pdf;
using iText.Kernel.Pdf;
using Markdig;

var markdown = """
    # Jane Smith
    **Email:** jane.smith@email.com | **Phone:** +44 7700 900000 | **Location:** London, UK

    ## Summary
    Experienced software engineer with 8 years building distributed systems and cloud-native applications.
    Strong background in .NET, Azure, and event-driven architecture.

    ## Experience

    ### Senior Software Engineer — Acme Corp *(2021 – Present)*
    - Led migration of monolithic application to microservices on Azure Kubernetes Service
    - Reduced deployment time by 70% through CI/CD pipeline improvements
    - Mentored a team of 4 junior engineers

    ### Software Engineer — Beta Ltd *(2018 – 2021)*
    - Built real-time data processing pipeline handling 500k events/day using Azure Event Hubs
    - Delivered REST APIs consumed by 3 external partners

    ## Education
    **BSc Computer Science** — University of Manchester *(2014 – 2018)*

    ## Skills
    C#, .NET 8, Azure Functions, Cosmos DB, Blob Storage, Databricks, SQL, React, TypeScript
    """;

var pipeline = new MarkdownPipelineBuilder()
    .UseAdvancedExtensions()
    .Build();

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
    """ + Markdown.ToHtml(markdown, pipeline) + """
    </body>
    </html>
    """;

var outputPath = Path.Combine(AppContext.BaseDirectory, "output.pdf");

using var output = new FileStream(outputPath, FileMode.Create);
using var writer = new PdfWriter(output);
using var pdf = new PdfDocument(writer);
HtmlConverter.ConvertToPdf(html, pdf, new ConverterProperties());
pdf.Close();

Console.WriteLine($"PDF written to: {outputPath}");
