using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace Clamity.Content.Particles
{
    public abstract class BaseClamityParticle : Particle
    {
        public Vector2 ScaleVector;
        public float Opacity = 1f;
        public float LifetimeRatio => (float)Time / (float)Lifetime;
        public Texture2D TextureFile => ModContent.Request<Texture2D>(Texture).Value;
        public string BaseTexturePath => "Clamity/Assets/Textures/Particles";
    }
}
