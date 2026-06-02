using CalamityMod;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items;
using CalamityMod.Items.Tools;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.WoB.Drop
{
    public class LargeFather : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Tools";

        public override void SetDefaults()
        {
            Item.damage = 465;
            Item.ArmorPenetration = 5;
            Item.knockBack = 0f;
            Item.useTime = 1;
            Item.useAnimation = 25;
            Item.pick = 280;
            Item.DamageType = ModContent.GetInstance<TrueMeleeNoSpeedDamageClass>();
            Item.width = 62;
            Item.height = 36;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.rare = ModContent.RarityType<BurnishedAuric>();
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.UseSound = SoundID.Item23;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<LargeFatherProj>();
            Item.shootSpeed = 40f;
            Item.tileBoost = 15;
        }

        public override void HoldItem(Player player)
        {
            player.Calamity().mouseWorldListener = true;
        }
    }
    public class LargeFatherProj : ModProjectile
    {
        public override string Texture => ModContent.GetInstance<LargeFather>().Texture;


        public static Asset<Texture2D> GlowmaskTex;

        public static Asset<Texture2D> BloomTex;

        //internal PrimitiveTrail TrailDrawer;

        public override LocalizedText DisplayName => CalamityUtils.GetItemName<LargeFather>();

        public Player Owner => Main.player[Projectile.owner];

        public ref float MoveInIntervals => ref Projectile.localAI[0];

        public ref float SpeenBeams => ref Projectile.localAI[1];

        public ref float Timer => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = ModContent.GetInstance<TrueMeleeNoSpeedDamageClass>();
        }

        public override bool ShouldUpdatePosition()
        {
            return false;
        }

        public override void AI()
        {
            Timer += 1f;
            SpeenBeams += Timer > 140f ? 1f : 1f + 2f * (float)Math.Pow(1f - Timer / 140f, 2.0);
            if (Projectile.soundDelay <= 0)
            {
                SoundEngine.PlaySound(in MarniteObliterator.UseSound, Projectile.Center);
                Projectile.soundDelay = 23;
            }

            if ((Owner.Center - Projectile.Center).Length() >= 5f)
            {
                if ((Owner.MountedCenter - Projectile.Center).Length() >= 30f)
                {
                    DelegateMethods.v3_1 = Color.Blue.ToVector3() * 0.5f;
                    Utils.PlotTileLine(Owner.MountedCenter + Owner.MountedCenter.DirectionTo(Projectile.Center) * 30f, Projectile.Center, 8f, DelegateMethods.CastLightOpen);
                }

                Lighting.AddLight(Projectile.Center, Color.Blue.ToVector3() * 0.7f);
            }

            if (MoveInIntervals > 0f)
            {
                MoveInIntervals -= 1f;
            }

            if (!Owner.channel || Owner.noItems || Owner.CCed)
            {
                Projectile.Kill();
            }
            else if (MoveInIntervals <= 0f && Main.myPlayer == Projectile.owner)
            {
                Vector2 vector = Owner.Calamity().mouseWorld - Owner.MountedCenter;
                if (Main.tile[Player.tileTargetX, Player.tileTargetY].HasTile)
                {
                    vector = new Vector2(Player.tileTargetX, Player.tileTargetY) * 16f + Vector2.One * 8f - Owner.MountedCenter;
                    MoveInIntervals = 2f;
                }

                vector = Vector2.Lerp(vector, Projectile.velocity, 0.7f);
                if (float.IsNaN(vector.X) || float.IsNaN(vector.Y))
                {
                    vector = -Vector2.UnitY;
                }

                if (vector.Length() < 50f)
                {
                    vector = vector.SafeNormalize(-Vector2.UnitY) * 50f;
                }

                int tileBoost = Owner.inventory[Owner.selectedItem].tileBoost;
                int num = (Player.tileRangeX + tileBoost - 1) * 16 + 11;
                int num2 = (Player.tileRangeY + tileBoost - 1) * 16 + 11;
                vector.X = Math.Clamp(vector.X, -num, num);
                vector.Y = Math.Clamp(vector.Y, -num2, num2);
                if (vector != Projectile.velocity)
                {
                    Projectile.netUpdate = true;
                }

                Projectile.velocity = vector;
            }

            Owner.heldProj = Projectile.whoAmI;
            Owner.ChangeDir(Math.Sign(Projectile.velocity.X));
            Owner.SetCompositeArmFront(enabled: true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() * Owner.gravDir - MathF.PI / 2f);
            Owner.SetCompositeArmBack(enabled: true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() * Owner.gravDir - MathF.PI / 2f - MathF.PI / 8f * Owner.direction);
            Owner.SetDummyItemTime(2);
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.Center = Owner.MountedCenter + Projectile.velocity;
        }

        internal Color ColorFunction(float completionRatio, Vector2 vertexPos)
        {
            float fadeOpacity = (float)Math.Sqrt(1 - completionRatio);
            return Color.DeepSkyBlue * fadeOpacity;
        }

        internal float WidthFunction(float completionRatio, Vector2 vertexPos)
        {
            return 29.4f * completionRatio;
        }

        public void DrawBeam(Texture2D beamTex, Vector2 direction, int beamIndex)
        {
            Vector2 startPos = Owner.MountedCenter + direction * 17f + direction.RotatedBy(1.5707963705062866) * (float)Math.Cos(MathF.PI * 2f * beamIndex / 3f + SpeenBeams * 0.06f) * 13f;
            float rotation = (Projectile.Center - startPos).ToRotation();
            Vector2 beamOrigin = new Vector2(beamTex.Width / 2f, beamTex.Height);
            Vector2 beamScale = new Vector2(5.4f, (startPos - Projectile.Center).Length() / beamTex.Height);
            CalamityUtils.DrawChromaticAberration(direction.RotatedBy(1.5707963705062866), 4f, delegate (Vector2 offset, Color colorMod)
            {
                Color firstColor = Color.Lerp(Color.Magenta, Color.DarkGoldenrod, 0.5f + 0.5f * (float)Math.Sin(SpeenBeams * 0.2f));
                firstColor *= 0.54f;
                firstColor = firstColor.MultiplyRGB(colorMod);

                Main.EntitySpriteDraw(beamTex, startPos + offset - Main.screenPosition, null, firstColor, rotation + MathF.PI / 2f, beamOrigin, beamScale, SpriteEffects.None);

                beamScale.X = 2.4f;

                firstColor = Color.Lerp(Color.Purple, Color.Chocolate, 0.5f + 0.5f * (float)Math.Sin(SpeenBeams * 0.2f + 1.2f));
                firstColor = firstColor.MultiplyRGB(colorMod);

                Main.EntitySpriteDraw(beamTex, startPos + offset - Main.screenPosition, null, firstColor, rotation + MathF.PI / 2f, beamOrigin, beamScale, SpriteEffects.None);
            });
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (!Projectile.active)
                return false;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);


            Vector2 normalizedVelocity = Projectile.velocity.SafeNormalize(Vector2.Zero);
            Texture2D beamTex = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/SimpleGradient").Value;

            for (int i = 0; i < 3; i++)
            {
                float beamElevation = (float)Math.Sin(MathHelper.TwoPi * i / 3f + SpeenBeams * 0.06f);
                if (beamElevation < 0)
                    DrawBeam(beamTex, normalizedVelocity, i);
            }

            if (BloomTex == null)
                BloomTex = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");
            Texture2D bloomTex = BloomTex.Value;

            Main.EntitySpriteDraw(bloomTex, Projectile.Center - Main.screenPosition, null, Color.DeepSkyBlue * 0.3f, MathHelper.PiOver2, bloomTex.Size() / 2f, 0.3f * Projectile.scale, SpriteEffects.None, 0);

            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/DoubleTrail"));
            PrimitiveRenderer.RenderTrail(new Vector2[] { Projectile.Center, Owner.MountedCenter - normalizedVelocity * 13f }, new(WidthFunction, ColorFunction, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), 30);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(9f, tex.Height / 2f);
            SpriteEffects effect = SpriteEffects.None;
            if (Owner.direction * Owner.gravDir < 0)
                effect = SpriteEffects.FlipVertically;

            Main.EntitySpriteDraw(tex, Owner.MountedCenter + normalizedVelocity * 10f - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, effect, 0);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            //Draw some bloom
            /*if (GlowmaskTex == null)
                GlowmaskTex = ModContent.Request<Texture2D>("CalamityMod/Items/Tools/MarniteObliteratorBloom");
            Texture2D glowTex = GlowmaskTex.Value;
            float bloomOpacity = (float)Math.Pow(Math.Clamp(Timer / 100f, 0f, 1f), 2) * (0.85f + (0.5f + 0.5f * (float)Math.Sin(Main.GlobalTimeWrappedHourly))) * 0.8f;
            Color bloomColor = Color.Lerp(Color.DeepSkyBlue, Color.Chocolate, 0.5f + 0.5f * (float)Math.Sin(SpeenBeams * 0.2f + 1.2f));

            Main.EntitySpriteDraw(glowTex, Owner.MountedCenter + normalizedVelocity * 10f - Main.screenPosition, null, bloomColor * bloomOpacity, Projectile.rotation, origin, Projectile.scale, effect, 0);
            */


            for (int i = 0; i < 3; i++)
            {
                float beamElevation = (float)Math.Sin(MathHelper.TwoPi * i / 3f + SpeenBeams * 0.06f);
                if (beamElevation >= 0)
                    DrawBeam(beamTex, normalizedVelocity, i);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);


            return false;
        }
        /*public override bool PreDraw(ref Color lightColor)
        {
            if (!Projectile.active)
            {
                return false;
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Vector2 vector = Projectile.velocity.SafeNormalize(Vector2.Zero);
            Texture2D value = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/SimpleGradient").Value;
            for (int i = 0; i < 3; i++)
            {
                if ((float)Math.Sin(MathF.PI * 2f * i / 3f + SpeenBeams * 0.06f) < 0f)
                {
                    DrawBeam(value, vector, i);
                }
            }

            if (BloomTex == null)
            {
                BloomTex = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");
            }

            Texture2D value2 = BloomTex.Value;
            Main.EntitySpriteDraw(value2, Projectile.Center - Main.screenPosition, null, Color.DeepSkyBlue * 0.3f, MathF.PI / 2f, value2.Size() / 2f, 0.3f * Projectile.scale, SpriteEffects.None);
            if (TrailDrawer == null)
            {
                TrailDrawer = new PrimitiveTrail(WidthFunction, ColorFunction, null, GameShaders.Misc["CalamityMod:TrailStreak"]);
            }

            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/DoubleTrail"));
            TrailDrawer.Draw(new Vector2[2]
            {
                Projectile.Center,
                Owner.MountedCenter - vector * 13f
            }, -Main.screenPosition, 30);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Texture2D value3 = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(9f, value3.Height / 2f);
            SpriteEffects effects = SpriteEffects.None;
            if (Owner.direction * Owner.gravDir < 0f)
            {
                effects = SpriteEffects.FlipVertically;
            }

            Main.EntitySpriteDraw(value3, Owner.MountedCenter + vector * 10f - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, effects);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            for (int j = 0; j < 3; j++)
            {
                if ((float)Math.Sin(MathF.PI * 2f * j / 3f + SpeenBeams * 0.06f) >= 0f)
                {
                    DrawBeam(value, vector, j);
                }
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }*/
    }
}
