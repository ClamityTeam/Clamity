using CalamityMod.Enums;
using CalamityMod.Particles;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.TheForbiddenLantern.Projectiles
{
    public class SeashineMine : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 42;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;

        }
        //public const int MaxTimeLeft = 600;
        public static int RotatingTime => 90;
        public static int Delay => 30;
        public static int MineTotalCount => 4;
        public override void AI()
        {
            NPC lantern = Main.npc[(int)Projectile.ai[0]];
            Projectile.ai[2]++;
            int time = RotatingTime + Delay * MineTotalCount - (int)Projectile.ai[1] * Delay + Delay;
            if (Projectile.ai[2] < time)
            {
                Projectile.Center = lantern.Center + Vector2.UnitX.RotatedBy(MathHelper.TwoPi / MineTotalCount * Projectile.ai[1] + Main.GlobalTimeWrappedHourly * 2) * 150f * MathHelper.Clamp(Projectile.ai[2] / (float)RotatingTime, 0, 1);
                Projectile.rotation = Projectile.Center.SafeDirectionTo(lantern.Center).ToRotation();
            }
            else if (Projectile.ai[2] == time)
            {
                Projectile.velocity = Main.rand.NextVector2CircularEdge(20, 20);
                Projectile.timeLeft = 60;
            }
            else Projectile.velocity *= .95f;


        }
        public override void OnKill(int timeLeft)
        {
            Particle explosion = new DetailedExplosion(Projectile.Center, Vector2.Zero, Color.Cyan, Vector2.One, Main.rand.NextFloat(-5, 5), 0f, .5f, Main.rand.Next(15, 22));
            GeneralParticleHandler.SpawnParticle(explosion, false, GeneralDrawLayer.AfterEverything);
            Particle explosion2 = new DetailedExplosion(Projectile.Center, Vector2.Zero, Color.Black, Vector2.One, Main.rand.NextFloat(-5, 5), 0f, .34f, Main.rand.Next(15, 22), false);
            GeneralParticleHandler.SpawnParticle(explosion2);
            Particle explosion3 = new DetailedExplosion(Projectile.Center, Vector2.Zero, Color.Black, Vector2.One, Main.rand.NextFloat(-5, 5), 0f, .23f, Main.rand.Next(15, 22), false);
            GeneralParticleHandler.SpawnParticle(explosion3);

            int c = 5;
            float offset = Main.rand.NextFloat(0, MathHelper.TwoPi);
            for (int i = 0; i < c; i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.UnitX.RotatedBy(MathHelper.TwoPi / c * i + offset) * 10, ModContent.ProjectileType<SeashineMineShards>(), Projectile.damage, 1, Main.myPlayer);
            }
        }
    }
}
