using Xiigen.Kernel;

namespace Xiigen.Platform.Api.DesignConversion;

public sealed record DesignConversionIdempotencyDecision(string OperationKey, IResult? ShortCircuitResult);

public static class DesignConversionIdempotency
{
    private static readonly TimeSpan ReservationTtl = TimeSpan.FromMinutes(30);

    public static async Task<DataProcessResult<DesignConversionIdempotencyDecision>> TryReserveSubmitAsync(
        IIdempotencyStore idempotencyStore,
        DesignConversionSubmitCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.IdempotencyKey))
        {
            return DataProcessResult<DesignConversionIdempotencyDecision>.Failure(
                "IDEMPOTENCY_KEY_REQUIRED",
                "X-Idempotency-Key or idempotencyKey is required.");
        }

        var operationKey = $"design-conversion:{command.TenantId}:{command.IdempotencyKey}";
        var reservation = await idempotencyStore.TryReserveAsync(operationKey, ReservationTtl, cancellationToken)
            .ConfigureAwait(false);
        if (reservation.IsFailure || reservation.Data is null)
        {
            return DataProcessResult<DesignConversionIdempotencyDecision>.Failure(
                reservation.Error?.Code ?? "IDEMPOTENCY_RESERVE_FAILED",
                reservation.Error?.Message ?? "Could not reserve idempotency key.");
        }

        if (string.Equals(reservation.Data.Status, "succeeded", StringComparison.Ordinal) &&
            reservation.Data.CachedResponse is not null)
        {
            return DataProcessResult<DesignConversionIdempotencyDecision>.Success(
                new DesignConversionIdempotencyDecision(operationKey, Results.Ok(reservation.Data.CachedResponse)));
        }

        if (!string.Equals(reservation.Data.Status, "reserved", StringComparison.Ordinal))
        {
            return DataProcessResult<DesignConversionIdempotencyDecision>.Success(
                new DesignConversionIdempotencyDecision(
                    operationKey,
                    Results.Conflict(new
                    {
                        code = "IDEMPOTENCY_KEY_CONFLICT",
                        message = "A prior request with this idempotency key is not replayable yet."
                    })));
        }

        return DataProcessResult<DesignConversionIdempotencyDecision>.Success(
            new DesignConversionIdempotencyDecision(operationKey, null));
    }

    public static Task<DataProcessResult> MarkSubmitSucceededAsync(
        IIdempotencyStore idempotencyStore,
        string operationKey,
        DesignConversionJobResponse response,
        CancellationToken cancellationToken = default) =>
        idempotencyStore.MarkSucceededAsync(operationKey, response.ToDictionary(), cancellationToken);
}
