using Terraria;

namespace Clamity.Content.Bosses.Ihor.Projectiles
{
    public class IhorSpiralIcicles : BaseIhorIcicle
    {
        public override void BaseAI()
        {
            //Projectile.velocity *= 1.02f;
            //Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.timeLeft > 500)
            {
                if (Projectile.velocity.Length() > 0.1f)
                    Projectile.velocity *= 0.95f;
            }
            else
            {
                //Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[0]);
                Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.rotation - Projectile.velocity.ToRotation());
                if (Projectile.velocity.Length() < 15f)
                    Projectile.velocity *= 1.05f;
            }
            Projectile.rotation += Projectile.ai[0];
            Projectile.ai[0] *= 0.99f;
        }
    }
}
