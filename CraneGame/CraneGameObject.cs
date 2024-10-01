using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CraneGameOverhaul.CraneGame {
    public class CraneGameObject {
        protected CustomCraneGame _game;

        public Vector2 position = Vector2.Zero;

        public float rotation;

        public Vector2 scale = new Vector2(1f, 1f);

        public bool flipX;

        public bool flipY;

        public Rectangle spriteRect;

        public Texture2D texture;

        public Vector2 spriteAnchor;

        public Color color = Color.White;

        public float layerDepth = -1f;

        public int width = 16;

        public int height = 16;

        public float zPosition;

        public bool visible = true;

        public List<DrawEffect> drawEffects;

        protected bool _destroyed;

        public CraneGameObject(CustomCraneGame game) {
            this._game = game;
            this.texture = this._game.spriteSheet;
            this.spriteRect = new Rectangle(0, 0, 16, 16);
            this.spriteAnchor = new Vector2(8f, 8f);
            this.drawEffects = new List<DrawEffect>();
            this._game.RegisterGameObject(this);
        }

        public void SetSpriteFromIndex(int index = 0) {
            this.spriteRect.X = 304 + index % 5 * 16;
            this.spriteRect.Y = index / 5 * 16;
        }

        public bool IsDestroyed() {
            return this._destroyed;
        }

        public virtual void Destroy() {
            this._destroyed = true;
            this._game.UnregisterGameObject(this);
        }

        public virtual void Move(float x, float y) {
            this.position.X += x;
            this.position.Y += y;
        }

        public Rectangle GetBounds() {
            return new Rectangle((int)(this.position.X - this.spriteAnchor.X), (int)(this.position.Y - this.spriteAnchor.Y), this.width, this.height);
        }

        public virtual void Update(GameTime time) {
        }

        public float GetRendererLayerDepth() {
            float layer_depth = this.layerDepth;
            if (layer_depth < 0f) {
                layer_depth = (float)this._game.gameHeight - this.position.Y;
            }
            return layer_depth;
        }

        public void ApplyDrawEffect(DrawEffect new_effect) {
            this.drawEffects.Add(new_effect);
        }

        public virtual void Draw(SpriteBatch b, float layer_depth) {
            if (!this.visible) {
                return;
            }
            SpriteEffects effects = SpriteEffects.None;
            if (this.flipX) {
                effects |= SpriteEffects.FlipHorizontally;
            }
            if (this.flipY) {
                effects |= SpriteEffects.FlipVertically;
            }
            float drawn_rotation = this.rotation;
            Vector2 drawn_scale = this.scale;
            Vector2 drawn_position = this.position - new Vector2(0f, this.zPosition);
            for (int i = 0; i < this.drawEffects.Count; i++) {
                if (this.drawEffects[i].Apply(ref drawn_position, ref drawn_rotation, ref drawn_scale)) {
                    this.drawEffects.RemoveAt(i);
                    i--;
                }
            }
            b.Draw(this.texture, this._game.upperLeft + drawn_position * 4f, this.spriteRect, this.color, drawn_rotation * ((float)Math.PI / 180f), this.spriteAnchor, 4f * drawn_scale, effects, layer_depth);
        }
    }
}
