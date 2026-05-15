using Xiigen.Kernel;

namespace Xiigen.Fabrics.InMemory;

public sealed class InMemoryFlowBootstrapper : IFlowBootstrapper
{
    private readonly List<(string FlowId, string Mode)> _calls = [];

    public IReadOnlyList<(string FlowId, string Mode)> Calls => _calls;

    public Task<FlowBootstrapResult> BootstrapAsync(
        string flowId,
        string mode,
        CancellationToken cancellationToken = default)
    {
        _calls.Add((flowId, mode));
        return Task.FromResult(new FlowBootstrapResult(true));
    }
}
