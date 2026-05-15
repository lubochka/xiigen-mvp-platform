using Xiigen.Freedom;

namespace Xiigen.Freedom.Tests;

public sealed class FreedomConfigTests
{
    [Fact]
    public async Task GetAsync_Returns_FLOW01_Defaults()
    {
        var freedom = new InMemoryFreedomConfigService();

        var resend = await freedom.GetAsync("flow01_resend_rate_limit_minutes");
        var moduleName = await freedom.GetAsync("flow_module_name");

        Assert.True(resend.IsSuccess);
        Assert.Equal(60, resend.Data!.Value);
        Assert.Equal("user-registration", moduleName.Data!.Value);
    }
}
