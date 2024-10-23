using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData;
using StardewValley;
using Microsoft.Xna.Framework;
using BetterCraneGame.CraneGame;
using BetterCraneGame.DataModels;
using Microsoft.Xna.Framework.Graphics;

namespace BetterCraneGame;

internal class ModEntry : Mod {

    public static ModConfig Config = new ModConfig();
    public static ISpaceCoreApi SpaceCoreApi = null!;

    int playCost = 500;
    string currency = "coins";
    string currencyDisplayName = null!;
    bool virtualCurrency;
    public static int PlayTime = 2;
    public static int Credits = 3;
    public static float LeftChance = 0.1f;
    public static float RightChance = 0.2f;
    public static Texture2D CraneGameTexture = AssetManager.CraneGameTexture;
    public static string MusicCue = "crane_game";
    public static string FastMusicCue = "crane_game_fast";
    public static string PrizeDataKey = null!;

    public override void Entry(IModHelper helper) {
        Config = helper.ReadConfig<ModConfig>();

        Logger.Init(Monitor);

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.Content.AssetRequested += static (_, e) => AssetManager.OnAssetRequested(e);
        helper.Events.Content.AssetReady += static (_, e) => AssetManager.OnAssetReady(e);
        helper.Events.GameLoop.GameLaunched += static (_, e) => AssetManager.OnGameLaunched(e);

        GameLocation.RegisterTileAction("BetterCraneGame", HandleCraneGame);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) {
        SetupGMCM();
        SpaceCoreApi = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore")!;
    }

    private bool HandleCraneGame(GameLocation location, string[] args, Farmer farmer, Point point) {
        Helper.GameContent.InvalidateCache(AssetManager.CraneGameDataName);
        Helper.GameContent.InvalidateCache(AssetManager.PrizeDataName);
        AssetManager.PrizeData = Game1.content.Load<Dictionary<string, PrizeDataModel>>(AssetManager.PrizeDataName);
        AssetManager.CraneGameData = Game1.content.Load<Dictionary<string, CraneGameDataModel>>(AssetManager.CraneGameDataName);
        CraneGameTexture = null!;
        AssetManager.CraneGameData.TryGetValue(args[1], out CraneGameDataModel? data);

        if (data != null) {
            playCost = Math.Max(data.Cost, 0);
            currency = data.Currency ?? "coins";
            currencyDisplayName = data.CurrencyDisplayName ?? currency;
            PlayTime = data.PlayTime;
            Credits = data.Credits;
            LeftChance = data.LeftPrizeChance;
            RightChance = data.RightPrizeChance;
            CraneGameTexture = data.Texture != null ? Game1.content.Load<Texture2D>(data.Texture) : AssetManager.CraneGameTexture;
            MusicCue = data.MusicCue != null ? data.MusicCue : "crane_game";
            FastMusicCue = data.FastMusicCue != null ? data.FastMusicCue : "crane_game_fast";
            PrizeDataKey = data.PrizeDataEntry!;
        }
        virtualCurrency = currency == "coins" ? false : true;

        Logger.Log($"\nTexture: {CraneGameTexture.Name}");
        Logger.Log($"\nMusic cue: {MusicCue}");
        Logger.Log($"\nFast music cue: {FastMusicCue}");
        Logger.Log($"\nPlay cost: {playCost}");
        Logger.Log($"\nCurrency: {currency}");
        Logger.Log($"\nCurrency display name: {currencyDisplayName}");
        Logger.Log($"\nPlay time: {PlayTime}");
        Logger.Log($"\nCredits: {Credits}");
        Logger.Log($"\nPrize data key: {PrizeDataKey}");
        Logger.Log($"\nLeft hidden prize chance: {LeftChance}");
        Logger.Log($"\nRight hidden prize chance: {RightChance}\n");

        var funds = virtualCurrency ? SpaceCoreApi.GetVirtualCurrencyAmount(farmer, currency) : farmer.Money;
        bool canAfford = funds >= playCost;

        if (canAfford) {
            Game1.currentLocation.createQuestionDialogue(
                $"{Helper.Translation.Get("play.text1")} " +
                $"{funds} {currencyDisplayName}" +
                $"{Helper.Translation.Get("play.text2")} {playCost} {currencyDisplayName} " +
                $"{Helper.Translation.Get("play.text3")}\n{Helper.Translation.Get("play.text4")}",
                Game1.currentLocation.createYesNoResponses(),
                CraneGameAnswer
            );
        } else {
            Game1.drawObjectDialogue(
                $"{Helper.Translation.Get("play.text1")} " +
                $"{funds} {currencyDisplayName}" +
                $"{Helper.Translation.Get("play.text2")} {playCost} {currencyDisplayName} " +
                $"{Helper.Translation.Get("play.text3")}");
        }

        return true;
    }

    void CraneGameAnswer(Farmer who, string whichAnswer) {
        if (!(whichAnswer.ToLower() == "yes")) {
            return;
        }

        if (virtualCurrency) {
            SpaceCoreApi.AddToVirtualCurrency(Game1.player, currency, -playCost);
        } else {
            Game1.player.Money -= playCost;
        }
        StartCraneGame();
    }

    void StartCraneGame() {
        Game1.changeMusicTrack("none", track_interruptable: false, MusicContext.MiniGame);

        Game1.globalFadeToBlack(delegate {
            Game1.currentMinigame = new CustomCraneGame(CraneGameTexture);
        }, 0.008f);
    }

    void SetupGMCM() {
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

        if (configMenu is null) return;

        configMenu.Register(ModManifest, () => Config = new ModConfig(), () => Helper.WriteConfig(Config));

        configMenu.AddBoolOption(
            mod: ModManifest,
            getValue: () => Config.Logging,
            setValue: value => Config.Logging = value,
            name: () => Helper.Translation.Get("console-logging.name"),
            tooltip: () => $"{Helper.Translation.Get("console-logging.tooltip1")}\n{Helper.Translation.Get("console-logging.tooltip2")}"
        );
    }

}