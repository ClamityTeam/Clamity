using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Ihor.Projectiles
{
    public class IhorSnowball : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 18;
            Projectile.aiStyle = -1;
            AIType = -1;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
        }

        public override void AI()
        {
            Projectile owner = Main.projectile[(int)Projectile.ai[0]];
            float process = Projectile.timeLeft / 120f;
            Projectile.Center = owner.Center + new Vector2(process * 100, 0).RotatedBy(process * MathHelper.PiOver2);
        }
    }
}
