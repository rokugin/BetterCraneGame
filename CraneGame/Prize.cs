using Microsoft.Xna.Framework;
using StardewValley.ItemTypeDefinitions;
using StardewValley;

namespace CraneGameOverhaul.CraneGame {
    public class Prize : CraneGameObject {
        protected Vector2 _conveyorBeltMove;

        public bool grabbed;

        public float gravity;

        protected Vector2 _velocity = Vector2.Zero;

        protected Item _item;

        protected float _restingZPosition;

        protected float _angularSpeed;

        protected bool _isBeingCollected;

        public bool isLargeItem;

        public float GetRestingZPosition() {
            return this._restingZPosition;
        }

        public Prize(CustomCraneGame game, Item item)
            : base(game) {
            base.SetSpriteFromIndex(3);
            base.spriteAnchor = new Vector2(8f, 12f);
            this._item = item;
            this._UpdateItemSprite();
            new Shadow(_game, this);
        }

        public void OnDrop() {
            if (!this.isLargeItem) {
                this._angularSpeed = Utility.Lerp(-5f, 5f, (float)Game1.random.NextDouble());
            } else {
                base.rotation = 0f;
            }
        }

        public void _UpdateItemSprite() {
            ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(this._item.QualifiedItemId);
            base.texture = itemData.GetTexture();
            base.spriteRect = itemData.GetSourceRect();
            base.width = base.spriteRect.Width;
            base.height = base.spriteRect.Height;
            if (base.width > 16 || base.height > 16) {
                this.isLargeItem = true;
            } else {
                this.isLargeItem = false;
            }
            if (base.height <= 16) {
                base.spriteAnchor = new Vector2(base.width / 2, (float)base.height - 4f);
            } else {
                base.spriteAnchor = new Vector2(base.width / 2, (float)base.height - 8f);
            }
            this._restingZPosition = 0f;
        }

        public bool CanBeGrabbed() {
            if (base.IsDestroyed()) {
                return false;
            }
            if (this._isBeingCollected) {
                return false;
            }
            if (base.zPosition != this._restingZPosition) {
                return false;
            }
            return true;
        }

