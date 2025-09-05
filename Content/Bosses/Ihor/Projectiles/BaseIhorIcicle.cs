using CalamityMod;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Ihor.Projectiles
{
    public abstract class BaseIhorIcicle : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => IhorTextures.Icicle;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 18;
            Projectile.aiStyle = -1;
            AIType = -1;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(3);
        }
        public sealed override void AI()
        {
            BaseAI();

            if (Projectile.timeLeft % 2 == 0 && Projectile.timeLeft < 550 && Projectile.velocity.Length() > 1f)
            {
                SparkParticle spark = new SparkParticle(Projectile.Center - Projectile.velocity * 2f, -Projectile.velocity * 0.1f, false, 9, 1.5f, Color.White * 0.2f);
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }
        public virtual void BaseAI()
        {

        }
        /*public override bool PreDraw(ref Color lightColor)
        {
            Projectile.DrawProjectileWithBackglow(Color.Cyan, lightColor, 2f);
            return base.PreDraw(ref lightColor);
        }*/
        public override bool PreDraw(ref Color lightColor)
        {
            Projectile.DrawProjectileWithBackglow(Color.Cyan, lightColor, 2f);
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor);
            return false;
        }
    }
}
