public enum Actions
{
    StaySilent,
    GiveClue,
    Accuse,
    Vote,
    Defend
}

public class AIIntent
{
    public Actions ActionType { get; set; }
    public string TargetPlayerId { get; set; }
    public string Clue { get; set; }
    public string Reasoning { get; set; }
    public string PersonalityLabel { get; set; }

    public AIIntent()
    {
        ActionType = Actions.StaySilent;
        TargetPlayerId = string.Empty;
        Clue = string.Empty;
        Reasoning = string.Empty;
        PersonalityLabel = string.Empty;
    }
}