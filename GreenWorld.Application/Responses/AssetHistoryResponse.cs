namespace GreenWorld.Application.Responses;

public sealed record ReadingDto(
    DateTimeOffset OccurredAt, double EnergyKwh, double PowerKw, string Direction);

public sealed record AssetHistoryResponse(
    Guid AssetId, int Count, IReadOnlyList<ReadingDto> Readings);
