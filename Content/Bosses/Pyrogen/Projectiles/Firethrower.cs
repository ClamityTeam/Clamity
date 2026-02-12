using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Pyrogen.Projectiles
{
    public class Firethrower : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 7;
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 98;
            Projectile.aiStyle = ProjAIStyleID.Flamethrower;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 2;
        }
        /*public bool Spawned
        {
            get => Projectile.ai[0] == 1f;
            set => Projectile.ai[0] = value ? 1f : 0f;
        }*/
        /*public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            int num = (int)Utils.Remap(Projectile.localAI[0], 0.0f, 72f, 10f, 40f);
            hitbox.Inflate(num, num); 
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return projHitbox.Intersects(targetHitbox) && Collision.CanHit(Projectile.Center, 0, 0, targetHitbox.Center.ToVector2(), 0, 0);
        }*/
        public override void AI()
        {
            //Projectile.velocity *= 0.95f;
            //Projectile.position = Projectile.position - Projectile.velocity;
            Projectile.scale = 0.75f + (180 - Projectile.timeLeft) / 240f;
            Projectile.scale *= Projectile.ai[0];
            Projectile.rotation += Projectile.velocity.Length() * 0.1f * (Projectile.velocity.X > 0 ? 1 : -1);
            Projectile.frameCounter++;
            if (Projectile.frameCounter % 10 == 0)
            {
                Projectile.frame++;
                if (Projectile.frame > 6)
                    Projectile.Kill();
            }
        }
    }
}
