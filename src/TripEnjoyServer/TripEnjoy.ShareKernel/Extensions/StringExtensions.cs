using System.Globalization;
using System.Linq;
using System.Text;

namespace TripEnjoy.ShareKernel.Extensions;

public static class StringExtensions
{
    public static string ToTitleCase(this string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return s;

        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
    }

    /// <summary>
    /// Sanitizes a filename by removing or replacing special characters that can cause issues
    /// with cloud storage signature generation and URL encoding.
    /// </summary>
    /// <param name="fileName">The original filename to sanitize</param>
    /// <param name="maxLength">Maximum length of the sanitized filename (default: 50)</param>
    /// <param name="fallbackName">Fallback name if sanitization results in empty string (default: "document")</param>
    /// <returns>A sanitized filename safe for use in cloud storage operations</returns>
    public static string SanitizeFileName(this string fileName, int maxLength = 50, string fallbackName = "document")
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return fallbackName;

        // Remove file extension if present
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        
        // Replace Vietnamese and other special characters with ASCII equivalents
        var sanitized = nameWithoutExtension
            // Vietnamese characters - lowercase
            .Replace("á", "a").Replace("à", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
            .Replace("ă", "a").Replace("ắ", "a").Replace("ằ", "a").Replace("ẳ", "a").Replace("ẵ", "a").Replace("ặ", "a")
            .Replace("â", "a").Replace("ấ", "a").Replace("ầ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a")
            .Replace("é", "e").Replace("è", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e")
            .Replace("ê", "e").Replace("ế", "e").Replace("ề", "e").Replace("ể", "e").Replace("ễ", "e").Replace("ệ", "e")
            .Replace("í", "i").Replace("ì", "i").Replace("ỉ", "i").Replace("ĩ", "i").Replace("ị", "i")
            .Replace("ó", "o").Replace("ò", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o")
            .Replace("ô", "o").Replace("ố", "o").Replace("ồ", "o").Replace("ổ", "o").Replace("ỗ", "o").Replace("ộ", "o")
            .Replace("ơ", "o").Replace("ớ", "o").Replace("ờ", "o").Replace("ở", "o").Replace("ỡ", "o").Replace("ợ", "o")
            .Replace("ú", "u").Replace("ù", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u")
            .Replace("ư", "u").Replace("ứ", "u").Replace("ừ", "u").Replace("ử", "u").Replace("ữ", "u").Replace("ự", "u")
            .Replace("ý", "y").Replace("ỳ", "y").Replace("ỷ", "y").Replace("ỹ", "y").Replace("ỵ", "y")
            .Replace("đ", "d")
            // Vietnamese characters - uppercase
            .Replace("Á", "A").Replace("À", "A").Replace("Ả", "A").Replace("Ã", "A").Replace("Ạ", "A")
            .Replace("Ă", "A").Replace("Ắ", "A").Replace("Ằ", "A").Replace("Ẳ", "A").Replace("Ẵ", "A").Replace("Ặ", "A")
            .Replace("Â", "A").Replace("Ấ", "A").Replace("Ầ", "A").Replace("Ẩ", "A").Replace("Ẫ", "A").Replace("Ậ", "A")
            .Replace("É", "E").Replace("È", "E").Replace("Ẻ", "E").Replace("Ẽ", "E").Replace("Ẹ", "E")
            .Replace("Ê", "E").Replace("Ế", "E").Replace("Ề", "E").Replace("Ể", "E").Replace("Ễ", "E").Replace("Ệ", "E")
            .Replace("Í", "I").Replace("Ì", "I").Replace("Ỉ", "I").Replace("Ĩ", "I").Replace("Ị", "I")
            .Replace("Ó", "O").Replace("Ò", "O").Replace("Ỏ", "O").Replace("Õ", "O").Replace("Ọ", "O")
            .Replace("Ô", "O").Replace("Ố", "O").Replace("Ồ", "O").Replace("Ổ", "O").Replace("Ỗ", "O").Replace("Ộ", "O")
            .Replace("Ơ", "O").Replace("Ớ", "O").Replace("Ờ", "O").Replace("Ở", "O").Replace("Ỡ", "O").Replace("Ợ", "O")
            .Replace("Ú", "U").Replace("Ù", "U").Replace("Ủ", "U").Replace("Ũ", "U").Replace("Ụ", "U")
            .Replace("Ư", "U").Replace("Ứ", "U").Replace("Ừ", "U").Replace("Ử", "U").Replace("Ữ", "U").Replace("Ự", "U")
            .Replace("Ý", "Y").Replace("Ỳ", "Y").Replace("Ỷ", "Y").Replace("Ỹ", "Y").Replace("Ỵ", "Y")
            .Replace("Đ", "D")
            // Common accented characters from other languages
            .Replace("ç", "c").Replace("Ç", "C")
            .Replace("ñ", "n").Replace("Ñ", "N")
            .Replace("ß", "ss")
            .Replace("æ", "ae").Replace("Æ", "AE")
            .Replace("œ", "oe").Replace("Œ", "OE")
            .Replace("ø", "o").Replace("Ø", "O")
            .Replace("å", "a").Replace("Å", "A");

        // Remove or replace other problematic characters
        var result = new StringBuilder();
        foreach (char c in sanitized)
        {
            if (char.IsLetterOrDigit(c))
            {
                result.Append(c);
            }
            else if (char.IsWhiteSpace(c) || c == '-' || c == '_' || c == '.')
            {
                // Replace spaces and common separators with underscores
                if (result.Length > 0 && result[result.Length - 1] != '_')
                {
                    result.Append('_');
                }
            }
            // Skip other special characters
        }

        // Clean up the result
        var finalResult = result.ToString()
            .Trim('_') // Remove leading/trailing underscores
            .Replace("__", "_"); // Replace multiple underscores with single

        // Ensure we have a valid filename
        if (string.IsNullOrWhiteSpace(finalResult))
        {
            finalResult = fallbackName;
        }

        // Limit length to prevent issues (cloud storage public IDs have limits)
        if (finalResult.Length > maxLength)
        {
            finalResult = finalResult.Substring(0, maxLength - 3) + "...";
        }

        return finalResult;
    }
}
