using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.TheForbiddenLantern.Projectiles
{
    public class WaterRayLight : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/StarProj";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 1;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;

        }
        //public int MaxTimeLeft = 600;
        public static int RotatingTime => 45;
        public static int MaxRandomDelay => 60;
        public static int TotalProjectileCount => 8;
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.timeLeft += Main.rand.Next(0, MaxRandomDelay);

        }
        public override void AI()
        {
            NPC lantern = Main.npc[(int)Projectile.ai[0]];

            Projectile.scale = 1f - MathHelper.Clamp(Projectile.timeLeft / (float)RotatingTime, 0, 1);
            float num1 = Projectile.timeLeft > (90 - RotatingTime) ? MathF.Sin(Projectile.scale / 2 * MathHelper.Pi) : 1f;
            Projectile.Center = lantern.Center + Vector2.UnitX.RotatedBy(MathHelper.TwoPi / TotalProjectileCount * Projectile.ai[1] + Main.GlobalTimeWrappedHourly) * 150f * Projectile.scale;


        }
        public override void OnKill(int timeLeft)
        {
            Player target = Main.player[TheForbiddenLantern.NPCs.TheForbiddenLantern.Myself.target];
            Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Projectile.Center.SafeDirectionTo(target.Center) * 10, ModContent.ProjectileType<WaterRay>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D t = ModContent.Request<Texture2D>(Texture).Value;

            Main.spriteBatch.Draw(t, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, t.Size()/2, new Vector2(1f, 2f) * Projectile.scale, SpriteEffects.None, 1);
            Main.spriteBatch.Draw(t, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation + MathHelper.PiOver2, t.Size()/2, new Vector2(1f, 2f) * Projectile.scale, SpriteEffects.None, 1);
            
            Main.spriteBatch.Draw(t, Projectile.Center - Main.screenPosition, null, Color.LightCyan, Projectile.rotation, t.Size()/2, new Vector2(1f, 2f) * Projectile.scale * .8f, SpriteEffects.None, 1);
            Main.spriteBatch.Draw(t, Projectile.Center - Main.screenPosition, null, Color.LightCyan, Projectile.rotation + MathHelper.PiOver2, t.Size()/2, new Vector2(1f, 2f) * Projectile.scale * .8f, SpriteEffects.None, 1);

            return false;
        }
    }
}
