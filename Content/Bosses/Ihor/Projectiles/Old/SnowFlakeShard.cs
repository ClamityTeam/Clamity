using CalamityMod;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Ihor.Projectiles.Old
{
    public class SnowFlakeShard : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => IhorTextures.SnowFlakeShard;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.aiStyle = -1;
            AIType = -1;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
        }
        public override void AI()
        {
            Projectile.velocity *= 1.02f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Projectile.DrawProjectileWithBackglow(Color.Cyan, lightColor, 2f);
            return base.PreDraw(ref lightColor);
        }
    }
}
