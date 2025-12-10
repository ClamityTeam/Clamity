using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Clamity.Content.Particles
{
    public class MagicBurstParticle : BaseClamityParticle
    {
        public float ScaleExpandRate;

        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => true;

        public override string Texture => $"{BaseTexturePath}/MagicBurst";

        public MagicBurstParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float scaleExpandRate = 0f)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            ScaleVector = Vector2.One * scale;
            Lifetime = lifetime;
            ScaleExpandRate = scaleExpandRate;
        }

        public override void Update()
        {
            Opacity = ClamityUtils.InverseLerp(0f, 4f, Lifetime - Time);
            ScaleVector += Vector2.One * ScaleExpandRate;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Rectangle frame = TextureFile.Frame(1, 5, 0, (int)(LifetimeRatio * 4.999f));
            spriteBatch.Draw(TextureFile, Position - Main.screenPosition, frame, Color * Opacity, Rotation, new Vector2(frame.Width / 2, frame.Height / 2), ScaleVector * 0.8f, SpriteEffects.None, 0);

        }
    }
}
