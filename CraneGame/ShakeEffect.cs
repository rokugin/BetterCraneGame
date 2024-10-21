using Microsoft.Xna.Framework;
using StardewValley;

namespace BetterCraneGame.CraneGame {
    public class ShakeEffect : DrawEffect {
        public Vector2 shakeAmount;

        public int shakeDuration = 1;

        public int age;

        public ShakeEffect(float shake_x, float shake_y, int shake_duration = 10) {
            this.shakeAmount = new Vector2(shake_x, shake_y);
            this.shakeDuration = shake_duration;
            this.age = 0;
        }

        public override bool Apply(ref Vector2 position, ref float rotation, ref Vector2 scale) {
            if (this.age > this.shakeDuration) {
                return true;
            }
            float progress = (float)this.age / (float)this.shakeDuration;
            Vector2 current_shake = new Vector2(Utility.Lerp(this.shakeAmount.X, 1f, progress), Utility.Lerp(this.shakeAmount.Y, 1f, progress));
            position += new Vector2((float)(Game1.random.NextDouble() - 0.5) * 2f * current_shake.X, (float)(Game1.random.NextDouble() - 0.5) * 2f * current_shake.Y);
            this.age++;
            return false;
        }
    }
}
