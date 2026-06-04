using Planova.Excel.Models;
using Xunit;

namespace Planova.Excel.Tests.Models;

public class ModelTests
{
    [Fact]
    public void WorkbookInfo_DefaultValues()
    {
        var info = new WorkbookInfo();

        Assert.Equal(string.Empty, info.FilePath);
        Assert.Equal(0L, info.FileSize);
        Assert.Empty(info.Worksheets);
        Assert.False(info.IsValid);
        Assert.Equal(string.Empty, info.Format);
    }

    [Fact]
    public void ImportRequest_DefaultBatchSize()
    {
        var request = new ImportRequest();

        Assert.Equal(1000, request.BatchSize);
        Assert.Equal(DuplicateStrategy.Prompt, request.DuplicateHandling);
    }

    [Fact]
    public void ValidationResult_DefaultIsValid()
    {
        var result = new ValidationResult();

        Assert.False(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void ValidationError_Properties()
    {
        var error = new ValidationError
        {
            RowIndex = 1,
            ColumnName = "Name",
            ErrorType = "Required",
            Message = "Name is required",
            RawValue = null
        };

        Assert.Equal(1, error.RowIndex);
        Assert.Equal("Name", error.ColumnName);
        Assert.Equal("Required", error.ErrorType);
        Assert.Equal("Name is required", error.Message);
    }

    [Fact]
    public void MappingProfile_DefaultVersion()
    {
        var profile = new MappingProfile();

        Assert.Equal(Guid.Empty, profile.Id);
        Assert.Equal(0, profile.Version);
        Assert.Empty(profile.ColumnMappings);
        Assert.Empty(profile.ValidationRules);
    }

    [Fact]
    public void ExportRequest_Defaults()
    {
        var request = new ExportRequest();

        Assert.True(request.IncludeHeaders);
        Assert.Empty(request.SelectedColumns);
        Assert.Empty(request.OutputPath);
    }
}
