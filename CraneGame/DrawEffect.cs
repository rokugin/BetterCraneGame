using Microsoft.Xna.Framework;

namespace CraneGameOverhaul.CraneGame {
    public class DrawEffect {
        public virtual bool Apply(ref Vector2 position, ref float rotation, ref Vector2 scale) {
            return true;
        }
    }
}
