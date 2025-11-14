using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stubble.Core.Settings;
using System;
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
            if (++Projectile.ai[0] >= 10)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<IhorSnowball>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.whoAmI, Main.rand.NextFloat(MathHelper.TwoPi));
                Projectile.ai[0] = 0;
            }

            int playerTracker = Player.FindClosest(Projectile.Center, 1, 1);
            Player player = Main.player[playerTracker];

            Projectile.velocity = Projectile.Center.DirectionTo(player.Center) * Projectile.velocity.Length();
            if (Projectile.velocity.Length() < 1)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero);
            else
                Projectile.velocity *= 0.95f;
        }


        public override bool CanHitPlayer(Player target)
        {
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D t = ModContent.Request<Texture2D>(Texture).Value;

            Texture2D ball = null; float ballScale = 1f;
            switch (Projectile.timeLeft / 200)
            {
                case 0:
                    ball = ModContent.Request<Texture2D>(IhorTextures.GiantSnowball).Value;
                    ballScale = Projectile.timeLeft / 200f;
                    break;
                default:

                    break;
            }
            if (ball != null)
            {
                Main.spriteBatch.Draw(ball, Projectile.Center - Main.screenPosition, null, lightColor, 0, ball.Size() / 2f, new Vector2(1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2) / 8, 1f - (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2) / 8) * ballScale, SpriteEffects.None, 0);
            }


            Main.spriteBatch.Draw(t, Projectile.Center - Main.screenPosition, null, Color.Cyan, 0, t.Size() / 2, new Vector2(0.5f, 3f * (1.25f + (float)Math.Sin(Main.GlobalTimeWrappedHourly) / 4)), SpriteEffects.None, 0);
            Main.spriteBatch.Draw(t, Projectile.Center - Main.screenPosition, null, Color.Cyan, MathHelper.PiOver2, t.Size() / 2, new Vector2(3f * (1.25f - (float)Math.Sin(Main.GlobalTimeWrappedHourly) / 4), 0.5f), SpriteEffects.None, 0);

            return false;
        }
    }
}
