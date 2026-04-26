namespace HQPRVTAI.Features.BeamLongitudinalSection
{
    internal sealed record BeamLongitudinalSectionResult(bool IsSuccess);

    internal sealed record BeamLongitudinalSectionResponse(IReadOnlyList<BeamLongitudinalSectionResult> Results)
    {
        public int SuccessCount => Results.Count(r => r.IsSuccess);
        public int FailCount => Results.Count(r => !r.IsSuccess);
    }
}
