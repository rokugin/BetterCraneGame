using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CraneGameOverhaul {
    internal static class AssetManager {

        internal const string ModPrefix = "CGO";

        static Lazy<Texture2D> craneGameTexture = new(() => Game1.content.Load<Texture2D>(craneGameSpriteSheet!.BaseName));
        static Lazy<Dictionary<string, PrizeDataModel>> prizeData = new(() =>
            Game1.content.Load<Dictionary<string, PrizeDataModel>>(prizeDataName!.BaseName));

        internal static Texture2D CraneGameTexture => craneGameTexture.Value;
        internal static Dictionary<string, PrizeDataModel> PrizeData => prizeData.Value;

        internal static IAssetName craneGameSpriteSheet { get; set; } = null!;
        internal static IAssetName prizeDataName { get; set; } = null!;

        internal static void Initialize(IGameContentHelper parser) {
            craneGameSpriteSheet = parser.ParseAssetName($"{ModPrefix}_CraneGame");
            prizeDataName = parser.ParseAssetName($"Data/{ModPrefix}_PrizeData");
        }

        internal static void OnAssetRequested(AssetRequestedEventArgs e) {
            if (e.Name.IsEquivalentTo(craneGameSpriteSheet)) {
                e.LoadFromModFile<Texture2D>("assets/textures/CraneGame.png", AssetLoadPriority.Exclusive);
            }
            if (e.Name.IsEquivalentTo(prizeDataName)) {
                e.LoadFromModFile<Dictionary<string, PrizeDataModel>>("assets/data/PrizeData.json", AssetLoadPriority.Medium);
            }
        }

        internal static void Reset(IReadOnlySet<IAssetName>? assets = null) {
            if ((assets is null || assets.Contains(craneGameSpriteSheet)) && craneGameTexture.IsValueCreated) {
                craneGameTexture = new(() => Game1.content.Load<Texture2D>(craneGameSpriteSheet.BaseName));
                prizeData = new(() => Game1.content.Load<Dictionary<string, PrizeDataModel>>(prizeDataName.BaseName));
            }
        }

    }
}
