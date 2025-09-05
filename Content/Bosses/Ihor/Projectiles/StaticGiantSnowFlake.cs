using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Ihor.Projectiles
{
    public class StaticGiantSnowFlake : ModProjectile
    {
        public override string Texture => IhorTextures.GiantSnowFlake;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 42;
            Projectile.aiStyle = -1;
            AIType = -1;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item30, Projectile.Center);
        }
        public override void AI()
        {
            Projectile.rotation += Projectile.ai[0];
            Projectile.ai[0] *= 0.99f;
            Projectile.Center = Main.player[(int)Projectile.ai[1]].Center + Vector2.UnitX.RotatedBy(Projectile.ai[2]) * 400;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.UnitX.RotatedBy(i * MathHelper.TwoPi / 8 + Projectile.rotation), ModContent.ProjectileType<SnowFlakeShard>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
        }
        public override bool CanHitPlayer(Player target)
        {
            return false;
        }
    }
}
