using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Clamity.Content.Particles
{
    public class ChromaticBurstParticle : BaseClamityParticle
    {
        public float StartingScale = 1f;
        public float FinalScale = 1f;

        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => true;

        public override string Texture => $"{BaseTexturePath}/ChromaticBurst";

        public ChromaticBurstParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float startingScale, float finalScale)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            StartingScale = startingScale;
            FinalScale = finalScale;
            Lifetime = lifetime;
            Time = lifetime;
        }

        public override void Update()
        {
            Opacity = Utils.GetLerpValue(0f, 4f, Lifetime - Time, true);
            Scale = MathHelper.Lerp(StartingScale, FinalScale, LifetimeCompletion);
            //Scale += 0.8f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureFile, Position - Main.screenPosition, null, Color * Opacity, Rotation, TextureFile.Size() * 0.5f, Scale * 0.3f, 0, 0f);
        }
    }
}
