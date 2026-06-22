public readonly struct ResultData
{
    public ResultData(
        float distance,
        float maxHeight,
        float flightTime,
        float rotations,
        int penaltyPoints,
        int finalScore)
    {
        Distance = distance;
        MaxHeight = maxHeight;
        FlightTime = flightTime;
        Rotations = rotations;
        PenaltyPoints = penaltyPoints;
        FinalScore = finalScore;
    }

    public float Distance { get; }
    public float MaxHeight { get; }
    public float FlightTime { get; }
    public float Rotations { get; }
    public int PenaltyPoints { get; }
    public int FinalScore { get; }
}
