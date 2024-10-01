using Microsoft.Xna.Framework;
using StardewValley;

namespace CraneGameOverhaul.CraneGame {
    public class Claw : CraneGameObject {
        protected CraneGameObject _leftArm;

        protected CraneGameObject _rightArm;

        protected Prize? _grabbedPrize;

        protected Vector2 _prizePositionOffset;

        protected int _nextDropCheckTimer;

        protected int _dropChances;

        protected int _grabTime;

        public float openAngle {
            get
            {
                return this._leftArm.rotation;
            }
            set
            {
                this._leftArm.rotation = value;
            }
        }

        public Claw(CustomCraneGame game)
            : base(game) {
            base.SetSpriteFromIndex();
            base.spriteAnchor = new Vector2(8f, 24f);
            this._leftArm = new CraneGameObject(game);
            this._leftArm.SetSpriteFromIndex(1);
            this._leftArm.spriteAnchor = new Vector2(16f, 0f);
            this._rightArm = new CraneGameObject(game);
            this._rightArm.SetSpriteFromIndex(1);
            this._rightArm.flipX = true;
            this._rightArm.spriteAnchor = new Vector2(0f, 0f);
            new Shadow(base._game, this);
        }

        public void CheckDropPrize() {
            if (this._grabbedPrize == null) {
                return;
            }
            this._nextDropCheckTimer--;
            if (this._nextDropCheckTimer > 0) {
                return;
            }
            float drop_chance = this._prizePositionOffset.Length() * 0.1f;
            drop_chance += base.zPosition * 0.001f;
            if (this._grabbedPrize.isLargeItem) {
                drop_chance += 0.1f;
            }
            double roll = Game1.random.NextDouble();
            if (roll < (double)drop_chance) {
                this._dropChances--;
                if (this._dropChances <= 0) {
                    Game1.playSound("fishEscape");
                    this.ReleaseGrabbedObject();
                } else {
                    Game1.playSound("bob");
                    this._grabbedPrize.ApplyDrawEffect(new ShakeEffect(2f, 2f, 50));
                    this._grabbedPrize.rotation += (float)Game1.random.NextDouble() * 10f;
                }
            } else if (roll < (double)drop_chance) {
                Game1.playSound("dwop");
                this._grabbedPrize.ApplyDrawEffect(new ShakeEffect(1f, 1f, 50));
            }
            this._nextDropCheckTimer = Game1.random.Next(50, 100);
        }

        public void ApplyDrawEffectToArms(DrawEffect new_effect) {
            this._leftArm.ApplyDrawEffect(new_effect);
            this._rightArm.ApplyDrawEffect(new_effect);
        }

        public void ReleaseGrabbedObject() {
            if (this._grabbedPrize != null) {
                this._grabbedPrize.grabbed = false;
                this._grabbedPrize.OnDrop();
                this._grabbedPrize = null;
            }
        }

        public void GrabObject() {
            Prize? closest_prize = null;
            float closest_distance = 0f;
            foreach (Prize prize in base._game.GetObjectsAtPoint<Prize>(base.position)) {
                if (!prize.IsDestroyed() && prize.CanBeGrabbed()) {
                    float distance = (base.position - prize.position).LengthSquared();
                    if (closest_prize == null || distance < closest_distance) {
                        closest_distance = distance;
                        closest_prize = prize;
                    }
                }
            }
            if (closest_prize != null) {
                this._grabbedPrize = closest_prize;
                this._grabbedPrize.grabbed = true;
                this._prizePositionOffset = this._grabbedPrize.position - base.position;
                this._nextDropCheckTimer = Game1.random.Next(50, 100);
                this._dropChances = 3;
                Game1.playSound("pickUpItem");
                this._grabTime = 0;
                this._grabbedPrize.ApplyDrawEffect(new StretchEffect(0.95f, 1.1f));
                this._grabbedPrize.ApplyDrawEffect(new ShakeEffect(1f, 1f, 20));
            }
        }

        public Prize GetGrabbedPrize() {
            return this._grabbedPrize!;
        }

        public override void Update(GameTime time) {
            this._leftArm.position = base.position + new Vector2(0f, -16f);
            this._rightArm.position = base.position + new Vector2(0f, -16f);
            this._rightArm.rotation = 0f - this._leftArm.rotation;
            this._leftArm.layerDepth = (this._rightArm.layerDepth = base.GetRendererLayerDepth() + 0.01f);
            this._leftArm.zPosition = (this._rightArm.zPosition = base.zPosition);
            if (this._grabbedPrize != null) {
                this._grabbedPrize.position = base.position + this._prizePositionOffset * Utility.Lerp(1f, 0.25f, Math.Min(1f, (float)this._grabTime / 200f));
                this._grabbedPrize.zPosition = base.zPosition + this._grabbedPrize.GetRestingZPosition();
            }
            this._grabTime++;
        }

        public override void Destroy() {
            this._leftArm.Destroy();
            this._rightArm.Destroy();
            base.Destroy();
        }
    }
}
