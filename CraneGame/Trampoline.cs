namespace CraneGameOverhaul.CraneGame {
    public class Trampoline : CraneGameObject {
        public Trampoline(CustomCraneGame game, int x, int y)
            : base(game) {
            base.SetSpriteFromIndex(30);
            base.spriteRect.Width = 32;
            base.spriteRect.Height = 32;
            base.spriteAnchor.X = 15f;
            base.spriteAnchor.Y = 15f;
            base.position.X = x;
            base.position.Y = y;
        }
    }
}
