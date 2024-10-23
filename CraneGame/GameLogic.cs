using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using StardewValley;
using System.Xml.Serialization;
using StardewValley.Extensions;
using BetterCraneGame.DataModels;
using Microsoft.Xna.Framework.Input;

namespace BetterCraneGame.CraneGame;

public class GameLogic : CraneGameObject {
    [XmlType("CraneGame.GameStates")]
    public enum GameStates {
        Setup,
        Idle,
        MoveClawRight,
        WaitForMoveDown,
        MoveClawDown,
        ClawDescend,
        ClawAscend,
        ClawReturn,
        ClawRelease,
        ClawReset,
        EndGame
    }

    public string musicCue => Game1.soundBank.Exists(ModEntry.MusicCue) ? ModEntry.MusicCue : "crane_game";
    public string fastMusicCue => Game1.soundBank.Exists(ModEntry.FastMusicCue) ? ModEntry.FastMusicCue : "crane_game_fast";

    public List<Item> collectedItems;

    public const int CLAW_HEIGHT = 50;

    protected Claw _claw;

    public int maxLives = ModEntry.Credits;

    public int lives = ModEntry.Credits;

    public Vector2 _startPosition = new Vector2(24f, 56f);

    public Vector2 _dropPosition = new Vector2(32f, 56f);

    public Rectangle playArea = new Rectangle(16, 48, 272, 64);

    public Rectangle prizeChute = new Rectangle(16, 48, 32, 32);

    protected GameStates _currentState;

    protected int _stateTimer;

    public CraneGameObject moveRightIndicator;

    public CraneGameObject moveDownIndicator;

    public CraneGameObject creditsDisplay;

    public CraneGameObject timeDisplay1;

    public CraneGameObject timeDisplay2;

    public CraneGameObject sunShockedFace;

    public int currentTimer;

    public CraneGameObject joystick;

    public int[] conveyorBeltTiles = new int[68]
    {
                0, 0, 0, 0, 7, 6, 6, 9, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 8, 0, 0, 2, 0, 0, 0, 7, 6, 6, 6, 6, 9,
                0, 0, 0, 0, 8, 0, 0, 2, 0, 0, 0, 8, 0, 0, 0, 0, 2,
                0, 0, 0, 0, 1, 4, 4, 3, 0, 0, 0, 1, 4, 4, 4, 4, 3
    };

    public int[] prizeMap = new int[68]
    {
                0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 2,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 2, 0, 3
    };

