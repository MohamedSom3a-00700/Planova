using System.IO;
using QRCoder;

namespace Planova.UI.Services;

public class QrCodeService
{
    public string GenerateLocationQr(int projectId, double latitude, double longitude)
    {
        var geoUri = $"geo:{latitude},{longitude}";

        using var generator = new QRCodeGenerator();
        using var qrData = generator.CreateQrCode(geoUri, QRCodeGenerator.ECCLevel.Q);
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
}
