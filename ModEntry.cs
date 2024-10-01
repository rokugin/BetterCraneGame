using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData;
using StardewValley;
using Microsoft.Xna.Framework;
using CraneGameOverhaul.CraneGame;

namespace CraneGameOverhaul;

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

    public override void Entry(IModHelper helper) {
        Config = helper.ReadConfig<ModConfig>();

        Logger.Init(Monitor);
        AssetManager.Init(helper.GameContent);

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.Content.AssetRequested += static (_, e) => AssetManager.OnAssetRequested(e);
        helper.Events.Content.AssetsInvalidated += static (_, e) => AssetManager.Reset(e.NamesWithoutLocale);

        GameLocation.RegisterTileAction("rokugin.CGO", HandleCraneGame);

        helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e) {
        //SpaceCoreApi.AddToVirtualCurrency(Game1.player, "rokugin.cgotest_Token", 10);//initialize currency and add for testing
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) {
        SetupGMCM();
        SpaceCoreApi = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore")!;
    }

    private bool HandleCraneGame(GameLocation location, string[] args, Farmer farmer, Point point) {
        if (args.Length < 2 || !int.TryParse(args[1], out playCost)) {
            playCost = 500;
            Logger.Log("\nPrice not set or not an integer, using default.");
        }
        Math.Max(playCost, 0);
        Logger.Log($"\nPlay cost: {playCost}");

        if (args.Length < 3 || args[2] is null) {
            currency = "coins";
            Logger.Log("\nCurrency not set, using default.");
        } else {
            currency = args[2];
        }
        virtualCurrency = currency == "coins" ? false : true;
        Logger.Log($"\nCurrency: {currency}");

        if (args.Length < 4 || args[3] is null) {
            currencyDisplayName = "coins";
            Logger.Log("\nCurrency display name not set, using default.");
        } else {
            currencyDisplayName = args[3];
        }
        Logger.Log($"\nCurrency display name: {currencyDisplayName}");

        if (args.Length < 5 || !int.TryParse(args[4], out playTime)) {
            playTime = 20;
            Logger.Log("\nPlay time not set or not an integer, using default.");
        }
        Logger.Log($"\nPlay time: {playTime}");

        if (args.Length < 6 || !int.TryParse(args[5], out credits)) {
            credits = 3;
            Logger.Log("\nCredits was not set or not an integer, using default.");
        }
        Logger.Log($"\nCredits: {credits}");

        if (args.Length < 7 || !float.TryParse(args[6], out leftChance)) {
            leftChance = 0.1f;
            Logger.Log("\nLeft hidden prize chance not set or not a float, using default.");
        }
        Logger.Log($"\nLeft hidden prize chance: {leftChance}");

        if (args.Length < 8 || !float.TryParse(args[7], out rightChance)) {
            rightChance = 0.2f;
            Logger.Log("\nRight hidden prize chance not set or not a float, using default.");
        }
        Logger.Log($"\nRight hidden prize chance: {rightChance}\n");

        var funds = virtualCurrency ? SpaceCoreApi.GetVirtualCurrencyAmount(farmer, currency) : farmer.Money;
        bool canAfford = virtualCurrency ? SpaceCoreApi.GetVirtualCurrencyAmount(farmer, currency) >= playCost : farmer.Money >= playCost;

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
            Game1.currentMinigame = new CustomCraneGame();
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