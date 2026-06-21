using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using QRCoder;

namespace Planova.UI.Services;

public class QrCodeService
{
    public string GenerateLocationQr(int projectId, double latitude, double longitude)
    {
        var latStr = latitude.ToString(CultureInfo.InvariantCulture);
        var lngStr = longitude.ToString(CultureInfo.InvariantCulture);
        var mapsUrl = $"https://www.google.com/maps?q={latStr},{lngStr}";
        return GenerateQrFromUrl(projectId, mapsUrl);
    }

    public string GenerateQrFromUrl(int projectId, string url)
    {
        using var generator = new QRCodeGenerator();
        using var qrData = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        var qrBytes = qrCode.GetGraphic(20);

        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Planova", "Projects", projectId.ToString());

        Directory.CreateDirectory(folder);
        var filePath = Path.Combine(folder, "qr_location.png");
        File.WriteAllBytes(filePath, qrBytes);

        return filePath;
    }

    public static bool TryParseGoogleMapsLink(string? link, out double latitude, out double longitude)
    {
        latitude = 0;
        longitude = 0;

        if (string.IsNullOrWhiteSpace(link))
            return false;

        var match = Regex.Match(link, @"q=([+-]?\d+\.?\d*),([+-]?\d+\.?\d*)");
        if (match.Success
            && double.TryParse(match.Groups[1].Value, CultureInfo.InvariantCulture, out latitude)
            && double.TryParse(match.Groups[2].Value, CultureInfo.InvariantCulture, out longitude))
            return true;

        match = Regex.Match(link, @"@([+-]?\d+\.?\d*),([+-]?\d+\.?\d*)");
        if (match.Success
            && double.TryParse(match.Groups[1].Value, CultureInfo.InvariantCulture, out latitude)
            && double.TryParse(match.Groups[2].Value, CultureInfo.InvariantCulture, out longitude))
            return true;

        match = Regex.Match(link, @"([+-]?\d+\.?\d*),([+-]?\d+\.?\d*)");
        if (match.Success
            && double.TryParse(match.Groups[1].Value, CultureInfo.InvariantCulture, out latitude)
            && double.TryParse(match.Groups[2].Value, CultureInfo.InvariantCulture, out longitude))
            return true;

        return false;
    }
}
