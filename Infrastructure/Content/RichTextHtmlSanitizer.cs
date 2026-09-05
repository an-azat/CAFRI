using Ganss.Xss;

namespace CAFRI.Infrastructure.Content;

// Matches the formatting exposed by the Quill toolbar on the executive summary
// editor (Areas/Admin/Views/Publications/Edit.cshtml): headers, bold/italic/underline,
// lists, blockquote, and links. Everything else — scripts, iframes, event handler
// attributes, javascript: URIs, inline styles — is stripped by the allowlist.
public static class RichTextHtmlSanitizer
{
    private static readonly HtmlSanitizer Sanitizer = CreateSanitizer();

    public static string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        return Sanitizer.Sanitize(html.Trim()).Trim();
    }

    private static HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer(new HtmlSanitizerOptions
        {
            AllowedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "p", "br", "h2", "h3", "strong", "b", "em", "i", "u", "s",
                "ol", "ul", "li", "blockquote", "a"
            },
            AllowedAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "href" },
            UriAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "href" },
            AllowedSchemes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "http", "https", "mailto" }
        });

        sanitizer.PostProcessNode += (_, args) =>
        {
            if (args.Node is AngleSharp.Dom.IElement { TagName: "A" } anchor && anchor.HasAttribute("href"))
            {
                anchor.SetAttribute("rel", "noopener noreferrer");
                anchor.SetAttribute("target", "_blank");
            }
        };

        return sanitizer;
    }
}
