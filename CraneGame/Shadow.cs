using Microsoft.Xna.Framework;
using StardewValley;

namespace CraneGameOverhaul.CraneGame {
    public class Shadow : CraneGameObject {
        public CraneGameObject _target;

        public Shadow(CustomCraneGame game, CraneGameObject target)
            : base(game) {
            base.SetSpriteFromIndex(2);
            base.layerDepth = 900f;
            this._target = target;
        }

        public override void Update(GameTime time) {
            if (this._target != null) {
                base.position = this._target.position;
            }
            if (this._target is Prize { grabbed: not false }) {
                base.visible = false;
            }
            if (this._target!.IsDestroyed()) {
                this.Destroy();
                return;
            }
            base.color.A = (byte)(Math.Min(1f, this._target.zPosition / 50f) * 255f);
            base.scale = Utility.Lerp(1f, 0.5f, Math.Min(this._target.zPosition / 100f, 1f)) * new Vector2(1f, 1f);
        }
    }
}
