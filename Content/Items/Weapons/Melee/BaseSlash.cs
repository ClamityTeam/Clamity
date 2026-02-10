using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using Terraria;

namespace Clamity.Content.Items.Weapons.Melee
{
    public abstract class BaseSlash : ExobeamSlash
    {
        public override string Texture => "Clamity/Content/Items/Weapons/Melee/BaseSlash";
        public virtual float Scale => 1f;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = Projectile.height = 24;
            Projectile.scale = Scale;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.Opacity = Projectile.timeLeft / 35f;
            if (Projectile.timeLeft == 34)
            {
                Particle spark2 = new GlowSparkParticle(Projectile.Center, Projectile.velocity, false, 12, Scale * 0.1f, GetNewColor * 0.7f, new Vector2(2, 0.5f), true);
                GeneralParticleHandler.SpawnParticle(spark2);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { }
        public override void OnHitPlayer(Player target, Player.HurtInfo info) { }
        public virtual Color FirstColor => Color.White;
        public virtual Color SecondColor => Color.Gray;
        public Color GetNewColor => Color.Lerp(FirstColor, SecondColor, Projectile.identity / 7f % 1f) * Projectile.Opacity;
        public override Color? GetAlpha(Color lightColor)
        {
            return GetNewColor;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
            {
                return true;
            }

            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + new Vector2(200, 0).RotatedBy(Projectile.rotation) * Scale, Projectile.Size.Length() * Projectile.scale / 10f, ref collisionPoint)
                || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center - new Vector2(200, 0).RotatedBy(Projectile.rotation) * Scale, Projectile.Size.Length() * Projectile.scale / 10f, ref collisionPoint);
        }
    }
}
