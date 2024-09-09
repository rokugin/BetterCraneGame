using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using StardewValley.GameData;
using StardewValley.Menus;
using StardewValley;
using StardewValley.Minigames;

namespace CraneGameOverhaul {
    public class CustomCraneGame : IMinigame {

        public int gameWidth = 304;

        public int gameHeight = 150;

        protected LocalizedContentManager _content;

        public Texture2D spriteSheet;

        public Vector2 upperLeft;

        protected List<CraneGameObject> _gameObjects;

        protected Dictionary<GameButtons, int> _buttonStates;

        protected bool _shouldQuit;

        public Action? onQuit;

        public ICue? music;

        public ICue? fastMusic;

        public Effect _effect;

        public int freezeFrames;

        public ICue? craneSound;

        public List<Type> _gameObjectTypes;

        public Dictionary<Type, List<CraneGameObject>> _gameObjectsByType;

        public CustomCraneGame() {
            Utility.farmerHeardSong(Game1.soundBank.Exists("cgo_crane_game") ? "cgo_crane_game" : "crane_game");
            Utility.farmerHeardSong(Game1.soundBank.Exists("cgo_crane_game_fast") ? "cgo_crane_game_fast" : "crane_game_fast");
            this._effect = Game1.content.Load<Effect>("Effects\\ShadowRemoveMG3.8.0");
            this._content = Game1.content.CreateTemporary();
            this.spriteSheet = AssetManager.CraneGameTexture;
            this._buttonStates = new Dictionary<GameButtons, int>();
            this._gameObjects = new List<CraneGameObject>();
            this._gameObjectTypes = new List<Type>();
            this._gameObjectsByType = new Dictionary<Type, List<CraneGameObject>>();
            this.changeScreenSize();
            new GameLogic(this);
            for (int i = 0; i < 9; i++) {
                this._buttonStates[(GameButtons)i] = 0;
            }
        }

        public void Quit() {
            if (!this._shouldQuit) {
                this.onQuit?.Invoke();
                this._shouldQuit = true;
            }
        }

        protected void _UpdateInput() {
            HashSet<InputButton> additional_keys = new HashSet<InputButton>();
            if (Game1.options.gamepadControls) {
                GamePadState pad_state = Game1.input.GetGamePadState();
                ButtonCollection.ButtonEnumerator enumerator = new ButtonCollection(ref pad_state).GetEnumerator();
                while (enumerator.MoveNext()) {
                    Keys key = Utility.mapGamePadButtonToKey(enumerator.Current);
                    additional_keys.Add(new InputButton(key));
                }
            }
            if (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed) {
                additional_keys.Add(new InputButton(mouseLeft: true));
            } else if (Game1.input.GetMouseState().RightButton == ButtonState.Pressed) {
                additional_keys.Add(new InputButton(mouseLeft: false));
            }
            this._UpdateButtonState(GameButtons.Action, Game1.options.actionButton, additional_keys);
            this._UpdateButtonState(GameButtons.Tool, Game1.options.useToolButton, additional_keys);
            this._UpdateButtonState(GameButtons.Confirm, Game1.options.menuButton, additional_keys);
            this._UpdateButtonState(GameButtons.Cancel, Game1.options.cancelButton, additional_keys);
            this._UpdateButtonState(GameButtons.Run, Game1.options.runButton, additional_keys);
            this._UpdateButtonState(GameButtons.Up, Game1.options.moveUpButton, additional_keys);
            this._UpdateButtonState(GameButtons.Down, Game1.options.moveDownButton, additional_keys);
            this._UpdateButtonState(GameButtons.Left, Game1.options.moveLeftButton, additional_keys);
            this._UpdateButtonState(GameButtons.Right, Game1.options.moveRightButton, additional_keys);
        }

        public bool IsButtonPressed(GameButtons button) {
            return this._buttonStates[button] == 1;
        }

        public bool IsButtonDown(GameButtons button) {
            return this._buttonStates[button] > 0;
        }

