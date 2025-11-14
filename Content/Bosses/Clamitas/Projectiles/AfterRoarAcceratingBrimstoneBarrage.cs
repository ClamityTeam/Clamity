using CalamityMod;
using Clamity.Content.Bosses.Clamitas.NPCs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Clamitas.Projectiles
{
    public class AfterRoarAcceratingBrimstoneBarrage : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/Boss/BrimstoneBarrage";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 2;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 44;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 690;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }
        public override void AI()
        {
            if (Projectile.ai[1] == 0) 
            {
                if (Projectile.velocity.Length() > 0.1f)
                    Projectile.velocity *= 0.8f;
                else
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 0.01f;

                if (ClamitasBoss.Myself.ai[0] == (int)ClamitasAttacks.SidefallTeleports && ClamitasBoss.Myself.Calamity().newAI[0] == 1)
                {
                    Projectile.velocity = Projectile.velocity *= 100;
                    Projectile.ai[1] = 1f;
                }
            }
            else
            {

            }
        }
    }
}
