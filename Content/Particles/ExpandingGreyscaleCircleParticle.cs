using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Clamity.Content.Particles
{
    public class ExpandingGreyscaleCircleParticle : BaseClamityParticle
    {
        public override bool SetLifetime => true;

        public override bool UseCustomDraw => true;

        public override bool UseAdditiveBlend => true;

        public override string Texture => $"{BaseTexturePath}/ExpandingGreyscaleCircle";

        public ExpandingGreyscaleCircleParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            ScaleVector = Vector2.One * scale;
            Lifetime = lifetime;

        }
        public override void Update()
        {
            Opacity = ClamityUtils.InverseLerp(0f, 4f, Lifetime - Time);
            ScaleVector += Vector2.One * 0.9f;

        }
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureFile, Position - Main.screenPosition, null, Color * Opacity, Rotation, TextureFile.Size() / 2, ScaleVector * 0.4f, SpriteEffects.None, 0);
        }
    }
}
