using Microsoft.Xna.Framework;

namespace CraneGameOverhaul {
    public class Bush : CraneGameObject {
        public Bush(CustomCraneGame game, int tile_index, int tile_width, int tile_height, int x, int y)
            : base(game) {
            base.SetSpriteFromIndex(tile_index);
            base.spriteRect.Width = tile_width * 16;
            base.spriteRect.Height = tile_height * 16;
            base.spriteAnchor.X = (float)base.spriteRect.Width / 2f;
            base.spriteAnchor.Y = base.spriteRect.Height;
            if (tile_height > 16) {
                base.spriteAnchor.Y -= 8f;
            } else {
                base.spriteAnchor.Y -= 4f;
            }
            base.position.X = x;
            base.position.Y = y;
        }

        public override void Update(GameTime time) {
            base.rotation = (float)Math.Sin(time.TotalGameTime.TotalMilliseconds * 0.0024999999441206455 + (double)base.position.Y + (double)(base.position.X * 2f)) * 2f;
        }
    }
}
