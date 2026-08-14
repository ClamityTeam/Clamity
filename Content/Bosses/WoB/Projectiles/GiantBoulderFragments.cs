using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.WoB.Projectiles
{
    public class GiantBoulderFragments : ModProjectile, ILocalizedModType
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
            Projectile.velocity.Y -= Projectile.velocity.Y > 10f ? 0 : 0.2f;
            Projectile.rotation += 0.2f;
        }
    }
}
