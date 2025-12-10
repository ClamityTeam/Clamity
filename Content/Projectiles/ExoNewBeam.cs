using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace Clamity.Content.Projectiles
{
    public class ExoNewBeam : Exobeam, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => ModContent.GetInstance<Exobeam>().Texture;
        /*public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Main.rand.NextBool(2))
            {
                Color newColor = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.9f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f) + Projectile.velocity, 267, Projectile.velocity * -2.6f, 0, newColor);
                dust.scale = 0.3f;
                dust.fadeIn = Main.rand.NextFloat() * 1.2f;
                dust.noGravity = true;
            }

            Projectile.scale = Utils.GetLerpValue(0f, 0.1f, Projectile.timeLeft / 600f, clamped: true);
            if (Projectile.FinalExtraUpdate())
            {
                Time += 1f;
            }
        }*/
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(in Exoblade.BeamHitSound, target.Center);
            if (Projectile.ai[2] == 1)
            {
                int index = Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center, Vector2.Zero, ModContent.ProjectileType<TerratomereExplosion>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner, 0f, 0f, 0f);
                Main.projectile[index].DamageType = Projectile.DamageType;
            }

            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 300);
        }
    }
    public class ExoNewExplosion : ModProjectile, /*IAdditiveDrawer,*/ ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 7;
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 520;
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 150;
            Projectile.MaxUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = Projectile.MaxUpdates * 14;
            Projectile.scale = 0.2f;
            Projectile.hide = true;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                SoundEngine.PlaySound(in SubsumingVortex.ExplosionSound, Projectile.Center);
                Projectile.localAI[0] = 1f;
            }

            Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 1.5f);
            Projectile.frameCounter++;
            if (Projectile.frameCounter % 20 == 19)
            {
                Projectile.frame++;
            }

            if (Projectile.frame >= 7)
            {
                Projectile.Kill();
            }

            Projectile.scale *= 1.013f;
            Projectile.Opacity = Utils.GetLerpValue(5f, 36f, Projectile.timeLeft, clamped: true);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D value = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D value2 = ModContent.Request<Texture2D>("CalamityMod/Skies/XerocLight").Value;
            Rectangle rectangle = value.Frame(1, 7, Projectile.frame, 0);
            //Rectangle rectangle = new Rectangle(value.Width * Projectile.frame, 0, value.Width, value.Height / Main.projFrames[Type]);
            Vector2 vector = Projectile.Center - Main.screenPosition;
            Vector2 origin = rectangle.Size() * 0.5f;
            for (int i = 0; i < 36; i++)
            {
                Vector2 position = vector + (MathF.PI * 2f * i / 36f + Main.GlobalTimeWrappedHourly * 5f).ToRotationVector2() * Projectile.scale * 12f;
                Color value3 = CalamityUtils.MulticolorLerp(Projectile.timeLeft / 144f, new Color(210, 234, 110), new Color(141, 162, 67));
                value3 = Color.Lerp(value3, Color.White, 0.4f) * Projectile.Opacity * 0.184f;
                Main.spriteBatch.Draw(value2, position, null, value3, 0f, value2.Size() * 0.5f, Projectile.scale * 1.32f, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.Draw(value, vector, rectangle, Color.White, 0f, origin, 1.6f, SpriteEffects.None, 0f);
            return false;
        }
        /*public void AdditiveDraw(SpriteBatch spriteBatch)
        {
            Texture2D value = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D value2 = ModContent.Request<Texture2D>("CalamityMod/Skies/XerocLight").Value;
            //Rectangle rectangle = value.Frame(1, 7, Projectile.frame, 0);
            Rectangle rectangle = new Rectangle(value.Width * Projectile.frame, 0, value.Width, value.Height);
            Vector2 vector = Projectile.Center - Main.screenPosition;
            Vector2 origin = rectangle.Size() * 0.5f;
            for (int i = 0; i < 36; i++)
            {
                Vector2 position = vector + (MathF.PI * 2f * i / 36f + Main.GlobalTimeWrappedHourly * 5f).ToRotationVector2() * Projectile.scale * 12f;
                Color value3 = CalamityUtils.MulticolorLerp(Projectile.timeLeft / 144f, new Color(210, 234, 110), new Color(141, 162, 67));
                value3 = Color.Lerp(value3, Color.White, 0.4f) * Projectile.Opacity * 0.184f;
                Main.spriteBatch.Draw(value2, position, null, value3, 0f, value2.Size() * 0.5f, Projectile.scale * 1.32f, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.Draw(value, vector, rectangle, Color.White, 0f, origin, 1.6f, SpriteEffects.None, 0f);
        }*/
    }
}