    public GameLogic(CustomCraneGame game)
        : base(game) {
        Game1.playSound(musicCue, out base._game.music);
        base._game.fastMusic = Game1.soundBank.GetCue(fastMusicCue);
        this._claw = new Claw(base._game);
        this._claw.position = this._startPosition;
        this._claw.zPosition = 50f;
        this.collectedItems = new List<Item>();
        this.SetState(GameStates.Setup);
        new Bush(base._game, 55, 2, 3, 31, 111);
        new Bush(base._game, 45, 2, 2, 112, 84);
        new Bush(base._game, 45, 2, 2, 63, 63);
        new Bush(base._game, 48, 1, 2, 56, 80);
        new Bush(base._game, 48, 1, 2, 72, 80);
        new Bush(base._game, 48, 1, 2, 56, 96);
        new Bush(base._game, 48, 1, 2, 72, 96);
        new Bush(base._game, 48, 1, 2, 56, 112);
        new Bush(base._game, 48, 1, 2, 72, 112);
        new Bush(base._game, 45, 2, 2, 159, 63);
        new Bush(base._game, 48, 1, 2, 152, 80);
        new Bush(base._game, 48, 1, 2, 168, 80);
        new Bush(base._game, 48, 1, 2, 152, 96);
        new Bush(base._game, 48, 1, 2, 168, 96);
        new Bush(base._game, 48, 1, 2, 152, 112);
        new Bush(base._game, 48, 1, 2, 168, 112);
        this.sunShockedFace = new CraneGameObject(base._game);
        this.sunShockedFace.SetSpriteFromIndex(9);
        this.sunShockedFace.position = new Vector2(96f, 0f);
        this.sunShockedFace.spriteAnchor = Vector2.Zero;
        CraneGameObject craneGameObject = new CraneGameObject(base._game);
        craneGameObject.position.X = 16f;
        craneGameObject.position.Y = 87f;
        craneGameObject.SetSpriteFromIndex(3);
        craneGameObject.spriteRect.Width = 32;
        craneGameObject.spriteAnchor = new Vector2(0f, 15f);
        this.joystick = new CraneGameObject(base._game);
        this.joystick.position.X = 151f;
        this.joystick.position.Y = 134f;
        this.joystick.SetSpriteFromIndex(28);
        this.joystick.spriteRect.Width = 32;
        this.joystick.spriteRect.Height = 48;
        this.joystick.spriteAnchor = new Vector2(15f, 47f);
        this.lives = this.maxLives;
        this.moveRightIndicator = new CraneGameObject(base._game);
        this.moveRightIndicator.position.X = 21f;
        this.moveRightIndicator.position.Y = 126f;
        this.moveRightIndicator.SetSpriteFromIndex(26);
        this.moveRightIndicator.spriteAnchor = Vector2.Zero;
        this.moveRightIndicator.visible = false;
        this.moveDownIndicator = new CraneGameObject(base._game);
        this.moveDownIndicator.position.X = 49f;
        this.moveDownIndicator.position.Y = 126f;
        this.moveDownIndicator.SetSpriteFromIndex(27);
        this.moveDownIndicator.spriteAnchor = Vector2.Zero;
        this.moveDownIndicator.visible = false;
        this.creditsDisplay = new CraneGameObject(base._game);
        this.creditsDisplay.SetSpriteFromIndex(70);
        this.creditsDisplay.position = new Vector2(234f, 125f);
        this.creditsDisplay.spriteAnchor = Vector2.Zero;
        this.timeDisplay1 = new CraneGameObject(base._game);
        this.timeDisplay1.SetSpriteFromIndex(70);
        this.timeDisplay1.position = new Vector2(274f, 125f);
        this.timeDisplay1.spriteAnchor = Vector2.Zero;
        this.timeDisplay2 = new CraneGameObject(base._game);
        this.timeDisplay2.SetSpriteFromIndex(70);
        this.timeDisplay2.position = new Vector2(285f, 125f);
        this.timeDisplay2.spriteAnchor = Vector2.Zero;
        int level_width = 17;
        for (int i = 0; i < this.conveyorBeltTiles.Length; i++) {
            if (this.conveyorBeltTiles[i] != 0) {
                int x = i % level_width + 1;
                int y = i / level_width + 3;
                switch (this.conveyorBeltTiles[i]) {
                    case 8:
                        new ConveyorBelt(base._game, x, y, 0);
                        break;
                    case 4:
                        new ConveyorBelt(base._game, x, y, 3);
                        break;
                    case 6:
                        new ConveyorBelt(base._game, x, y, 1);
                        break;
                    case 2:
                        new ConveyorBelt(base._game, x, y, 2);
                        break;
                    case 7:
                        new ConveyorBelt(base._game, x, y, 1).SetSpriteFromCorner(240, 272);
                        break;
                    case 9:
                        new ConveyorBelt(base._game, x, y, 2).SetSpriteFromCorner(240, 240);
                        break;
                    case 1:
                        new ConveyorBelt(base._game, x, y, 0).SetSpriteFromCorner(240, 224);
                        break;
                    case 3:
                        new ConveyorBelt(base._game, x, y, 3).SetSpriteFromCorner(240, 256);
                        break;
                }
            }
        }

        // Set up items list
        Dictionary<int, List<Item>> possible_items = new Dictionary<int, List<Item>>();
        Dictionary<string, PrizeDataModel> prizeData = AssetManager.PrizeData;
        bool fullPrizeList = ModEntry.PrizeDataKey == null;
        List<Item> item_list = new();

        if (fullPrizeList) {
            foreach (var key in prizeData.Keys) {
                if (prizeData.TryGetValue(key, out PrizeDataModel? data)) {
                    if (data.CommonList != null && data.CommonList.Prizes != null) {
                        foreach (ItemField item in data.CommonList.Prizes) {
                            int amount = item.MaxStack > item.MinStack ? Game1.random.Next(item.MinStack, item.MaxStack + 1) : item.MinStack;
                            if (item.Condition != null) {
                                if (!GameStateQuery.CheckConditions(item.Condition, Game1.currentLocation, Game1.player, null, null, null, null)) {
                                    continue;
                                }
                            }
                            item_list.Add(ItemRegistry.Create(item.ItemId, amount, item.Quality));
                        }
                    }
                }
            }
        } else {
            if (prizeData.TryGetValue(ModEntry.PrizeDataKey!, out PrizeDataModel? data)) {
                if (data.CommonList != null && data.CommonList.Prizes != null) {
                    foreach (ItemField item in data.CommonList.Prizes) {
                        int amount = item.MaxStack > item.MinStack ? Game1.random.Next(item.MinStack, item.MaxStack + 1) : item.MinStack;
                        if (item.Condition != null) {
                            if (!GameStateQuery.CheckConditions(item.Condition, Game1.currentLocation, Game1.player, null, null, null, null)) {
                                continue;
                            }
                        }
                        item_list.Add(ItemRegistry.Create(item.ItemId, amount, item.Quality));
                    }
                }
            }
        }
        possible_items[1] = item_list;

        item_list = new();
        if (fullPrizeList) {
            foreach (var key in prizeData.Keys) {
                if (prizeData.TryGetValue(key, out PrizeDataModel? data)) {
                    if (data.RareList != null && data.RareList.Prizes != null) {
                        foreach (ItemField item in data.RareList.Prizes) {
                            int amount = item.MaxStack > item.MinStack ? Game1.random.Next(item.MinStack, item.MaxStack + 1) : item.MinStack;
                            if (item.Condition != null) {
                                if (!GameStateQuery.CheckConditions(item.Condition, Game1.currentLocation, Game1.player, null, null, null, null)) {
                                    continue;
                                }
                            }
                            item_list.Add(ItemRegistry.Create(item.ItemId, amount, item.Quality));
                        }
                    }
                }
            }
        } else {
            if (prizeData.TryGetValue(ModEntry.PrizeDataKey!, out PrizeDataModel? data)) {
                if (data.RareList != null && data.RareList.Prizes != null) {
                    foreach (ItemField item in data.RareList.Prizes) {
                        int amount = item.MaxStack > item.MinStack ? Game1.random.Next(item.MinStack, item.MaxStack + 1) : item.MinStack;
                        if (item.Condition != null) {
                            if (!GameStateQuery.CheckConditions(item.Condition, Game1.currentLocation, Game1.player, null, null, null, null)) {
                                continue;
                            }
                        }
                        item_list.Add(ItemRegistry.Create(item.ItemId, amount, item.Quality));
                    }
                }
            }
        }
        possible_items[2] = item_list;

        item_list = new();
        if (fullPrizeList) {
            foreach (var key in prizeData.Keys) {
                if (prizeData.TryGetValue(key, out PrizeDataModel? data)) {
                    if (data.DeluxeList != null && data.DeluxeList.Prizes != null) {
                        foreach (ItemField item in data.DeluxeList.Prizes) {
                            int amount = item.MaxStack > item.MinStack ? Game1.random.Next(item.MinStack, item.MaxStack + 1) : item.MinStack;
                            if (item.Condition != null) {
                                if (!GameStateQuery.CheckConditions(item.Condition, Game1.currentLocation, Game1.player, null, null, null, null)) {
                                    continue;
                                }
                            }
                            item_list.Add(ItemRegistry.Create(item.ItemId, amount, item.Quality));
                        }
                    }
                }
            }
        } else {
            if (prizeData.TryGetValue(ModEntry.PrizeDataKey!, out PrizeDataModel? data)) {
                if (data.DeluxeList != null && data.DeluxeList.Prizes != null) {
                    foreach (ItemField item in data.DeluxeList.Prizes) {
                        int amount = item.MaxStack > item.MinStack ? Game1.random.Next(item.MinStack, item.MaxStack + 1) : item.MinStack;
                        if (item.Condition != null) {
                            if (!GameStateQuery.CheckConditions(item.Condition, Game1.currentLocation, Game1.player, null, null, null, null)) {
                                continue;
                            }
                        }
                        item_list.Add(ItemRegistry.Create(item.ItemId, amount, item.Quality));
                    }
                }
            }
        }
        possible_items[3] = item_list;

        // Choose items from list
        for (int i = 0; i < this.prizeMap.Length; i++) {
            if (this.prizeMap[i] == 0) {
                continue;
            }
            int x = i % level_width + 1;
            int y = i / level_width + 3;
            Item? item = null;
            int prize_rarity = i;
            while (prize_rarity > 0 && item == null) {
                int index = this.prizeMap[i];
                if ((uint)(index - 1) <= 2u) {
                    item = Game1.random.ChooseFrom(possible_items[index]);
                }
                prize_rarity--;
            }

            if (item is null) continue;

            new Prize(base._game, item) { position = { X = x * 16 + 8, Y = y * 16 + 8 } };
        }

        // Secret items spawn
        Item? secretItemLeft = null;
        Item? secretItemRight = null;
        Vector2 offset = new Vector2(0f, 4f);
        Vector2 prizePositionLeft = new();
        Vector2 prizePositionRight = new();

        if (Game1.random.NextDouble() < ModEntry.LeftChance) {
            item_list = new();
            if (fullPrizeList) {
                foreach (var key in prizeData.Keys) {
                    if (prizeData.TryGetValue(key, out PrizeDataModel? data)) {
                        if (data.LeftSecretPrizes != null) {
                            foreach (ItemField item in data.LeftSecretPrizes) {
                                int amount = item.MaxStack > item.MinStack ? Game1.random.Next(item.MinStack, item.MaxStack + 1) : item.MinStack;
                                if (item.Condition != null) {
                                    if (!GameStateQuery.CheckConditions(item.Condition, Game1.currentLocation, Game1.player, null, null, null, null)) {
                                        continue;
                                    }
                                }
                                item_list.Add(ItemRegistry.Create(item.ItemId, amount, item.Quality));
                            }
                        }
                    }
                }
            } else {
                if (prizeData.TryGetValue(ModEntry.PrizeDataKey!, out PrizeDataModel? data)) {
                    if (data.LeftSecretPrizes != null) {
                        foreach (ItemField item in data.LeftSecretPrizes) {
                            int amount = item.MaxStack > item.MinStack ? Game1.random.Next(item.MinStack, item.MaxStack + 1) : item.MinStack;
                            if (item.Condition != null) {
                                if (!GameStateQuery.CheckConditions(item.Condition, Game1.currentLocation, Game1.player, null, null, null, null)) {
                                    continue;
                                }
                            }
                            item_list.Add(ItemRegistry.Create(item.ItemId, amount, item.Quality));
                        }
                    }
                }
            }
            secretItemLeft = Game1.random.ChooseFrom(item_list);
            prizePositionLeft = new Vector2(offset.X * 16f + 30f, offset.Y * 16f + 32f); // in front of the prize bucket, behind the large bush
        }
        if (Game1.random.NextDouble() < ModEntry.RightChance) {
            item_list = new();
            if (fullPrizeList) {
                foreach (var key in prizeData.Keys) {
                    if (prizeData.TryGetValue(key, out PrizeDataModel? data)) {
                        if (data.RightSecretPrizes != null) {
                            foreach (ItemField item in data.RightSecretPrizes) {
                                int amount = item.MaxStack > item.MinStack ? Game1.random.Next(item.MinStack, item.MaxStack + 1) : item.MinStack;
                                if (item.Condition != null) {
                                    if (!GameStateQuery.CheckConditions(item.Condition, Game1.currentLocation, Game1.player, null, null, null, null)) {
                                        continue;
                                    }
                                }
                                item_list.Add(ItemRegistry.Create(item.ItemId, amount, item.Quality));
                            }
                        }
                    }
                }
            } else {
                if (prizeData.TryGetValue(ModEntry.PrizeDataKey!, out PrizeDataModel? data)) {
                    if (data.RightSecretPrizes != null) {
                        foreach (ItemField item in data.RightSecretPrizes) {
                            int amount = item.MaxStack > item.MinStack ? Game1.random.Next(item.MinStack, item.MaxStack + 1) : item.MinStack;
                            if (item.Condition != null) {
                                if (!GameStateQuery.CheckConditions(item.Condition, Game1.currentLocation, Game1.player, null, null, null, null)) {
                                    continue;
                                }
                            }
                            item_list.Add(ItemRegistry.Create(item.ItemId, amount, item.Quality));
                        }
                    }
                }
            }
            secretItemRight = Game1.random.ChooseFrom(item_list);
            prizePositionRight = new Vector2(160f, 58f); // middle divider area, behind medium bush
        }
        if (secretItemLeft != null) {
            new Prize(base._game, secretItemLeft) { position = prizePositionLeft };
        }
        if (secretItemRight != null) {
            new Prize(_game, secretItemRight) { position = prizePositionRight };
        }

        //Add items as set dressing
        Item? decorationItem;

        item_list = new();
        if (fullPrizeList) {
            foreach (var key in prizeData.Keys) {
                if (prizeData.TryGetValue(key, out PrizeDataModel? data)) {
                    if (data.LeftDecorationPrizes != null) {
                        foreach (ItemField item in data.LeftDecorationPrizes) {
                            int amount = item.MaxStack > item.MinStack ? Game1.random.Next(item.MinStack, item.MaxStack + 1) : item.MinStack;
                            if (item.Condition != null) {
                                if (!GameStateQuery.CheckConditions(item.Condition, Game1.currentLocation, Game1.player, null, null, null, null)) {
                                    continue;
                                }
                            }
                            item_list.Add(ItemRegistry.Create(item.ItemId, amount, item.Quality));
                        }
                    }
                }
            }
        } else {
            if (prizeData.TryGetValue(ModEntry.PrizeDataKey!, out PrizeDataModel? data)) {
                if (data.LeftDecorationPrizes != null) {
                    foreach (ItemField item in data.LeftDecorationPrizes) {
                        int amount = item.MaxStack > item.MinStack ? Game1.random.Next(item.MinStack, item.MaxStack + 1) : item.MinStack;
                        if (item.Condition != null) {
                            if (!GameStateQuery.CheckConditions(item.Condition, Game1.currentLocation, Game1.player, null, null, null, null)) {
                                continue;
                            }
                        }
                        item_list.Add(ItemRegistry.Create(item.ItemId, amount, item.Quality));
                    }
                }
            }
        }
        decorationItem = Game1.random.ChooseFrom(item_list);
        // above the right conveyor, left side
        if (decorationItem != null) {
            new Prize(_game, decorationItem) { position = new Vector2(215f, 56f), zPosition = 0f };
        }

        item_list = new();
        if (fullPrizeList) {
            foreach (var key in prizeData.Keys) {
                if (prizeData.TryGetValue(key, out PrizeDataModel? data)) {
                    if (data.RightDecorationPrizes != null) {
                        foreach (ItemField item in data.RightDecorationPrizes) {
                            int amount = item.MaxStack > item.MinStack ? Game1.random.Next(item.MinStack, item.MaxStack + 1) : item.MinStack;
                            if (item.Condition != null) {
                                if (!GameStateQuery.CheckConditions(item.Condition, Game1.currentLocation, Game1.player, null, null, null, null)) {
                                    continue;
                                }
                            }
                            item_list.Add(ItemRegistry.Create(item.ItemId, amount, item.Quality));
                        }
                    }
                }
            }
        } else {
            if (prizeData.TryGetValue(ModEntry.PrizeDataKey!, out PrizeDataModel? data)) {
                if (data.RightDecorationPrizes != null) {
                    foreach (ItemField item in data.RightDecorationPrizes) {
                        int amount = item.MaxStack > item.MinStack ? Game1.random.Next(item.MinStack, item.MaxStack + 1) : item.MinStack;
                        if (item.Condition != null) {
                            if (!GameStateQuery.CheckConditions(item.Condition, Game1.currentLocation, Game1.player, null, null, null, null)) {
                                continue;
                            }
                        }
                        item_list.Add(ItemRegistry.Create(item.ItemId, amount, item.Quality));
                    }
                }
            }
        }
        decorationItem = Game1.random.ChooseFrom(item_list);
        // above the right conveyor, right side
        if (decorationItem != null) {
            new Prize(_game, decorationItem) { position = new Vector2(263f, 56f), zPosition = 0f };
        }
    }

