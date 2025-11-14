using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Ihor.Projectiles
{
    public class IhorIcicleHomingIThink : BaseIhorIcicle
    {
        public const int PreFlyTime = 60;
        public override bool CanCreateParticles => Projectile.timeLeft < MaxTimeLeft - PreFlyTime;
        public override void BaseAI()
        {
            int playerTracker = Player.FindClosest(Projectile.Center, 1, 1);
            Player player = Main.player[playerTracker];

            if (Projectile.timeLeft == MaxTimeLeft - PreFlyTime)
            {
                Projectile.velocity = new Vector2(1, 0).RotatedBy(Projectile.rotation);
            }
            else if (Projectile.timeLeft > MaxTimeLeft - PreFlyTime)
            {
                Projectile.rotation = Projectile.SafeDirectionTo(player.Center).ToRotation();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects effects = SpriteEffects.None;
            Texture2D t = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomLine").Value;
            float progress = (Projectile.timeLeft - MaxTimeLeft + PreFlyTime ) / PreFlyTime;
            Color color3 = Color.Lerp(Color.DarkBlue, Color.White, progress);
            Main.spriteBatch.Draw(t,
                             Projectile.Center - Main.screenPosition,
                             null,
                             color3,
                             Projectile.rotation + MathF.PI / 2f,
                             new Vector2((float)t.Width / 2f, t.Height),
                             new Vector2(0.5f * progress, 4200f),
                             effects,
                             0f);

            return true;
        }
    }
}
