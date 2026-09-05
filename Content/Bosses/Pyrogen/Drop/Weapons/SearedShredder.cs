using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.BaseItems;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Pyrogen.Drop.Weapons
{
    public class SearedShredder : CustomUseProjItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetStaticDefaults()
        {
            if (!ModLoader.TryGetMod("Redemption", out var redemption))
                return;
            redemption.Call("addElementItem", 2, Type);
        }
        public override void SetDefaults()
        {
            Item.width = 74;
            Item.height = 68;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;

            Item.useTime = Item.useAnimation = 60;
            Item.useStyle = ItemUseStyleID.Shoot;
            //Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;

            Item.shoot = ModContent.ProjectileType<SearedShredderHoldout>();
            //Item.shootSpeed = 10f;

            Item.damage = 62;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 7.5f;

            Item.channel = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
        }
        public override bool MeleePrefix() => true;
    }
    public class SearedShredderHoldout : BaseCustomUseStyleProjectile, ILocalizedModType
    {
        public override int AssignedItemID => ModContent.ItemType<SearedShredder>();


        public override LocalizedText DisplayName => CalamityUtils.GetItemName<SearedShredder>();
        public override string Texture => "Clamity/Content/Bosses/Pyrogen/Drop/Weapons/SearedShredder";
        public int size = 74 + 15;
        public override float HitboxOutset => size * 0.85f;
        public override Vector2 HitboxSize => new Vector2(size, size);
        public override Vector2 SpriteOrigin => new(0, size - 15);
        public override float HitboxRotationOffset => MathHelper.ToRadians(-45);

        public Vector2 mousePos;
        public Vector2 aimVel;
        public bool doSwing = true;
        public bool postSwing = false;
        public float fadeIn = 0;
        public int useAnim;
        public int swingCount;
        public bool finalFlip = false;
        public bool playSwingSound = true;
        public int armoredHits = 0;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();
        }
        public override void WhenSpawned()
        {
            Projectile.knockBack = 0;
            Projectile.ai[1] = 1;

            // 14NOV2024: Ozzatron: clamped mouse position unnecessary, as Grand Guardian has no projectiles
            mousePos = Owner.Calamity().mouseWorld;
            aimVel = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitX) * 65;
            useAnim = Owner.itemAnimationMax;

            if (mousePos.X < Owner.Center.X) Owner.direction = -1;
            else Owner.direction = 1;

            FlipAsSword = Owner.direction == 1 ? true : false;
        }

        public override void UseStyle()
        {
            AnimationProgress = Animation % useAnim;
            DrawUnconditionally = false;

            if (CanHit || postSwing)
                mousePos = Owner.Center - aimVel;
            else
            {
                mousePos = Owner.Calamity().mouseWorld;
            }

            if (CanHit)
                fadeIn = MathHelper.Lerp(fadeIn, 1, 0.3f);
            else
                fadeIn = MathHelper.Lerp(fadeIn, 0, 0.25f);


            if (!doSwing)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                    Projectile.localNPCImmunity[i] = 0;

                playSwingSound = true;
                Projectile.numHits = 0;
                mousePos = Owner.Calamity().mouseWorld;
                aimVel = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitX) * 65;
                CanHit = false;
                if (mousePos.X < Owner.Center.X) Owner.direction = -1;
                else Owner.direction = 1;
                FlipAsSword = Owner.direction == -1 ? true : false;
                if (Projectile.ai[1] == 1) FlipAsSword = !FlipAsSword;

                doSwing = true;
                finalFlip = false;
                armoredHits = 0;
            }
            else
            {
                if (!CanHit && !postSwing)
                {
                    if (mousePos.X < Owner.Center.X) Owner.direction = -1;
                    else Owner.direction = 1;
                }
                else
                {
                    if ((Owner.Center - aimVel).X < Owner.Center.X) Owner.direction = -1;
                    else Owner.direction = 1;
                }


                Projectile.rotation = Projectile.rotation.AngleLerp(Owner.AngleTo(mousePos) + MathHelper.ToRadians(45f), 0.1f);

                if (AnimationProgress < (useAnim / 1.2f))
                {
                    aimVel = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitX) * 65;
                    CanHit = false;
                    postSwing = false;
                    if (AnimationProgress == 0)
                    {
                        Animation = 0;
                        doSwing = false;
                        Projectile.ai[1] = -Projectile.ai[1];
                    }
                    RotationOffset = MathHelper.Lerp(RotationOffset, MathHelper.ToRadians(120f * Projectile.ai[1] * Owner.direction * (1 + (Utils.GetLerpValue(useAnim * 0.1f, useAnim * 0.25f, Animation, true)) * 0.35f)), 0.2f);
                }
                else
                {
                    if (!finalFlip)
                    {
                        FlipAsSword = Owner.direction < 0 ? true : false;
                        if (Projectile.ai[1] == 1) FlipAsSword = !FlipAsSword;
                    }

                    float time = (AnimationProgress) - (useAnim / 3);
                    float timeMax = useAnim - (useAnim / 3);

                    if (time >= (int)(timeMax * 0.3f) && playSwingSound)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing with { Volume = 1f, Pitch = -0.2f }, Projectile.Center);
                        SoundEngine.PlaySound(SoundID.DD2_GhastlyGlaivePierce with { Volume = 1f, Pitch = -0.5f }, Projectile.Center);
                        swingCount++;
                        playSwingSound = false;
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Owner.Center, (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.Zero) * 0.01f, ModContent.ProjectileType<SearedShredderProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Owner.direction * Owner.gravDir, useAnim);
                    }
                    if (time > (int)(timeMax * 0.4f) && time < (int)(timeMax))
                    {
                        CanHit = true;

                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 particleVel = new Vector2(0, 10 * -Projectile.ai[1] * Owner.direction).RotatedBy(FinalRotation + MathHelper.ToRadians(-45));
                            Vector2 particlePos = Owner.Center + (new Vector2(Main.rand.Next(30, 70), 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45))) * Projectile.scale;
                            GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(particlePos, -particleVel.RotatedByRandom(0.2f), false, 19, Main.rand.NextFloat(0.5f, 1f) * Projectile.scale, Main.rand.NextBool(4) ? Color.Orange : Color.Yellow));
                        }
                    }
                    else
                        CanHit = false;

                    RotationOffset = MathHelper.Lerp(RotationOffset, MathHelper.ToRadians(MathHelper.Lerp(150f * Projectile.ai[1] * Owner.direction, 120f * -Projectile.ai[1] * Owner.direction, CalamityUtils.ExpInOutEasing(time / timeMax, 1))),
                        0.2f);

                    if (time >= timeMax)
                        doSwing = false;
                    if (time < (int)(timeMax * 0.7f))
                        postSwing = true;

                    if (CanHit)
                    {
                        /*for (int i = 0; i < 3; i++)
                        {
                            bool color = Main.rand.NextBool();
                            GenericSparkle sparker = new GenericSparkle(Owner.Center + (new Vector2(198 * Projectile.scale, 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)).RotatedByRandom(0.3f)), Vector2.Zero, color ? Color.Cyan : Color.DarkOrchid, color ? Color.DarkOrchid : Color.Cyan, Main.rand.NextFloat(0.4f, 0.6f) * Projectile.scale, 10, Main.rand.NextFloat(-0.1f, 0.1f), 2.68f);
                            GeneralParticleHandler.SpawnParticle(sparker);

                            Dust dust2 = Dust.NewDustPerfect(Owner.Center + (new Vector2(180 * Projectile.scale, 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)).RotatedByRandom(0.3f)), DustID.FireworksRGB, Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 2));
                            dust2.scale = Main.rand.NextFloat(0.55f, 0.85f);
                            dust2.noGravity = true;
                            dust2.color = Main.rand.NextBool() ? Color.Cyan : Color.DarkOrchid;
                        }*/
                    }
                }
            }

            ArmRotationOffset = MathHelper.ToRadians(-140f);
            ArmRotationOffsetBack = MathHelper.ToRadians(-140f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if ((target.life <= 0 && target.realLife == -1) && Projectile.numHits > 0)
                Projectile.numHits -= 1;
            if (damageDone <= 2)
                armoredHits++;

            Vector2 launchVel = Utils.DirectionTo(Owner.Center, Owner.Calamity().mouseWorld);
            target.MoveNPC(launchVel, 8, true, Owner);

            /*SoundStyle fire = new("CalamityMod/Sounds/Item/WulfrumKnifeTileHit2");
            SoundEngine.PlaySound(fire with { Volume = 0.65f, Pitch = -0.6f }, Projectile.Center);
            SoundStyle fire2 = new("CalamityMod/Sounds/Item/ExobladeBeamSlash");
            SoundEngine.PlaySound(fire2 with { Volume = 0.35f, Pitch = Main.rand.NextFloat(0.5f, 0.7f) }, Projectile.Center);

            int heal = (int)(MathHelper.Clamp(5 - Projectile.numHits * 3, 1, 5));
            if (Projectile.numHits < 5)
            {
                Owner.DoLifestealDirect(target, heal, 0.75f);
            }

            int points = 4;
            float radians = MathHelper.TwoPi / points;
            Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f)).RotatedByRandom(100);
            Color useColor = Main.rand.NextBool() ? Color.Cyan : Color.DarkOrchid;
            for (int k = 0; k < points; k++)
            {
                Vector2 velocity = spinningPoint.RotatedBy(radians * k).RotatedBy(-0.45f);
                Particle spark = new GlowSparkParticle((target.Center + velocity * 7.5f), velocity * 0.5f, false, 11, 0.07f, useColor, new Vector2(1f, 0.4f), true, false);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            for (int i = 0; i < MathHelper.Clamp(10 - Projectile.numHits * 2, 2, 10); i++)
            {
                Dust dust2 = Dust.NewDustPerfect(target.Center, DustID.FireworksRGB, new Vector2(12, 12).RotatedByRandom(100) * Main.rand.NextFloat(0.05f, 0.7f));
                dust2.scale = Main.rand.NextFloat(0.55f, 0.85f);
                dust2.noGravity = true;
                dust2.color = Main.rand.NextBool() ? Color.Cyan : Color.DarkOrchid;
            }*/
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float minMult = 0.3f;
            int hitsToMinMult = 5;
            float damageMult = Utils.Remap(Projectile.numHits - armoredHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // Only draw the projectile if the projectile's owner is currently using the item this projectile is attached to.
            if ((useAnim > 0 || DrawUnconditionally) && Owner.ItemAnimationActive)
            {
                Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
                Asset<Texture2D> swoosh = ModContent.Request<Texture2D>("CalamityMod/Particles/VerticalSmearLarge");

                float r = FlipAsSword ? MathHelper.ToRadians(90) : 0f;

                if (swingCount > 0)
                    Main.EntitySpriteDraw(swoosh.Value, Projectile.Center - Main.screenPosition + new Vector2(0, Owner.gfxOffY), null, Color.Orange with { A = 0 } * fadeIn * 0.9f, (FinalRotation + MathHelper.ToRadians(45)) + MathHelper.ToRadians(swingCount % 2 != 0 ? -70 : 70) * -Owner.direction, swoosh.Size() * 0.5f, Projectile.scale * 1.55f / 4, SpriteEffects.None);

                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + new Vector2(0, Owner.gfxOffY), tex.Frame(1, FrameCount, 0, Frame), lightColor, Projectile.rotation + RotationOffset + r, FlipAsSword ? new Vector2(tex.Width() - SpriteOrigin.X, SpriteOrigin.Y) : SpriteOrigin, Projectile.scale, spriteEffects != SpriteEffects.None ? spriteEffects : (FlipAsSword ? SpriteEffects.FlipHorizontally : SpriteEffects.None));

            }
            return false;
        }
        public override void ResetStyle()
        {
        }
    }
    public class SearedShredderProjectile : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        internal const float FadeInTime = 30f;
        internal const float FadeOutTime = 30f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.NoMeleeSpeedVelocityScaling[Type] = true;
            Main.projFrames[Type] = 4;

            if (ModLoader.TryGetMod("Redemption", out var redemption))
                redemption.Call("addElementProj", 2, Type);
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 150;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1; // Blazing blades and hyper blades hit four times, sunlight blades hit ten times.
            Projectile.ignoreWater = true;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 8;
            Projectile.timeLeft = 1000;
            Projectile.noEnchantmentVisuals = true;
            Projectile.scale = 1.5f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
        }
        public override void SendExtraAI(BinaryWriter writer)
        {

        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {

        }
        public override void OnSpawn(IEntitySource source)
        {
            //Projectile.ai[2] = (Main.MouseWorld - Main.player[Projectile.owner].MountedCenter).Length();
            Projectile.ai[2] = 250;
        }
        public override void AI()
        {
            float fullyVisibleDuration = Projectile.ai[1] - 2.5f - 2.5f;

            float timeBeforeFadeOut = fullyVisibleDuration + 2.5f;
            //float projectileDuration = timeBeforeFadeOut + 5f;

            Player Owner = Main.player[Projectile.owner];

            Projectile.Center = Owner.Center + new Vector2(0, Owner.gfxOffY) +
                Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.ai[2] * MathF.Sin(MathF.PI * Projectile.timeLeft / Projectile.ai[1]);
            //Projectile.velocity = Owner.velocity;
            Projectile.velocity = Projectile.velocity.RotateDirectionTowards((Main.MouseWorld - Owner.MountedCenter).ToRotation(), 0.04f / Projectile.ai[2] * 200f);
            if (Projectile.timeLeft > Projectile.ai[1]) Projectile.timeLeft = (int)Projectile.ai[1];

            Projectile.localAI[0] += 1f;
            Projectile.Opacity = Utils.Remap(Projectile.localAI[0], 0f, fullyVisibleDuration, 0f, 1f) * Utils.Remap(Projectile.localAI[0], timeBeforeFadeOut, Projectile.ai[1], 1f, 0f);
            //Main.NewText(Projectile.Opacity);
            if (Projectile.localAI[0] >= Projectile.ai[1])
            {
                Projectile.localAI[1] = 1f;
                Projectile.Kill();
                return;
            }

            Projectile.localAI[1] += 1f;
            Projectile.rotation += Projectile.ai[0] * MathHelper.TwoPi * (4f + Projectile.Opacity * 4f) / 90f * MathF.Sin(MathF.PI * Projectile.localAI[0] / Projectile.ai[1]);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D asset = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Microsoft.Xna.Framework.Rectangle rectangle = asset.Frame(1, 4);
            Vector2 origin = rectangle.Size() / 2f;
            float num = Projectile.scale * 1.1f;
            SpriteEffects effects = ((!(Projectile.ai[0] >= 0f)) ? SpriteEffects.FlipVertically : SpriteEffects.None);
            float num2 = 0.975f;
            float fromValue = Lighting.GetColor(Projectile.Center.ToTileCoordinates()).ToVector3().Length() / (float)Math.Sqrt(3D);
            fromValue = Utils.Remap(fromValue, 0.2f, 1f, 0f, 1f);
            float num3 = MathHelper.Min(0.15f + fromValue * 0.85f, Utils.Remap(Projectile.localAI[0], 0, Projectile.ai[1] * 4, 1f, 0f));
            //Main.NewText(num3);
            //float num3 = Utils.Remap(Projectile.localAI[0], 0, Projectile.ai[1], 1f, 0f);
            float num4 = 2f;
            for (float num5 = num4; num5 >= 0f; num5 -= 1f)
            {
                if (!(Projectile.oldPos[(int)num5] == Vector2.Zero))
                {
                    Vector2 vectorScale = Projectile.Center - Projectile.velocity * 0.5f * num5;
                    float num6 = Projectile.oldRot[(int)num5] + Projectile.ai[0] * MathHelper.TwoPi * 0.1f * (0f - num5);
                    Vector2 position = vectorScale - Main.screenPosition;
                    float num7 = 1f - num5 / num4;
                    float num8 = Projectile.Opacity * num7 * num7 * 0.85f;
                    float amount = Projectile.Opacity * Projectile.Opacity;

                    Color colorOne = Color.Lerp(new Color(112, 0, 0, 120),
                        new Color(241, 128, 14, 120), amount);

                    Main.spriteBatch.Draw(asset, position, rectangle, colorOne * num3 * num8, num6 + Projectile.ai[0] * MathHelper.PiOver4 * -1f, origin, num * num2, effects, 0f);

                    Color colorTwo = Color.Lerp(new Color(196, 43, 18),
                        new Color(250, 226, 65), amount);

                    Color color3 = Color.White * num8 * 0.5f;
                    color3.A = (byte)((float)(int)color3.A * (1f - num3));

                    Color color4 = color3 * num3 * 0.5f;
                    color4.B = (byte)((float)(int)color4.B * num3);
                    color4.R = (byte)((float)(int)color4.R * (0.25f + num3 * 0.75f));



                    float num9 = 3f;
                    for (float num10 = -MathHelper.TwoPi + MathHelper.TwoPi / num9; num10 < 0f; num10 += MathHelper.TwoPi / num9)
                    {
                        float num11 = Utils.Remap(num10, -MathHelper.TwoPi, 0f, 0f, 0.5f);
                        Main.spriteBatch.Draw(asset, position, rectangle, color4 * 0.15f * num11, num6 + Projectile.ai[0] * 0.01f + num10, origin, num, effects, 0f);

                        Main.spriteBatch.Draw(asset, position, rectangle, Color.Lerp(new Color(87, 81, 173),
                            new Color(126, 128, 196), amount) * fromValue * num8 * num11,
                            num6 + num10, origin, num * 0.8f, effects, 0f);

                        Main.spriteBatch.Draw(asset, position, rectangle, colorTwo * fromValue * num8 * MathHelper.Lerp(0.05f, 0.4f, fromValue) * num11, num6 + num10, origin, num * num2, effects, 0f);

                        //Main.spriteBatch.Draw(asset, position, rectangle, colorTwo * fromValue * num8 * MathHelper.Lerp(0.05f, 0.4f, fromValue) * num11, MathHelper.Pi + num6 + num10, origin, num * num2, effects, 0f);

                        Main.spriteBatch.Draw(asset, position, asset.Frame(1, 4, 0, 3),
                            new Color(150, 255, 200) * MathHelper.Lerp(0.05f, 0.5f, fromValue) * num8 * num11,
                            num6 + num10, origin, num, effects, 0f);
                    }

                    Main.spriteBatch.Draw(asset, position, rectangle, color4 * 0.15f, num6 + Projectile.ai[0] * 0.01f, origin, num, effects, 0f);

                    Main.spriteBatch.Draw(asset, position, rectangle, Color.Lerp(new Color(30, 80, 160),
                        new Color(255, 255, 0), amount) * num3 * num8, num6, origin, num * 0.8f, effects, 0f);

                    Main.spriteBatch.Draw(asset, position, rectangle, Color.Lerp(new Color(30, 80, 160),
                        new Color(255, 255, 0), amount) * num3 * num8, MathHelper.Pi + num6, origin, num * 0.8f, effects, 0f);

                    Main.spriteBatch.Draw(asset, position, rectangle, colorTwo * fromValue * num8 * MathHelper.Lerp(0.05f, 0.4f, num3), num6, origin, num * num2, effects, 0f);

                    //Main.spriteBatch.Draw(asset, position, rectangle, colorTwo * fromValue * num8 * MathHelper.Lerp(0.05f, 0.4f, num3), MathHelper.Pi + num6, origin, num * num2, effects, 0f);

                    Main.spriteBatch.Draw(asset, position, asset.Frame(1, 4, 0, 3),
                        new Color(255, 255, 75) * MathHelper.Lerp(0.05f, 0.5f, num3) * num8,
                        num6, origin, num, effects, 0f);
                }
            }

            float num12 = 1f - Projectile.localAI[0] * 1f / 80f;
            if (num12 < 0.5f)
                num12 = 0.5f;

            float num13 = MathHelper.Min(num3, MathHelper.Lerp(1f, fromValue, Utils.Remap(Projectile.localAI[0], 0f, 80f, 0f, 1f)));

            Texture2D value = TextureAssets.Extra[ExtrasID.SharpTears].Value;
            SpriteEffects dir = SpriteEffects.None;
            Vector2 drawpos = Projectile.Center - Main.screenPosition + (Projectile.rotation + (MathHelper.Pi / (20f / 3f)) * Projectile.ai[0]).ToRotationVector2() * ((float)asset.Width * 0.5f - 4f) * num * num12;
            Color drawColor = new Color(255, 255, 255, 0) * Projectile.Opacity * 0.5f * num13;
            Color shineColor = new Color(255, 255, 50) * num13;
            Color color = shineColor * Projectile.Opacity * 0.5f;
            color.A = 0;
            float flareCounter = Projectile.Opacity;
            float fadeInStart = 0f;
            float fadeInEnd = 1f;
            float fadeOutStart = 1f;
            float fadeOutEnd = 2f;
            float rotation = MathHelper.PiOver4;
            Vector2 scale = new Vector2(2f, 2f);
            Vector2 fatness = Vector2.One;
            Vector2 origin2 = value.Size() / 2f;
            Color color2 = drawColor * 0.5f;
            float num14 = Utils.GetLerpValue(fadeInStart, fadeInEnd, flareCounter, clamped: true) * Utils.GetLerpValue(fadeOutEnd, fadeOutStart, flareCounter, clamped: true);
            Vector2 vector = new Vector2(fatness.X * 0.5f, scale.X) * num14;
            Vector2 vector2 = new Vector2(fatness.Y * 0.5f, scale.Y) * num14;
            color *= num14;
            color2 *= num14;
            Main.EntitySpriteDraw(value, drawpos, null, color, MathHelper.PiOver2 + rotation, origin2, vector, dir);
            Main.EntitySpriteDraw(value, drawpos, null, color, 0f + rotation, origin2, vector2, dir);
            Main.EntitySpriteDraw(value, drawpos, null, color2, MathHelper.PiOver2 + rotation, origin2, vector * 0.6f, dir);
            Main.EntitySpriteDraw(value, drawpos, null, color2, 0f + rotation, origin2, vector2 * 0.6f, dir);

            return false;
        }
    }
}
