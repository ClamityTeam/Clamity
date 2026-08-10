using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.ForbiddenLantern.Projectiles
{
    public class WaterRay : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public static int Lifetime = 600;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 2;
            Projectile.alpha = 255;
            Projectile.timeLeft = Lifetime;
            Projectile.MaxUpdates = 5;
        }

        public override void AI()
        {
            if (Main.rand.NextBool())
            {
                Gore bubble = Gore.NewGorePerfect(Projectile.GetSource_FromAI(), Projectile.position, Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 411);
                bubble.timeLeft = 9 + Main.rand.Next(7);
                bubble.scale = Main.rand.NextFloat(0.6f, 1f);
                bubble.type = Main.rand.NextBool(3) ? 412 : 411;
            }
            if (Projectile.timeLeft <= Lifetime - 4)
            {
                Particle spark = new SparkParticle(Projectile.Center - Projectile.velocity * 2, -Projectile.velocity * 0.05f, false, 10, 1f, Color.RoyalBlue * 0.5f);
                GeneralParticleHandler.SpawnParticle(spark);
                if (Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(3) ? 45 : 56, -Projectile.velocity.RotatedByRandom(0.1f) * Main.rand.NextFloat(0.01f, 0.3f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(1.5f, 0.9f);
                }
            }
        }        
    }
}
