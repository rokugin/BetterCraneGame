using Microsoft.Xna.Framework;

namespace CraneGameOverhaul.CraneGame {
    public class ConveyorBelt : CraneGameObject {
        protected int _direction;

        protected Vector2 _spriteStartPosition;

        protected int _spriteOffset;

        public int GetDirection() {
            return this._direction;
        }

        public ConveyorBelt(CustomCraneGame game, int x, int y, int direction)
            : base(game) {
            base.position.X = x * 16;
            base.position.Y = y * 16;
            this._direction = direction;
            base.spriteAnchor = Vector2.Zero;
            base.layerDepth = 1000f;
            switch (this._direction) {
                case 0:
                    base.SetSpriteFromIndex(5);
                    break;
                case 2:
                    base.SetSpriteFromIndex(10);
                    break;
                case 3:
                    base.SetSpriteFromIndex(15);
                    break;
                case 1:
                    base.SetSpriteFromIndex(20);
                    break;
            }
            this._spriteStartPosition = new Vector2(base.spriteRect.X, base.spriteRect.Y);
        }

        public void SetSpriteFromCorner(int x, int y) {
            base.spriteRect.X = x;
            base.spriteRect.Y = y;
            this._spriteStartPosition = new Vector2(base.spriteRect.X, base.spriteRect.Y);
        }

        public override void Update(GameTime time) {
            int ticks_per_frame = 4;
            int frame_count = 4;
            base.spriteRect.X = (int)this._spriteStartPosition.X + this._spriteOffset / ticks_per_frame * 16;
            this._spriteOffset++;
            if (this._spriteOffset >= (frame_count - 1) * ticks_per_frame) {
                this._spriteOffset = 0;
            }
        }
    }
}
