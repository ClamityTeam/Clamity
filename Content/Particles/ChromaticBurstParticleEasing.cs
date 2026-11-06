using Luminance.Common.Easings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Clamity.Content.Particles
{
    public class ChromaticBurstParticleEasing : BaseClamityParticle
    {
        public float StartingScale = 1f;
        public float FinalScale = 1f;
        public Color StartingColor = Color.White;
        public Color FinalColor = Color.White;
        public EasingType ScaleEasingType = EasingType.In;
        public EasingCurves.Curve ScaleEasingCurve = EasingCurves.Linear;

        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => true;

        public override string Texture => $"{BaseTexturePath}/ChromaticBurst";

        public ChromaticBurstParticleEasing(Vector2 position, Vector2 velocity, Color startingColor, Color finalColor, int lifetime, float startingScale, float finalScale, EasingCurves.Curve scaleEasingCurve, EasingType scaleEasingType)
        {
            Position = position;
            Velocity = velocity;

            Color = Color.White;
            StartingColor = startingColor;
            FinalColor = finalColor;

            StartingScale = startingScale;
            FinalScale = finalScale;

            Lifetime = lifetime;
            //Time = lifetime;
            ScaleEasingCurve = scaleEasingCurve;
            ScaleEasingType = scaleEasingType;
        }

        public override void Update()
        {
            //Opacity = Utils.GetLerpValue(0f, 4f, Lifetime - Time, true);
            Color = Color.Lerp(StartingColor, FinalColor, LifetimeCompletion);
            Scale = ScaleEasingCurve.Evaluate(ScaleEasingType, StartingScale, FinalScale, LifetimeCompletion);
            //Scale += 0.8f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureFile, Position - Main.screenPosition, null, Color * Opacity, Rotation, TextureFile.Size() * 0.5f, Scale * 0.3f, 0, 0f);
        }
    }
}
