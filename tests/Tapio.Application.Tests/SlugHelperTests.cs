using Tapio.Application.Common.Helpers;

namespace Tapio.Application.Tests;

public class SlugHelperTests
{
    [Fact]
    public void Generate_NormalizesTurkishCharacters()
    {
        var slug = SlugHelper.Generate("Cafe Mola");
        Assert.Equal("cafe-mola", slug);
    }

    [Fact]
    public void Generate_HandlesSpecialChars()
    {
        var slug = SlugHelper.Generate("Test & Co!!!");
        Assert.DoesNotContain("&", slug);
    }
}
