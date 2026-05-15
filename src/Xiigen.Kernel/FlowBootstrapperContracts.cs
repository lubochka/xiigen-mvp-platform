namespace Xiigen.Kernel;

public sealed record FlowBootstrapResult(bool IsSuccess, string? ErrorCode = null, string? ErrorMessage = null);

public interface IFlowBootstrapper
{
    Task<FlowBootstrapResult> BootstrapAsync(
        string flowId,
        string mode,
        CancellationToken cancellationToken = default);
}
