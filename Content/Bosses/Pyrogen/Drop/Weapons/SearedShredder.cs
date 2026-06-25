using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Pyrogen.Drop.Weapons
{
    public class SearedShredder : ModItem, ILocalizedModType, IModType
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

            Item.useTime = Item.useAnimation = 19;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;

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
        public override int AssignedItemID => ModContent.ItemType<GrandGuardian>();


        public override LocalizedText DisplayName => CalamityUtils.GetItemName<GrandGuardian>();
        public override string Texture => "Clamity/Content/Bosses/Pyrogen/Drop/Weapons/SearedShredder";
        public int size = 130 + 15;
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

            FlipAsSword = Owner.direction == -1 ? true : false;
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
                    }

                    float time = (AnimationProgress) - (useAnim / 3);
                    float timeMax = useAnim - (useAnim / 3);

                    if (time >= (int)(timeMax * 0.3f) && playSwingSound)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing with { Volume = 1f, Pitch = -0.2f }, Projectile.Center);
                        SoundEngine.PlaySound(SoundID.DD2_GhastlyGlaivePierce with { Volume = 1f, Pitch = -0.5f }, Projectile.Center);
                        swingCount++;
                        playSwingSound = false;
                    }
                    if (time > (int)(timeMax * 0.4f) && time < (int)(timeMax))
                    {
                        CanHit = true;

                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 particleVel = new Vector2(0, 10 * -Projectile.ai[1] * Owner.direction).RotatedBy(FinalRotation + MathHelper.ToRadians(-45));
                            Vector2 particlePos = Owner.Center + (new Vector2(Main.rand.Next(30, 170), 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45))) * Projectile.scale;
                            GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(particlePos, -particleVel.RotatedByRandom(0.2f), false, 19, Main.rand.NextFloat(0.5f, 1f) * Projectile.scale, Main.rand.NextBool(4) ? Color.DarkOrchid : Color.DodgerBlue));
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
                        for (int i = 0; i < 3; i++)
                        {
                            bool color = Main.rand.NextBool();
                            GenericSparkle sparker = new GenericSparkle(Owner.Center + (new Vector2(198 * Projectile.scale, 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)).RotatedByRandom(0.3f)), Vector2.Zero, color ? Color.Cyan : Color.DarkOrchid, color ? Color.DarkOrchid : Color.Cyan, Main.rand.NextFloat(0.4f, 0.6f) * Projectile.scale, 10, Main.rand.NextFloat(-0.1f, 0.1f), 2.68f);
                            GeneralParticleHandler.SpawnParticle(sparker);

                            Dust dust2 = Dust.NewDustPerfect(Owner.Center + (new Vector2(180 * Projectile.scale, 0).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)).RotatedByRandom(0.3f)), DustID.FireworksRGB, Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 2));
                            dust2.scale = Main.rand.NextFloat(0.55f, 0.85f);
                            dust2.noGravity = true;
                            dust2.color = Main.rand.NextBool() ? Color.Cyan : Color.DarkOrchid;
                        }
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

            SoundStyle fire = new("CalamityMod/Sounds/Item/WulfrumKnifeTileHit2");
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
            }
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
                Asset<Texture2D> glowTex = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/GrandGuardianGlow");
                Asset<Texture2D> swoosh = ModContent.Request<Texture2D>("CalamityMod/Particles/VerticalSmearLarge");

                float r = FlipAsSword ? MathHelper.ToRadians(90) : 0f;

                for (int i = 0; i < 25; i++)
                {
                    Texture2D centerTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/GrandGuardianGhost").Value;
                    Color auraColor = Color.Cyan with { A = 0 } * 0.15f * fadeIn;
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 25f).ToRotationVector2() * 6 * fadeIn;
                    Main.EntitySpriteDraw(centerTexture, Projectile.Center - Main.screenPosition + drawOffset + new Vector2(0, Owner.gfxOffY), centerTexture.Frame(1, FrameCount, 0, Frame), auraColor, Projectile.rotation + RotationOffset + r, FlipAsSword ? new Vector2(tex.Width() - SpriteOrigin.X, SpriteOrigin.Y) : SpriteOrigin, Projectile.scale, spriteEffects != SpriteEffects.None ? spriteEffects : (FlipAsSword ? SpriteEffects.FlipHorizontally : SpriteEffects.None));
                }
                if (swingCount > 0)
                    Main.EntitySpriteDraw(swoosh.Value, Projectile.Center - Main.screenPosition + new Vector2(0, Owner.gfxOffY), null, Color.DarkOrchid with { A = 0 } * fadeIn * 0.9f, (FinalRotation + MathHelper.ToRadians(45)) + MathHelper.ToRadians(swingCount % 2 != 0 ? -70 : 70) * -Owner.direction, swoosh.Size() * 0.5f, Projectile.scale * 2.55f / 4, SpriteEffects.None);

                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + new Vector2(0, Owner.gfxOffY), tex.Frame(1, FrameCount, 0, Frame), lightColor, Projectile.rotation + RotationOffset + r, FlipAsSword ? new Vector2(tex.Width() - SpriteOrigin.X, SpriteOrigin.Y) : SpriteOrigin, Projectile.scale, spriteEffects != SpriteEffects.None ? spriteEffects : (FlipAsSword ? SpriteEffects.FlipHorizontally : SpriteEffects.None));
                Main.EntitySpriteDraw(glowTex.Value, Projectile.Center - Main.screenPosition + new Vector2(0, Owner.gfxOffY), glowTex.Frame(1, FrameCount, 0, Frame), Color.White, Projectile.rotation + RotationOffset + r, FlipAsSword ? new Vector2(glowTex.Width() - SpriteOrigin.X, SpriteOrigin.Y) : SpriteOrigin, Projectile.scale, spriteEffects != SpriteEffects.None ? spriteEffects : (FlipAsSword ? SpriteEffects.FlipHorizontally : SpriteEffects.None));

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
        public int TargetIndex = -1;
        public ref float Time => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            if (!ModLoader.TryGetMod("Redemption", out var redemption))
                return;
            redemption.Call("addElementProj", 2, Type);
        }
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.DeathSickle);
            Projectile.width = 74;
            Projectile.height = 68;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 180);
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 120);
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(TargetIndex);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            TargetIndex = reader.ReadInt32();
        }
        public override void AI()
        {
            Projectile.ai[0]++;
            if (Time >= 24f)
            {
                if (TargetIndex >= 0)
                {
                    if (!Main.npc[TargetIndex].active || !Main.npc[TargetIndex].CanBeChasedBy())
                    {
                        TargetIndex = -1;
                    }
                    else
                    {
                        Vector2 value = Projectile.SafeDirectionTo(Main.npc[TargetIndex].Center) * (Projectile.velocity.Length() + 3.5f);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, value, 0.05f);
                    }
                }

                if (TargetIndex == -1)
                {
                    NPC nPC = Projectile.Center.ClosestNPCAt(1600f, ignoreTiles: false);
                    if (nPC != null)
                    {
                        TargetIndex = nPC.whoAmI;
                    }
                }
            }
        }
    }
}
