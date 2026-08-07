using Terraria.ModLoader;

namespace Clamity.Content.Bosses.TheForbiddenLantern.Projectiles
{
    public class SeashineMineShards : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 22;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;

        }
        public override void AI()
        {
            Projectile.rotation += .1f * (Projectile.timeLeft / 300);
            Projectile.velocity *= .99f;
        }
    }
}