        protected void _UpdateButtonState(GameButtons button, InputButton[] keys, HashSet<InputButton> emulated_keys) {
            bool down = Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), keys);
            for (int i = 0; i < keys.Length; i++) {
                if (emulated_keys.Contains(keys[i])) {
                    down = true;
                    break;
                }
            }
            if (this._buttonStates[button] == -1) {
                this._buttonStates[button] = 0;
            }
            if (down) {
                this._buttonStates[button]++;
            } else if (this._buttonStates[button] > 0) {
                this._buttonStates[button] = -1;
            }
        }

        public T GetObjectAtPoint<T>(Vector2 point, int max_count = -1) where T : CraneGameObject {
            foreach (CraneGameObject gameObject in this._gameObjects) {
                if (gameObject is T match && match.GetBounds().Contains((int)point.X, (int)point.Y)) {
                    return match;
                }
            }
            return null!;
        }

        public List<T> GetObjectsAtPoint<T>(Vector2 point, int max_count = -1) where T : CraneGameObject {
            List<T> results = new List<T>();
            foreach (CraneGameObject gameObject in this._gameObjects) {
                if (gameObject is T match && match.GetBounds().Contains((int)point.X, (int)point.Y)) {
                    results.Add(match);
                    if (max_count >= 0 && results.Count >= max_count) {
                        return results;
                    }
                }
            }
            return results;
        }

        public T GetObjectOfType<T>() where T : CraneGameObject {
            if (this._gameObjectsByType.TryGetValue(typeof(T), out var gameObjects) && gameObjects.Count > 0) {
                return (gameObjects[0] as T)!;
            }
            return null!;
        }

        public List<T> GetObjectsOfType<T>() where T : CraneGameObject {
            List<T> results = new List<T>();
            foreach (CraneGameObject gameObject in this._gameObjects) {
                if (gameObject is T match) {
                    results.Add(match);
                }
            }
            return results;
        }

        public List<T> GetOverlaps<T>(CraneGameObject target, int max_count = -1) where T : CraneGameObject {
            List<T> results = new List<T>();
            foreach (CraneGameObject gameObject in this._gameObjects) {
                if (gameObject is T match && target.GetBounds().Intersects(match.GetBounds()) && target != match) {
                    results.Add(match);
                    if (max_count >= 0 && results.Count >= max_count) {
                        return results;
                    }
                }
            }
            return results;
        }

        public bool tick(GameTime time) {
            if (this._shouldQuit) {
                return true;
            }
            if (this.freezeFrames > 0) {
                this.freezeFrames--;
            } else {
                this._UpdateInput();
                for (int i = 0; i < this._gameObjects.Count; i++) {
                    if (this._gameObjects[i] != null) {
                        this._gameObjects[i].Update(time);
                    }
                }
            }
            if (this.IsButtonPressed(GameButtons.Confirm)) {
                this.Quit();
                Game1.playSound("bigDeSelect");
                GameLogic logic = this.GetObjectOfType<GameLogic>();
                if (logic != null && logic.collectedItems.Count > 0) {
                    List<Item> items = new List<Item>();
                    foreach (Item item in logic.collectedItems) {
                        items.Add(item);
                    }
                    Game1.activeClickableMenu = new ItemGrabMenu(
                        inventory: items,
                        reverseGrab: false,
                        showReceivingMenu: true,
                        highlightFunction: null,
                        behaviorOnItemSelectFunction: null,
                        message: "Rewards",
                        behaviorOnItemGrab: null,
                        snapToBottom: false,
                        canBeExitedWithKey: false,
                        playRightClickSound: false,
                        allowRightClick: false,
                        showOrganizeButton: false,
                        source: 0,
                        sourceItem: null,
                        whichSpecialButton: -1,
                        context: this);
                }
            }
            return false;
        }

        public bool forceQuit() {
            this.Quit();
            this.unload();
            GameLogic logic = this.GetObjectOfType<GameLogic>();
            if (logic != null) {
                foreach (Item collectedItem in logic.collectedItems) {
                    Utility.CollectOrDrop(collectedItem);
                }
            }
            return true;
        }

        public bool overrideFreeMouseMovement() {
            return Game1.options.SnappyMenus;
        }

        public bool doMainGameUpdates() {
            return false;
        }

        public void receiveLeftClick(int x, int y, bool playSound = true) {
        }

        public void leftClickHeld(int x, int y) {
        }

        public void receiveRightClick(int x, int y, bool playSound = true) {
        }

        public void releaseLeftClick(int x, int y) {
        }

        public void releaseRightClick(int x, int y) {
        }

        public void receiveKeyPress(Keys k) {
        }

        public void receiveKeyRelease(Keys k) {
        }

        public void RegisterGameObject(CraneGameObject game_object) {
            if (!this._gameObjectTypes.Contains(game_object.GetType())) {
                this._gameObjectTypes.Add(game_object.GetType());
                this._gameObjectsByType[game_object.GetType()] = new List<CraneGameObject>();
            }
            this._gameObjectsByType[game_object.GetType()].Add(game_object);
            this._gameObjects.Add(game_object);
        }

        public void UnregisterGameObject(CraneGameObject game_object) {
            this._gameObjectsByType[game_object.GetType()].Remove(game_object);
            this._gameObjects.Remove(game_object);
        }

        public void draw(SpriteBatch b) {
            b.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, this._effect);
            b.Draw(this.spriteSheet, this.upperLeft, new Rectangle(0, 0, this.gameWidth, this.gameHeight), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            Dictionary<CraneGameObject, float> depth_lookup = new Dictionary<CraneGameObject, float>();
            float lowest_depth = 0f;
            float highest_depth = 0f;
            for (int i = 0; i < this._gameObjects.Count; i++) {
                if (this._gameObjects[i] != null) {
                    float depth = this._gameObjects[i].GetRendererLayerDepth();
                    depth_lookup[this._gameObjects[i]] = depth;
                    if (depth < lowest_depth) {
                        lowest_depth = depth;
                    }
                    if (depth > highest_depth) {
                        highest_depth = depth;
                    }
                }
            }
            for (int i = 0; i < this._gameObjectTypes.Count; i++) {
                Type type = this._gameObjectTypes[i];
                for (int j = 0; j < this._gameObjectsByType[type].Count; j++) {
                    float drawn_depth = Utility.Lerp(0.1f, 0.9f, (depth_lookup[this._gameObjectsByType[type][j]] - lowest_depth) / (highest_depth - lowest_depth));
                    this._gameObjectsByType[type][j].Draw(b, drawn_depth);
                }
            }
            b.End();
        }

        public void changeScreenSize() {
            float pixel_zoom_adjustment = 1f / Game1.options.zoomLevel;
            Rectangle localMultiplayerWindow = Game1.game1.localMultiplayerWindow;
            float w = localMultiplayerWindow.Width;
            float h = localMultiplayerWindow.Height;
            Vector2 tmp = new Vector2(w / 2f, h / 2f) * pixel_zoom_adjustment;
            tmp.X -= this.gameWidth / 2 * 4;
            tmp.Y -= this.gameHeight / 2 * 4;
            this.upperLeft = tmp;
        }

        public void unload() {
            Game1.stopMusicTrack(MusicContext.MiniGame);
            if (this.music?.IsPlaying ?? false) {
                this.music.Stop(AudioStopOptions.Immediate);
            }
            if (this.fastMusic?.IsPlaying ?? false) {
                this.fastMusic.Stop(AudioStopOptions.Immediate);
            }
            if (this.craneSound?.IsPlaying ?? false) {
                this.craneSound.Stop(AudioStopOptions.Immediate);
            }
            this._content.Unload();
        }

        public void receiveEventPoke(int data) {
        }

        public string minigameId() {
            return "CraneGame";
        }

    }
}
