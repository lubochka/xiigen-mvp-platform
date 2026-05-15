using Microsoft.Extensions.Logging.Abstractions;
using Xiigen.Kernel;

namespace Xiigen.Kernel.Tests;

public sealed class KernelPrimitiveTests
{
    [Fact]
    public void DataProcessResult_RecordsSuccessAndFailure()
    {
        var success = DataProcessResult<string>.Success("ok");
        var failure = DataProcessResult<string>.Failure("NOPE", "not ok");

        Assert.True(success.IsSuccess);
        Assert.Equal("ok", success.Data);
        Assert.False(failure.IsSuccess);
        Assert.Equal("NOPE", failure.Error?.Code);
    }

    [Fact]
    public void TenantContextAccessor_RestoresPriorContextAfterScope()
    {
        var accessor = new AsyncLocalTenantContextAccessor();
        var outer = new TenantContext("tenant-a", "user-a", new HashSet<string> { "tenant_admin" }, "corr-a", "test", DateTimeOffset.UtcNow);
        var inner = new TenantContext("tenant-b", "user-b", new HashSet<string> { "tenant_user" }, "corr-b", "test", DateTimeOffset.UtcNow);

        using (accessor.Push(outer))
        {
            Assert.Equal("tenant-a", accessor.Current?.TenantId);
            using (accessor.Push(inner))
            {
                Assert.Equal("tenant-b", accessor.Current?.TenantId);
            }

            Assert.Equal("tenant-a", accessor.Current?.TenantId);
        }

        Assert.Null(accessor.Current);
    }

    [Fact]
    public void MicroserviceBase_ExposesDescriptor()
    {
        var tenantAccessor = new AsyncLocalTenantContextAccessor();
        var correlationAccessor = new AsyncLocalCorrelationContextAccessor();
        var service = new TestService(tenantAccessor, correlationAccessor);

        Assert.Equal("FLOW-TEST", service.Descriptor.FlowId);
    }

    private sealed class TestService : MicroserviceBase<TestService>
    {
        public TestService(
            ITenantContextAccessor tenantContextAccessor,
            ICorrelationContextAccessor correlationContextAccessor)
            : base(
                new ServiceDescriptor("T-TEST", "TestService", "FLOW-TEST"),
                tenantContextAccessor,
                correlationContextAccessor,
                NullLogger<TestService>.Instance)
        {
        }
    }
}
