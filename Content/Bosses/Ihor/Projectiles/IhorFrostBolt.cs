using CalamityMod.Particles;
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Ihor.Projectiles
{
    public class IhorFrostBolt : ModProjectile, ILocalizedModType
    {
        public override string Texture => ModContent.GetInstance<FrostBoltProjectile>().Texture;
        public new string LocalizationCategory => "Projectiles.Boss";

        public Color bColor = Color.DeepSkyBlue;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 480;
            Projectile.tileCollide = false;
            //Projectile.coldDamage = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 0)
                Projectile.velocity.Y += 0.1f;

            if (Main.rand.NextBool(15))
            {
                Particle fx = new CustomSpark(Projectile.Center + Main.rand.NextVector2Circular(8, 8), -Projectile.velocity * 0.3f, "CalamityMod/Particles/IceTypeParticle", false, 32, 0.9f, Color.Lerp(bColor, Color.White, 0.5f), new Vector2(0.8f, 1f), true, false);
                GeneralParticleHandler.SpawnParticle(fx);
            }

            Particle trail = new CustomSpark(Projectile.Center, -Projectile.velocity * 0.3f, "CalamityMod/Particles/BloomCircle", false, 7, 0.2f, bColor * 0.9f, new Vector2(1, 1f), true, false, shrinkSpeed: 0.8f);
            GeneralParticleHandler.SpawnParticle(trail);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return ModContent.GetInstance<FrostBoltProjectile>().PreDraw(ref lightColor);
        }
    }
}
