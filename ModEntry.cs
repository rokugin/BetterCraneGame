using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData;
using StardewValley;
using SpaceCore.VanillaAssetExpansion;
using Microsoft.Xna.Framework;
using StardewValley.Menus;

namespace CraneGameOverhaul {
    public class ModEntry : Mod {

        public static IMonitor? SMonitor;
        public static IModHelper? SHelper;
        public static ModConfig Config = new ModConfig();
        //public static ISpaceCoreApi? SpaceCoreApi;

        int playCost = new();
        public string? currency;
        public string? currencyName;
        public bool virtualCurrency;
        public static int playTime = new();
        public static int credits = new();
        public static float leftChance = new();
        public static float rightChance = new();

        static LogLevel DesiredLogLevel => Config.Logging ? LogLevel.Debug : LogLevel.Trace;

        public override void Entry(IModHelper helper) {
            SMonitor = Monitor;
            SHelper = helper;
            Config = helper.ReadConfig<ModConfig>();

            AssetManager.Initialize(helper.GameContent);

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Input.ButtonsChanged += OnButtonsChanged;
            helper.Events.Content.AssetRequested += static (_, e) => AssetManager.OnAssetRequested(e);
            helper.Events.Content.AssetsInvalidated += static (_, e) => AssetManager.Reset(e.NamesWithoutLocale);

            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e) {
            Game1.player.AddVirtualCurrencyAmount("rokugin.test_Token", 10);//initialize currency and add for testing
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) {
            SetupGMCM();
            //SpaceCoreApi = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
        }

        private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e) {
            if (!Context.IsPlayerFree) return;

            if (e.Pressed.Any(button => button.IsActionButton())) {
                var tile = Game1.currentCursorTile;

                if (!Utility.tileWithinRadiusOfPlayer((int)tile.X, (int)tile.Y, 1, Game1.player)) {
                    tile = Game1.player.GetGrabTile();
                }

                string[] property = Game1.currentLocation.doesTileHavePropertyNoNull((int)tile.X, (int)tile.Y, "Action", "Buildings").Split(' ');
                if (property[0].ToLower() == "cranegameoverhaul") {
                    if (property.Length < 2 || !Int32.TryParse(property[1], out playCost)) {
                        playCost = 500;
                        Monitor.Log("\nPrice not set or not an int, using default price.", DesiredLogLevel);
                    }
                    Math.Max(playCost, 0);

                    if (property.Length < 3 || property[2] is null) {
                        currency = "coins";
                        virtualCurrency = false;
                        Monitor.Log("\nCurrency not set, using default currency.", DesiredLogLevel);
                    } else {
                        //if (SpaceCoreApi != null && SpaceCoreApi.GetVirtualCurrencyList().Count > 0) {
                        //    Monitor.Log($"Check virtual currencies for {property[2]}", Config.Logging ? LogLevel.Debug : LogLevel.Trace);
                        //    if (SpaceCoreApi.GetVirtualCurrencyList().Contains(property[2])) {
                        currency = property[2];
                        //        virtualCurrency = true;
                        //        Monitor.Log("\nVirtual currency found, using virtual currency.",
                        //    Config.Logging ? LogLevel.Debug : LogLevel.Trace);
                        //    } else {
                        //        currency = "coins";
                        //        virtualCurrency = false;
                        //        Monitor.Log("\nVirtual currency not found, using default currency.",
                        //    Config.Logging ? LogLevel.Debug : LogLevel.Trace);
                        //    }
                        //} else {
                        //    currency = "coins";
                        //    virtualCurrency = false;
                        //    Monitor.Log("\nSpaceCore API null or no virtual currencies, using default currency.",
                        //Config.Logging ? LogLevel.Debug : LogLevel.Trace);
                        //}
                        virtualCurrency = currency == "coins" ? false : true;
                    }

                    if (property.Length < 4 || property[3] is null) {
                        currencyName = "coins";
                    } else {
                        currencyName = property[3];
                    }

                    if (property.Length < 5 || !Int32.TryParse(property[4], out playTime)) {
                        playTime = 20;
                        Monitor.Log("\nPlay time not set or not an int, using default play time.", DesiredLogLevel);
                    }

                    if (property.Length < 6 || !Int32.TryParse(property[5], out credits)) {
                        credits = 3;
                        Monitor.Log("\nCredits was not set or not an int, using default credits.", DesiredLogLevel);
                    }

                    if (property.Length < 7 || !float.TryParse(property[6], out leftChance)) {
                        leftChance = 0.1f;
                        Monitor.Log("\nLeft hidden prize chance not set or not a float, using default chance.", DesiredLogLevel);
                    }

                    if (property.Length < 8 || !float.TryParse(property[7], out rightChance)) {
                        rightChance = 0.2f;
                        Monitor.Log("\nRight hidden prize chance not set or not a float, using default chance.", DesiredLogLevel);
                    }

                    if (virtualCurrency) {
                        if (Game1.player.GetVirtualCurrencyAmount(currency) >= playCost) {
                            Game1.currentLocation.createQuestionDialogue(
                            $"{Helper.Translation.Get("play.text1")} " +
                            $"{Game1.player.GetVirtualCurrencyAmount(currency)} {currencyName}" +
                            $"{Helper.Translation.Get("play.text2")} {playCost} {currencyName} " +
                            $"{Helper.Translation.Get("play.text3")}\n{Helper.Translation.Get("play.text4")}",
                            Game1.currentLocation.createYesNoResponses(),
                            CraneGameAnswer);
                        } else {
                            Game1.drawObjectDialogue(
                                $"{Helper.Translation.Get("play.text1")} " +
                                $"{Game1.player.GetVirtualCurrencyAmount(currency)} {currencyName}" +
                                $"{Helper.Translation.Get("play.text2")} {playCost} {currencyName} " +
                                $"{Helper.Translation.Get("play.text3")}");
                        }
                    } else {
                        Game1.currentLocation.createQuestionDialogue(
                            $"{Helper.Translation.Get("play.text1")} {Game1.player.Money} {currencyName}" +
                            $"{Helper.Translation.Get("play.text2")} {playCost} {currencyName} " +
                            $"{Helper.Translation.Get("play.text3")}\n{Helper.Translation.Get("play.text4")}",
                            Game1.currentLocation.createYesNoResponses(),
                            CraneGameAnswer);
                    }
                }
            }
        }

        void CraneGameAnswer(Farmer who, string whichAnswer) {
            if (!(whichAnswer.ToLower() == "yes")) {
                return;
            }

            if (virtualCurrency) {
                Game1.player.AddVirtualCurrencyAmount(currency!, -playCost);
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
}