using Microsoft.Xna.Framework;
using StardewValley;

namespace BetterCraneGame.CraneGame {
    public class StretchEffect : DrawEffect {
        public Vector2 stretchScale;

        public int stretchDuration = 1;

        public int age;

        public StretchEffect(float x_scale, float y_scale, int stretch_duration = 10) {
            this.stretchScale = new Vector2(x_scale, y_scale);
            this.stretchDuration = stretch_duration;
            this.age = 0;
        }

        public override bool Apply(ref Vector2 position, ref float rotation, ref Vector2 scale) {
            if (this.age > this.stretchDuration) {
                return true;
            }
            float progress = (float)this.age / (float)this.stretchDuration;
            Vector2 current_scale = new Vector2(Utility.Lerp(this.stretchScale.X, 1f, progress), Utility.Lerp(this.stretchScale.Y, 1f, progress));
            scale *= current_scale;
            this.age++;
            return false;
        }
    }
}
