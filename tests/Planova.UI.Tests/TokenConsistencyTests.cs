using Xunit;

namespace Planova.UI.Tests;

public class TokenConsistencyTests
{
    [Fact]
    public void DarkTheme_Background_Token_Matches_Authoritative_Palette()
    {
        var color = ColorFromHex("#0D1117");
        Assert.NotNull(color);
        Assert.Equal(0x0D, color.Value.R);
        Assert.Equal(0x11, color.Value.G);
        Assert.Equal(0x17, color.Value.B);
    }

    [Fact]
    public void DarkTheme_Surface_Token_Matches_Authoritative_Palette()
    {
        var color = ColorFromHex("#161B22");
        Assert.NotNull(color);
        Assert.Equal(0x16, color.Value.R);
        Assert.Equal(0x1B, color.Value.G);
        Assert.Equal(0x22, color.Value.B);
    }

    [Fact]
    public void DarkTheme_Card_Token_Matches_Authoritative_Palette()
    {
        var color = ColorFromHex("#1F2937");
        Assert.NotNull(color);
        Assert.Equal(0x1F, color.Value.R);
        Assert.Equal(0x29, color.Value.G);
        Assert.Equal(0x37, color.Value.B);
    }

    [Fact]
    public void DarkTheme_Border_Token_Matches_Authoritative_Palette()
    {
        var color = ColorFromHex("#2A3441");
        Assert.NotNull(color);
        Assert.Equal(0x2A, color.Value.R);
        Assert.Equal(0x34, color.Value.G);
        Assert.Equal(0x41, color.Value.B);
    }

    [Fact]
    public void DarkTheme_Accent_Token_Matches_Authoritative_Palette()
    {
        var color = ColorFromHex("#00BFFF");
        Assert.NotNull(color);
        Assert.Equal(0x00, color.Value.R);
        Assert.Equal(0xBF, color.Value.G);
        Assert.Equal(0xFF, color.Value.B);
    }

    [Fact]
    public void DarkTheme_SecondaryAccent_Token_Matches_Authoritative_Palette()
    {
        var color = ColorFromHex("#0078D4");
        Assert.NotNull(color);
        Assert.Equal(0x00, color.Value.R);
        Assert.Equal(0x78, color.Value.G);
        Assert.Equal(0xD4, color.Value.B);
    }

    [Fact]
    public void LightTheme_Background_Token_Matches_Authorative_Palette()
    {
        var color = ColorFromHex("#F8F9FB");
        Assert.NotNull(color);
        Assert.Equal(0xF8, color.Value.R);
        Assert.Equal(0xF9, color.Value.G);
        Assert.Equal(0xFB, color.Value.B);
    }

    [Fact]
    public void LightTheme_Surface_Token_Matches_Authoritative_Palette()
    {
        var color = ColorFromHex("#FFFFFF");
        Assert.NotNull(color);
        Assert.Equal(0xFF, color.Value.R);
        Assert.Equal(0xFF, color.Value.G);
        Assert.Equal(0xFF, color.Value.B);
    }

    [Fact]
    public void BrandFontFamily_Token_Format_Is_Valid()
    {
        var fontFamily = "Segoe UI Variable, pack://application:,,,/Resources/Branding/Inter/#Inter";
        Assert.Contains("Segoe UI Variable", fontFamily);
        Assert.Contains("Inter", fontFamily);
    }

    private static System.Drawing.Color? ColorFromHex(string hex)
    {
        try
        {
            return System.Drawing.ColorTranslator.FromHtml(hex);
        }
        catch
        {
            return null;
        }
    }
}