using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Clamity.Content.Particles
{
    public class VerticalLightStreakParticle : BaseClamityParticle
    {
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => true;
        public override string Texture => $"{BaseTexturePath}/VerticalLightStreak";

        public VerticalLightStreakParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, Vector2 scale)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            ScaleVector = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            Opacity = ClamityUtils.InverseLerp(0f, 4f, Time);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureFile, Position - Main.screenPosition, null, Color * Opacity, Rotation, TextureFile.Size() / 2, ScaleVector, SpriteEffects.None, 0);
        }
    }
}
