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

    int playCost = new();
    public string currency = null!;
    public string currencyDisplayName = null!;
    public bool virtualCurrency;
    public static int playTime = new();
    public static int credits = new();
    public static float leftChance = new();
    public static float rightChance = new();
    public static Texture2D CraneGameTexture = null!;

    public override void Entry(IModHelper helper) {
        Config = helper.ReadConfig<ModConfig>();

        Logger.Init(Monitor);

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.Content.AssetRequested += static (_, e) => AssetManager.OnAssetRequested(e);
        helper.Events.Content.AssetReady += static (_, e) => AssetManager.OnAssetReady(e);
        helper.Events.GameLoop.SaveLoaded += static (_, e) => AssetManager.OnSaveLoaded(e);

        GameLocation.RegisterTileAction("BetterCraneGame", HandleCraneGame);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) {
        SetupGMCM();
        SpaceCoreApi = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore")!;
    }

    private bool HandleCraneGame(GameLocation location, string[] args, Farmer farmer, Point point) {
        CraneGameTexture = null!;
        AssetManager.CraneGameData.TryGetValue(args[1], out CraneGameDataModel? data);

        if (data != null) {
            playCost = data.Cost;
            currency = data.Currency ?? "coins";
            currencyDisplayName = data.CurrencyDisplayName ?? currency;
            playTime = data.PlayTime;
            credits = data.Credits;
            leftChance = data.LeftPrizeChance;
            rightChance = data.RightPrizeChance;
            CraneGameTexture = Game1.content.Load<Texture2D>(data.Texture) ?? AssetManager.CraneGameTexture;
        } else {
            playCost = 500;
            currency = "coins";
            currencyDisplayName = currency;
            playTime = 20;
            credits = 3;
            leftChance = 0.1f;
            rightChance = 0.2f;
            CraneGameTexture = AssetManager.CraneGameTexture;
        }
        
        Math.Max(playCost, 0);
        Logger.Log($"\nPlay cost: {playCost}");
        virtualCurrency = currency == "coins" ? false : true;
        Logger.Log($"\nCurrency: {currency}");
        Logger.Log($"\nCurrency display name: {currencyDisplayName}");
        Logger.Log($"\nPlay time: {playTime}");
        Logger.Log($"\nCredits: {credits}");
        Logger.Log($"\nLeft hidden prize chance: {leftChance}");
        Logger.Log($"\nRight hidden prize chance: {rightChance}\n");

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