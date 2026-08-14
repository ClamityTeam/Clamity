using CalamityMod;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.WoB.Projectiles
{
    public class GiantBoulder : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override void SetStaticDefaults()
        {
            
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 90;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 2;
            Projectile.Calamity().DealsDefenseDamage = true;

        }
        public override void AI()
        {
            NPC wob = Main.npc[(int)Projectile.ai[0]];

            Projectile.velocity.Y -= Projectile.velocity.Y > 10f ? 0 : 0.1f;
            Projectile.rotation += 0.1f;

            if (wob.Center.Distance(Projectile.Center) < 1200) Projectile.Kill();
        }
        public override void OnKill(int timeLeft)
        {
            int imax = 10;
            for (int i = 0; i < imax; i++) 
            {
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.UnitX.RotatedBy(MathHelper.TwoPi / imax * i) * 10, ModContent.ProjectileType<GiantBoulderFragments>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
        }
    }
}
