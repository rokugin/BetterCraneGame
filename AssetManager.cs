using BetterCraneGame.DataModels;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace BetterCraneGame;

internal static class AssetManager {

    internal const string CraneGameTextureName = "rokugin.BCG/DefaultTexture";
    internal const string CraneGameDataName = "rokugin.BCG/CraneGame";
    internal const string PrizeDataName = "rokugin.BCG/PrizeData";

    internal static Texture2D CraneGameTexture = null!;
    internal static Dictionary<string, PrizeDataModel> PrizeData = new();
    internal static Dictionary<string, CraneGameDataModel> CraneGameData = new();

    internal static void OnAssetRequested(AssetRequestedEventArgs e) {
        if (e.NameWithoutLocale.IsEquivalentTo(CraneGameTextureName)) {
            e.LoadFromModFile<Texture2D>("assets/textures/CraneGame.png", AssetLoadPriority.Medium);
        }
        if (e.NameWithoutLocale.IsEquivalentTo(PrizeDataName)) {
            e.LoadFromModFile<Dictionary<string, PrizeDataModel>>("assets/data/PrizeData.json", AssetLoadPriority.Medium);
        }
        if (e.NameWithoutLocale.IsEquivalentTo(CraneGameDataName)) {
            e.LoadFromModFile<Dictionary<string, CraneGameDataModel>>("assets/data/CraneGameData.json", AssetLoadPriority.Medium);
        }
    }

    internal static void OnAssetReady(AssetReadyEventArgs e) {
        if (e.NameWithoutLocale.IsEquivalentTo(CraneGameTextureName)) {
            CraneGameTexture = Game1.content.Load<Texture2D>(CraneGameTextureName);
        }
        if (e.NameWithoutLocale.IsEquivalentTo(PrizeDataName)) {
            PrizeData = Game1.content.Load<Dictionary<string, PrizeDataModel>>(PrizeDataName);
        }
        if (e.NameWithoutLocale.IsEquivalentTo(CraneGameDataName)) {
            CraneGameData = Game1.content.Load<Dictionary<string, CraneGameDataModel>>(CraneGameDataName);
        }
    }

    internal static void OnSaveLoaded(SaveLoadedEventArgs e) {
        CraneGameTexture = Game1.content.Load<Texture2D>(CraneGameTextureName);
        PrizeData = Game1.content.Load<Dictionary<string, PrizeDataModel>>(PrizeDataName);
        CraneGameData = Game1.content.Load<Dictionary<string, CraneGameDataModel>>(CraneGameDataName);
    }

}