namespace BetterCraneGame.DataModels;

public class CraneGameDataModel {

    /// <summary>The asset name for the game's texture, or <c>null</c> for <c>rokugin.BCG/DefaultTexture</c>.</summary>
    public string? Texture { get; set; }
    
    /// <summary>The internal currency name.</summary>
    public string? Currency { get; set; }

    /// <summary>The cost to start the crane game.</summary>
    public int Cost { get; set; }

    /// <summary>A tokenizable string for the currencies translated display name.</summary>
    public string? CurrencyDisplayName { get; set; }

    /// <summary>The time in seconds that each round of play lasts.</summary>
    public int PlayTime { get; set; }

    /// <summary>The number of rounds of play.</summary>
    public int Credits { get; set; }

    /// <summary>Chance that the left hidden prize will spawn.</summary>
    public float LeftPrizeChance { get; set; }

    /// <summary>Chance that the right hidden prize will spawn.</summary>
    public float RightPrizeChance { get; set; }

}