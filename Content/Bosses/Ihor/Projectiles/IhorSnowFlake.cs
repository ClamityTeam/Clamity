using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Ihor.Projectiles
{
    public class IhorSnowFlake : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => IhorTextures.SnowFlakeShard;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 25;
            Projectile.aiStyle = -1;
            AIType = -1;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.ai[1] = Main.rand.NextBool() ? 1 : -1;
        }
        public override void AI()
        {
            Projectile.velocity *= 0.98f;
            Projectile.rotation += Projectile.velocity.Length() / 10f * Projectile.ai[1];
        }
    }
}
