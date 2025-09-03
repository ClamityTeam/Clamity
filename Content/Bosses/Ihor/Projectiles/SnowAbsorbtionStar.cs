using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Ihor.Projectiles
{
    public class SnowAbsorbtionStar : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/StarProj";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 18;
            Projectile.aiStyle = -1;
            AIType = -1;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
        }

        public override void AI()
        {
            if (Projectile.ai[0] >= 10)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<IhorSnowball>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.whoAmI, Main.rand.NextFloat(MathHelper.TwoPi));
            }
        }


        public override bool CanHitPlayer(Player target)
        {
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D t = ModContent.Request<Texture2D>(Texture).Value;


            Main.spriteBatch.Draw(t, Projectile.Center - Main.screenPosition, null, Color.Cyan, 0, t.Size() / 2, new Vector2(1f, 0.5f), SpriteEffects.None, 0);
            Main.spriteBatch.Draw(t, Projectile.Center - Main.screenPosition, null, Color.Cyan, MathHelper.PiOver2, t.Size() / 2, new Vector2(3f, 0.5f), SpriteEffects.None, 0);

            return false;
        }
    }
}