        public override void Update(GameTime time) {
            if (this._isBeingCollected) {
                Vector4 color_vector = base.color.ToVector4();
                color_vector.X = Utility.MoveTowards(color_vector.X, 0f, 0.05f);
                color_vector.Y = Utility.MoveTowards(color_vector.Y, 0f, 0.05f);
                color_vector.Z = Utility.MoveTowards(color_vector.Z, 0f, 0.05f);
                color_vector.W = Utility.MoveTowards(color_vector.W, 0f, 0.05f);
                base.color = new Color(color_vector);
                base.scale.X = Utility.MoveTowards(base.scale.X, 0.5f, 0.05f);
                base.scale.Y = Utility.MoveTowards(base.scale.Y, 0.5f, 0.05f);
                if (color_vector.W == 0f) {
                    Game1.playSound("Ship");
                    this.Destroy();
                }
                base.position.Y += 0.5f;
            } else {
                if (this.grabbed) {
                    return;
                }
                if (this._velocity.X != 0f || this._velocity.Y != 0f) {
                    base.position.X += this._velocity.X;
                    if (!base._game.GetObjectsOfType<GameLogic>()[0].playArea.Contains(new Point((int)base.position.X, (int)base.position.Y))) {
                        base.position.X -= this._velocity.X;
                        this._velocity.X *= -1f;
                    }
                    base.position.Y += this._velocity.Y;
                    if (!base._game.GetObjectsOfType<GameLogic>()[0].playArea.Contains(new Point((int)base.position.X, (int)base.position.Y))) {
                        base.position.Y -= this._velocity.Y;
                        this._velocity.Y *= -1f;
                    }
                }
                if (base.zPosition < this._restingZPosition) {
                    base.zPosition = this._restingZPosition;
                }
                if (base.zPosition > this._restingZPosition || this._velocity != Vector2.Zero || this.gravity != 0f) {
                    if (!this.isLargeItem) {
                        base.rotation += this._angularSpeed;
                    }
                    this._conveyorBeltMove = Vector2.Zero;
                    if (base.zPosition > this._restingZPosition) {
                        this.gravity += 0.1f;
                    }
                    base.zPosition -= this.gravity;
                    if (!(base.zPosition < this._restingZPosition)) {
                        return;
                    }
                    base.zPosition = this._restingZPosition;
                    if (!(this.gravity >= 0f)) {
                        return;
                    }
                    if (!this.isLargeItem) {
                        this._angularSpeed = Utility.Lerp(-10f, 10f, (float)Game1.random.NextDouble());
                    }
                    this.gravity = (0f - this.gravity) * 0.6f;
                    if (base._game.GetObjectsOfType<GameLogic>()[0].prizeChute.Contains(new Point((int)base.position.X, (int)base.position.Y))) {
                        if (base._game.GetObjectsOfType<GameLogic>()[0].GetCurrentState() != 0) {
                            Game1.playSound("reward");
                            this._isBeingCollected = true;
                            List<Item> collectedItems = base._game.GetObjectsOfType<GameLogic>()[0].collectedItems;
                            if (collectedItems.Count > 0) {
                                foreach (Item item in collectedItems) {
                                    if (item.ItemId == _item.ItemId) {
                                        item.Stack += _item.Stack;
                                        _item = null!;
                                        break;
                                    }
                                }
                            }
                            if (_item != null) collectedItems.Add(this._item);
                        } else {
                            this.gravity = -2.5f;
                            Vector2 offset = new Vector2(base._game.GetObjectsOfType<GameLogic>()[0].playArea.Center.X, base._game.GetObjectsOfType<GameLogic>()[0].playArea.Center.Y) - new Vector2(base.position.X, base.position.Y);
                            offset.Normalize();
                            this._velocity = offset * Utility.Lerp(1f, 2f, (float)Game1.random.NextDouble());
                        }
                        return;
                    }
                    if (base._game.GetOverlaps<Trampoline>(this, 1).Count > 0) {
                        Trampoline trampoline = base._game.GetOverlaps<Trampoline>(this, 1)[0];
                        Game1.playSound("axchop");
                        trampoline.ApplyDrawEffect(new StretchEffect(0.75f, 0.75f, 5));
                        trampoline.ApplyDrawEffect(new ShakeEffect(2f, 2f));
                        base.ApplyDrawEffect(new ShakeEffect(2f, 2f));
                        this.gravity = -2.5f;
                        Vector2 offset = new Vector2(base._game.GetObjectsOfType<GameLogic>()[0].playArea.Center.X, base._game.GetObjectsOfType<GameLogic>()[0].playArea.Center.Y) - new Vector2(base.position.X, base.position.Y);
                        offset.Normalize();
                        this._velocity = offset * Utility.Lerp(0.5f, 1f, (float)Game1.random.NextDouble());
                        return;
                    }
                    if (Math.Abs(this.gravity) < 1.5f) {
                        base.rotation = 0f;
                        this._velocity = Vector2.Zero;
                        this.gravity = 0f;
                        return;
                    }
                    bool bumped_object = false;
                    foreach (Prize prize in base._game.GetOverlaps<Prize>(this)) {
                        if (prize.gravity == 0f && prize.CanBeGrabbed()) {
                            Vector2 offset = base.position - prize.position;
                            offset.Normalize();
                            this._velocity = offset * Utility.Lerp(0.25f, 1f, (float)Game1.random.NextDouble());
                            if (!prize.isLargeItem || this.isLargeItem) {
                                prize._velocity = -offset * Utility.Lerp(0.75f, 1.5f, (float)Game1.random.NextDouble());
                                prize.gravity = this.gravity * 0.75f;
                                prize.ApplyDrawEffect(new ShakeEffect(2f, 2f, 20));
                            }
                            bumped_object = true;
                        }
                    }
                    base.ApplyDrawEffect(new ShakeEffect(2f, 2f, 20));
                    if (!bumped_object) {
                        float rad_angle = Utility.Lerp(0f, (float)Math.PI * 2f, (float)Game1.random.NextDouble());
                        this._velocity = new Vector2((float)Math.Sin(rad_angle), (float)Math.Cos(rad_angle)) * Utility.Lerp(0.5f, 1f, (float)Game1.random.NextDouble());
                    }
                } else if (this._conveyorBeltMove.X == 0f && this._conveyorBeltMove.Y == 0f) {
                    List<ConveyorBelt> belts = base._game.GetObjectsAtPoint<ConveyorBelt>(base.position, 1);
                    if (belts.Count > 0) {
                        switch (belts[0].GetDirection()) {
                            case 0:
                                this._conveyorBeltMove = new Vector2(0f, -16f);
                                break;
                            case 2:
                                this._conveyorBeltMove = new Vector2(0f, 16f);
                                break;
                            case 3:
                                this._conveyorBeltMove = new Vector2(-16f, 0f);
                                break;
                            case 1:
                                this._conveyorBeltMove = new Vector2(16f, 0f);
                                break;
                        }
                    }
                } else {
                    float move_speed = 0.3f;
                    if (this._conveyorBeltMove.X != 0f) {
                        this.Move(move_speed * (float)Math.Sign(this._conveyorBeltMove.X), 0f);
                        this._conveyorBeltMove.X = Utility.MoveTowards(this._conveyorBeltMove.X, 0f, move_speed);
                    }
                    if (this._conveyorBeltMove.Y != 0f) {
                        this.Move(0f, move_speed * (float)Math.Sign(this._conveyorBeltMove.Y));
                        this._conveyorBeltMove.Y = Utility.MoveTowards(this._conveyorBeltMove.Y, 0f, move_speed);
                    }
                }
            }
        }
    }
}
