using Microsoft.Xna.Framework;
using StardewValley;

namespace CraneGameOverhaul.CraneGame {
    public class SwayEffect : DrawEffect {
        public float swayMagnitude;

        public float swaySpeed;

        public int swayDuration = 1;

        public int age;

        public SwayEffect(float magnitude, float speed = 1f, int sway_duration = 10) {
            this.swayMagnitude = magnitude;
            this.swaySpeed = speed;
            this.swayDuration = sway_duration;
            this.age = 0;
        }

        public override bool Apply(ref Vector2 position, ref float rotation, ref Vector2 scale) {
            if (this.age > this.swayDuration) {
                return true;
            }
            float progress = (float)this.age / (float)this.swayDuration;
            rotation += (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 1000.0 * 360.0 * (double)this.swaySpeed * 0.01745329238474369) * (1f - progress) * this.swayMagnitude;
            this.age++;
            return false;
        }
    }
}
