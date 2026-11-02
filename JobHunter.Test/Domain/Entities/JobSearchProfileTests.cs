
using JobHunter.Domain.Entities;

namespace JobHunter.Test.Domain.Entities;

public class JobSearchProfileTests
{
    private static JobSearchProfile CreateValidProfile()
    {
        return new JobSearchProfile(
            "Junior .NET Developer",
            "C#, .NET, Junior",
            "Budapest",
            true);
    }

    [Fact]
    public void Constructor_ValidData_CreatesProfile()
    {
        var profile = CreateValidProfile();

        Assert.NotEqual(Guid.Empty, profile.Id);
        Assert.Equal("Junior .NET Developer", profile.Name);
        Assert.Equal("C#, .NET, Junior", profile.Keywords);
        Assert.Equal("Budapest", profile.Location);
        Assert.True(profile.IncludeRemote);
        Assert.True(profile.IsEnabled);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_EmptyName_ThrowsException(
        string name)
    {
        Assert.Throws<ArgumentException>(() =>
            new JobSearchProfile(
                name,
                "C#",
                "Budapest"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_EmptyKeywords_ThrowsException(
        string keywords)
    {
        Assert.Throws<ArgumentException>(() =>
            new JobSearchProfile(
                "Developer",
                keywords,
                "Budapest"));
    }

    [Fact]
    public void Constructor_WhitespaceValues_TrimsProperties()
    {
        var profile = new JobSearchProfile(
            "  Developer  ",
            "  C#  ",
            "  Budapest  ");

        Assert.Equal("Developer", profile.Name);
        Assert.Equal("C#", profile.Keywords);
        Assert.Equal("Budapest", profile.Location);
    }

    [Fact]
    public void Constructor_EmptyLocation_UsesNull()
    {
        var profile = new JobSearchProfile(
            "Developer",
            "C#",
            " ");

        Assert.Null(profile.Location);
    }

    [Fact]
    public void Update_ValidData_UpdatesProfile()
    {
        var profile = CreateValidProfile();

        profile.Update(
            "Data Analyst",
            "SQL, Power BI",
            "Hungary",
            false);

        Assert.Equal("Data Analyst", profile.Name);
        Assert.Equal("SQL, Power BI", profile.Keywords);
        Assert.Equal("Hungary", profile.Location);
        Assert.False(profile.IncludeRemote);
    }

    [Fact]
    public void Update_InvalidName_DoesNotChangeProfile()
    {
        var profile = CreateValidProfile();

        Assert.Throws<ArgumentException>(() =>
            profile.Update(
                "",
                "SQL",
                "Hungary",
                false));

        Assert.Equal("Junior .NET Developer", profile.Name);
        Assert.Equal("C#, .NET, Junior", profile.Keywords);
    }

    [Fact]
    public void Update_InvalidKeywords_DoesNotChangeProfile()
    {
        var profile = CreateValidProfile();

        Assert.Throws<ArgumentException>(() =>
            profile.Update(
                "Data Analyst",
                "",
                "Hungary",
                false));

        Assert.Equal("Junior .NET Developer", profile.Name);
        Assert.Equal("C#, .NET, Junior", profile.Keywords);
    }

    [Fact]
    public void Disable_EnabledProfile_DisablesProfile()
    {
        var profile = CreateValidProfile();

        profile.Disable();

        Assert.False(profile.IsEnabled);
    }

    [Fact]
    public void Enable_DisabledProfile_EnablesProfile()
    {
        var profile = CreateValidProfile();
        profile.Disable();

        profile.Enable();

        Assert.True(profile.IsEnabled);
    }

    [Fact]
    public void Disable_AlreadyDisabled_DoesNotChangeTimestamp()
    {
        var profile = CreateValidProfile();
        profile.Disable();

        var timestamp = profile.UpdatedAt;

        profile.Disable();

        Assert.Equal(timestamp, profile.UpdatedAt);
    }

    [Fact]
    public void Enable_AlreadyEnabled_DoesNotChangeTimestamp()
    {
        var profile = CreateValidProfile();

        var timestamp = profile.UpdatedAt;

        profile.Enable();

        Assert.Equal(timestamp, profile.UpdatedAt);
    }

    [Fact]
    public void Constructor_SetsCreationAndUpdateTimestamps()
    {
        var before = DateTimeOffset.UtcNow;

        var profile = CreateValidProfile();

        var after = DateTimeOffset.UtcNow;

        Assert.InRange(profile.CreatedAt, before, after);
        Assert.Equal(profile.CreatedAt, profile.UpdatedAt);
    }
}