    public GameStates GetCurrentState() {
        return this._currentState;
    }

    public override void Update(GameTime time) {
        float desired_joystick_rotation = 0f;
        foreach (Shadow shadow in base._game.GetObjectsOfType<Shadow>()) {
            if (this.prizeChute.Contains(new Point((int)shadow.position.X, (int)shadow.position.Y))) {
                shadow.visible = false;
            } else {
                shadow.visible = true;
            }
        }
        int displayed_time = this.currentTimer / 60;
        if (this._currentState == GameStates.Setup) {
            this.creditsDisplay.SetSpriteFromIndex(70);
        } else {
            this.creditsDisplay.SetSpriteFromIndex(70 + this.lives);
        }
        this.timeDisplay1.SetSpriteFromIndex(70 + displayed_time / 10);
        this.timeDisplay2.SetSpriteFromIndex(70 + displayed_time % 10);
        if (this.currentTimer < 0) {
            this.timeDisplay1.SetSpriteFromIndex(80);
            this.timeDisplay2.SetSpriteFromIndex(81);
        }
        switch (this._currentState) {
            case GameStates.Setup: {
                    if (!base._game.music!.IsPlaying) {
                        base._game.music.Play();
                    }
                    this._claw.openAngle = 40f;
                    bool is_something_busy = false;
                    foreach (Prize item2 in base._game.GetObjectsOfType<Prize>()) {
                        if (!item2.CanBeGrabbed()) {
                            is_something_busy = true;
                            break;
                        }
                    }
                    if (!is_something_busy) {
                        if (this._stateTimer >= 10) {
                            this.SetState(GameStates.Idle);
                        }
                    } else {
                        this._stateTimer = 0;
                    }
                    break;
                }
            case GameStates.Idle:
                if (!base._game.music!.IsPlaying) {
                    base._game.music.Play();
                }
                if (base._game.fastMusic!.IsPlaying) {
                    base._game.fastMusic.Stop(AudioStopOptions.Immediate);
                    base._game.fastMusic = Game1.soundBank.GetCue(fastMusicCue);
                }
                this.currentTimer = ModEntry.PlayTime * 60;
                this.moveRightIndicator.visible = Game1.ticks / 20 % 2 == 0;
                if (base._game.IsButtonPressed(GameButtons.Tool) || base._game.IsButtonPressed(GameButtons.Action) || base._game.IsButtonPressed(GameButtons.Right)) {
                    Game1.playSound("bigSelect");
                    this.SetState(GameStates.MoveClawRight);
                }
                break;
            case GameStates.MoveClawRight:
                desired_joystick_rotation = 15f;
                if (this._stateTimer < 15) {
                    if (!base._game.IsButtonDown(GameButtons.Tool) && !base._game.IsButtonDown(GameButtons.Action) && !base._game.IsButtonDown(GameButtons.Right)) {
                        Game1.playSound("bigDeSelect");
                        this.SetState(GameStates.Idle);
                        return;
                    }
                    break;
                }
                if (base._game.craneSound == null || !base._game.craneSound.IsPlaying) {
                    Game1.playSound("crane", out base._game.craneSound);
                }
                this.currentTimer--;
                if (this.currentTimer <= 0) {
                    this.SetState(GameStates.ClawDescend);
                    this.currentTimer = -1;
                    if (base._game.craneSound != null && !base._game.craneSound.IsStopped) {
                        base._game.craneSound.Stop(AudioStopOptions.Immediate);
                    }
                }
                this.moveRightIndicator.visible = true;
                if (this._stateTimer <= 10) {
                    break;
                }
                if (this._stateTimer == 11) {
                    this._claw.ApplyDrawEffect(new ShakeEffect(1f, 1f));
                    this._claw.ApplyDrawEffect(new SwayEffect(2f, 10f, 20));
                    this._claw.ApplyDrawEffectToArms(new SwayEffect(15f, 4f, 50));
                }
                if (!base._game.IsButtonDown(GameButtons.Tool) && !base._game.IsButtonDown(GameButtons.Right) && !base._game.IsButtonDown(GameButtons.Action)) {
                    Game1.playSound("bigDeSelect");
                    this._claw.ApplyDrawEffect(new SwayEffect(2f, 10f, 20));
                    this._claw.ApplyDrawEffectToArms(new SwayEffect(15f, 4f, 100));
                    this.SetState(GameStates.WaitForMoveDown);
                    this.moveRightIndicator.visible = false;
                    if (base._game.craneSound != null && !base._game.craneSound.IsStopped) {
                        base._game.craneSound.Stop(AudioStopOptions.Immediate);
                    }
                } else {
                    this._claw.Move(0.5f, 0f);
                    if (this._claw.GetBounds().Right >= this.playArea.Right) {
                        this._claw.Move(-0.5f, 0f);
                    }
                }
                break;
            case GameStates.WaitForMoveDown:
                this.currentTimer--;
                if (this.currentTimer <= 0) {
                    this.SetState(GameStates.ClawDescend);
                    this.currentTimer = -1;
                }
                this.moveDownIndicator.visible = Game1.ticks / 20 % 2 == 0;
                if (base._game.IsButtonPressed(GameButtons.Tool) || base._game.IsButtonPressed(GameButtons.Down) || base._game.IsButtonPressed(GameButtons.Action)) {
                    Game1.playSound("bigSelect");
                    this.SetState(GameStates.MoveClawDown);
                }
                break;
            case GameStates.MoveClawDown:
                if (base._game.craneSound == null || !base._game.craneSound.IsPlaying) {
                    Game1.playSound("crane", out base._game.craneSound);
                }
                this.currentTimer--;
                if (this.currentTimer <= 0) {
                    this.SetState(GameStates.ClawDescend);
                    this.currentTimer = -1;
                    if (base._game.craneSound != null && !base._game.craneSound.IsStopped) {
                        base._game.craneSound.Stop(AudioStopOptions.Immediate);
                    }
                }
                desired_joystick_rotation = -5f;
                this.moveDownIndicator.visible = true;
                if (this._stateTimer <= 10) {
                    break;
                }
                if (this._stateTimer == 11) {
                    this._claw.ApplyDrawEffect(new ShakeEffect(1f, 1f));
                    this._claw.ApplyDrawEffect(new SwayEffect(2f, 10f, 20));
                    this._claw.ApplyDrawEffectToArms(new SwayEffect(15f, 4f, 50));
                }
                if (!base._game.IsButtonDown(GameButtons.Tool) && !base._game.IsButtonDown(GameButtons.Down) && !base._game.IsButtonDown(GameButtons.Action)) {
                    Game1.playSound("bigDeSelect");
                    this._claw.ApplyDrawEffect(new SwayEffect(2f, 10f, 20));
                    this._claw.ApplyDrawEffectToArms(new SwayEffect(15f, 4f, 100));
                    this.moveDownIndicator.visible = false;
                    this.SetState(GameStates.ClawDescend);
                    if (base._game.craneSound != null && !base._game.craneSound.IsStopped) {
                        base._game.craneSound.Stop(AudioStopOptions.Immediate);
                    }
                } else {
                    this._claw.Move(0f, 0.5f);
                    if (this._claw.GetBounds().Bottom >= this.playArea.Bottom) {
                        this._claw.Move(0f, -0.5f);
                    }
                }
                break;
            case GameStates.ClawDescend:
                if (this._claw.openAngle < 40f) {
                    this._claw.openAngle += 1.5f;
                    this._stateTimer = 0;
                } else {
                    if (this._stateTimer <= 30) {
                        break;
                    }
                    if (base._game.craneSound != null && base._game.craneSound.IsPlaying) {
                        Game1.sounds.SetPitch(base._game.craneSound, 2000f);
                    } else {
                        Game1.playSound("crane", 2000, out base._game.craneSound);
                    }
                    if (!(this._claw.zPosition > 0f)) {
                        break;
                    }
                    this._claw.zPosition -= 0.5f;
                    if (this._claw.zPosition <= 0f) {
                        this._claw.zPosition = 0f;
                        this.SetState(GameStates.ClawAscend);
                        if (base._game.craneSound != null && !base._game.craneSound.IsStopped) {
                            base._game.craneSound.Stop(AudioStopOptions.Immediate);
                        }
                    }
                }
                break;
            case GameStates.ClawAscend:
                if (this._claw.openAngle > 0f && this._claw.GetGrabbedPrize() == null) {
                    this._claw.openAngle -= 1f;
                    if (this._claw.openAngle == 15f) {
                        this._claw.GrabObject();
                        if (this._claw.GetGrabbedPrize() != null) {
                            Game1.playSound("FishHit");
                            this.sunShockedFace.ApplyDrawEffect(new ShakeEffect(1f, 1f, 5));
                            base._game.freezeFrames = 60;
                            if (base._game.music!.IsPlaying) {
                                base._game.music.Stop(AudioStopOptions.Immediate);
                                base._game.music = Game1.soundBank.GetCue(musicCue);
                            }
                        }
                    } else if (this._claw.openAngle == 0f && this._claw.GetGrabbedPrize() == null) {
                        if (this.lives == 1) {
                            base._game.music!.Stop(AudioStopOptions.Immediate);
                            Game1.playSound("fishEscape");
                        } else {
                            Game1.playSound("stoneStep");
                        }
                    }
                    this._stateTimer = 0;
                    break;
                }
                if (this._claw.GetGrabbedPrize() != null) {
                    if (!base._game.fastMusic!.IsPlaying) {
                        base._game.fastMusic.Play();
                    }
                } else if (base._game.fastMusic!.IsPlaying) {
                    base._game.fastMusic.Stop(AudioStopOptions.AsAuthored);
                    base._game.fastMusic = Game1.soundBank.GetCue(fastMusicCue);
                }
                if (this._claw.zPosition < 50f) {
                    if (_claw.GetGrabbedPrize() != null) {
                        this._claw.zPosition += 0.5f;
                    } else {
                        _claw.zPosition += 1f;
                    }
                    if (this._claw.zPosition >= 50f) {
                        this._claw.zPosition = 50f;
                        this.SetState(GameStates.ClawReturn);
                        if (this._claw.GetGrabbedPrize() == null && this.lives == 1) {
                            this.SetState(GameStates.EndGame);
                        }
                    }
                }
                this._claw.CheckDropPrize();
                break;
            case GameStates.ClawReturn:
                if (this._claw.GetGrabbedPrize() != null) {
                    if (!base._game.fastMusic!.IsPlaying) {
                        base._game.fastMusic.Play();
                    }
                } else if (base._game.fastMusic!.IsPlaying) {
                    base._game.fastMusic.Stop(AudioStopOptions.AsAuthored);
                    base._game.fastMusic = Game1.soundBank.GetCue(fastMusicCue);
                }
                if (this._stateTimer > 10) {
                    if (this._claw.position.Equals(this._dropPosition)) {
                        this.SetState(GameStates.ClawRelease);
                    } else {
                        float move_speed = 0.5f;
                        if (this._claw.GetGrabbedPrize() == null) {
                            move_speed = 2f;
                        }
                        if (this._claw.position.X != this._dropPosition.X) {
                            this._claw.position.X = Utility.MoveTowards(this._claw.position.X, this._dropPosition.X, move_speed);
                        }
                        if (this._claw.position.X != this._dropPosition.Y) {
                            this._claw.position.Y = Utility.MoveTowards(this._claw.position.Y, this._dropPosition.Y, move_speed);
                        }
                    }
                }
                this._claw.CheckDropPrize();
                break;
            case GameStates.ClawRelease: {
                    bool clawHadPrize = this._claw.GetGrabbedPrize() != null;
                    if (this._stateTimer <= 10) {
                        break;
                    }
                    this._claw.ReleaseGrabbedObject();
                    if (this._claw.openAngle < 40f) {
                        this._claw.openAngle++;
                        break;
                    }
                    this.SetState(GameStates.ClawReset);
                    if (!clawHadPrize) {
                        Game1.playSound("button1");
                        this._claw.ApplyDrawEffect(new ShakeEffect(1f, 1f));
                    }
                    break;
                }
            case GameStates.ClawReset: {
                    if (this._stateTimer <= 50) {
                        break;
                    }
                    if (this._claw.position.Equals(this._startPosition)) {
                        this.lives--;
                        if (this.lives <= 0) {
                            this.SetState(GameStates.EndGame);
                        } else {
                            this.SetState(GameStates.Idle);
                        }
                        break;
                    }
                    float move_speed = 1f;
                    if (this._claw.position.X != this._startPosition.X) {
                        this._claw.position.X = Utility.MoveTowards(this._claw.position.X, this._startPosition.X, move_speed);
                    }
                    if (this._claw.position.X != this._startPosition.Y) {
                        this._claw.position.Y = Utility.MoveTowards(this._claw.position.Y, this._startPosition.Y, move_speed);
                    }
                    break;
                }
            case GameStates.EndGame: {
                    if (base._game.music!.IsPlaying) {
                        base._game.music.Stop(AudioStopOptions.Immediate);
                    }
                    if (base._game.fastMusic!.IsPlaying) {
                        base._game.fastMusic.Stop(AudioStopOptions.Immediate);
                    }
                    bool is_something_busy = false;
                    foreach (Prize item3 in base._game.GetObjectsOfType<Prize>()) {
                        if (!item3.CanBeGrabbed()) {
                            is_something_busy = true;
                            break;
                        }
                    }
                    if (is_something_busy || this._stateTimer < 20) {
                        break;
                    }
                    if (this.collectedItems.Count > 0) {
                        List<Item> items = new List<Item>();
                        foreach (Item item in this.collectedItems) {
                            items.Add(item);
                        }
                        Game1.activeClickableMenu = new ItemGrabMenu(items, reverseGrab: false, showReceivingMenu: true, null, null, "Rewards", null, snapToBottom: false, canBeExitedWithKey: false, playRightClickSound: false, allowRightClick: false, showOrganizeButton: false, 0, null, -1, base._game);
                    }
                    base._game.Quit();
                    break;
                }
        }
        this.sunShockedFace.visible = this._claw.GetGrabbedPrize() != null;
        this.joystick.rotation = Utility.MoveTowards(this.joystick.rotation, desired_joystick_rotation, 2f);
        this._stateTimer++;
    }

    public override void Draw(SpriteBatch b, float layer_depth) {
    }

    public void SetState(GameStates new_state) {
        this._currentState = new_state;
        this._stateTimer = 0;
    }
}