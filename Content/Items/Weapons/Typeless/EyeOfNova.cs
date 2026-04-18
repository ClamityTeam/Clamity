using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Typeless;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Healing;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;

namespace Clamity.Content.Items.Weapons.Typeless
{
    public class EyeOfNova : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Weapons.Typeless";
        public override void SetDefaults()
        {
            Item.DamageType = ModContent.GetInstance<AverageDamageClass>();
            Item.width = 60;
            Item.damage = 500;
            Item.rare = ModContent.RarityType<BurnishedAuric>();
            Item.useAnimation = Item.useTime = 15;
            Item.useStyle = 5;
            Item.knockBack = 5f;
            Item.UseSound = LunicEye.UseSound;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.height = 50;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.shoot = ModContent.ProjectileType<EyeOfNovaProjectile>();
            Item.shootSpeed = 12f;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = (ContentSamples.CreativeHelper.ItemGroup)CalamityResearchSorting.ClasslessWeapon;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-15f, 0f);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) => position += velocity.SafeNormalize(Vector2.Zero) * 44f;

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<EyeofMagnus>()
                .AddIngredient<Celesticus>()
                .AddIngredient<GoldenGun>()
                .AddIngredient<MiracleMatter>()
                .AddTile<DraedonsForge>()
                .Register();
        }
    }
    public class EyeOfNovaProjectile : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";

        public ref float ProximityFactor => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 30;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.MaxUpdates = 4;
            Projectile.timeLeft = 120 * Projectile.MaxUpdates;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Projectile.ai[0] == 0f)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 velocity = (MathHelper.TwoPi * i / 6f + Projectile.rotation + MathHelper.ToRadians(30f)).ToRotationVector2() * 0.8f;
                    Color crossColor = i % 2 == 1 ? EyeOfNovaBoom.color : EyeOfNovaBoom.color2;
                    Particle cross = new GlowSparkParticle(Projectile.Center, velocity, false, 6, 0.03f, crossColor, Vector2.One, true);
                    GeneralParticleHandler.SpawnParticle(cross);
                }
                Projectile.ai[0] = 1f;
            }

            // Find the closest NPC targetable
            Color trailColor = Color.CornflowerBlue;
            float range = 320f;
            int targetNPC = -1;
            foreach (NPC target in Main.ActiveNPCs)
            {
                if (!target.CanBeChasedBy(Projectile))
                    continue;

                float distance = Vector2.Distance(target.Center, Projectile.Center);
                if (distance < range && Collision.CanHit(Projectile, target))
                {
                    range = distance;
                    targetNPC = target.whoAmI;
                }
            }
            if (targetNPC > -1)
            {
                NPC target = Main.npc[targetNPC];
                Vector2 idealVelocity = Projectile.SafeDirectionTo(target.Center) * 12f;
                Projectile.velocity = (Projectile.velocity * 29f + idealVelocity) / 30f;
                Projectile.velocity = Projectile.velocity.MoveTowards(idealVelocity, 1f);
                ProximityFactor = Utils.GetLerpValue(320f, 0f, Vector2.Distance(Projectile.Center, target.Center), true);
            }
            trailColor = Color.Lerp(Color.CornflowerBlue, Color.Magenta, ProximityFactor);
            Lighting.AddLight(Projectile.Center, trailColor.ToVector3() * 0.5f);

            Dust trail = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), Projectile.velocity * 0.05f);
            trail.noGravity = true;
            trail.scale = Main.rand.NextFloat(0.8f, 1f);
            trail.color = trailColor;

            Vector2 sinOffset = (Vector2.UnitY * MathF.Sin(Projectile.timeLeft * MathHelper.Pi * 0.05f) * 24f).RotatedBy(Projectile.rotation);
            Dust offTrail = Dust.NewDustPerfect(Projectile.Center + sinOffset, DustID.SpectreStaff, Main.rand.NextVector2Circular(0.2f, 0.2f));
            offTrail.noGravity = true;
            offTrail.scale = Main.rand.NextFloat(1.2f, 1.8f);
            offTrail.alpha = Main.rand.Next(120, 180 + 1);
        }

        internal float WidthFunction(float completionRatio, Vector2 vertexPos) => Projectile.scale * 24f;
        internal Color ColorFunction(float completionRatio, Vector2 vertexPos)
        {
            Vector3 trailColor = Main.rgbToHsl(Color.Lerp(Color.CornflowerBlue, EyeOfNovaBoom.color, ProximityFactor));
            Vector3 endColor = trailColor + new Vector3(0.1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 5f) * 0.05f, 0f, 0.1f);
            return Main.hslToRgb(Vector3.Lerp(trailColor, endColor, Utils.GetLerpValue(0f, 0.72f, completionRatio, true))) * Utils.GetLerpValue(0.8f, 0.54f, completionRatio, true) * Projectile.Opacity;
        }

        public override void PostDraw(Color lightColor)
        {
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_, _) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"]), 30);
            Texture2D glow = TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, glow.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MarkedforDeath>(), 480);
            target.AddBuff(ModContent.BuffType<Vaporfied>(), 480);
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 480);
            target.AddBuff(BuffID.Ichor, 480);

            Player player = Main.player[Projectile.owner];
            player.statMana += 25;
            player.ManaEffect(25);
            Main.player[Projectile.owner].SpawnLifeStealProjectile(target, Projectile, ModContent.ProjectileType<RoyalHeal>(), (int)Math.Round(hit.Damage * 0.1), 0.75f);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(EyeofMagnus.ImpactSound, Projectile.Center);

            if (Projectile.owner == Main.myPlayer)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<EyeOfNovaBoom>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }
    public class EyeOfNovaBoom : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public Player Owner => Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public static Color color => Color.Lerp(Main.DiscoColor, Color.White, 0.75f);
        public static Color color2 => Color.Lerp(Main.DiscoColor, Color.White, 0.5f);
        public override void AI()
        {
            if (Time == 0f)
            {
                for (int i = 0; i < 5; i++)
                {
                    Particle explosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Lerp(EyeOfNovaBoom.color, EyeOfNovaBoom.color2, Utils.GetLerpValue(0, 5, i, true)), "CalamityMod/Particles/SoftRoundExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.12f + 0.005f * i, (int)(20 - i * 1.5f));
                    GeneralParticleHandler.SpawnParticle(explosion);
                }
                for (int i = 0; i < 3; i++)
                {
                    Particle explosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Lerp(EyeOfNovaBoom.color, EyeOfNovaBoom.color2, Utils.GetLerpValue(0, 3, i, true)), "CalamityMod/Particles/FlameExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.12f + 0.01f * i, (int)(20 - i * 2f));
                    GeneralParticleHandler.SpawnParticle(explosion);
                }

                Particle outerGlow = new CustomPulse(Projectile.Center, Vector2.Zero, Color.PowderBlue, "CalamityMod/Particles/BloomCircle", Vector2.One, 0f, 0.1f, 2f, 24, true);
                GeneralParticleHandler.SpawnParticle(outerGlow);
                Particle innerGlow = new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/BloomCircle", Vector2.One, 0f, 0.05f, 1f, 24, true);
                GeneralParticleHandler.SpawnParticle(innerGlow);

                float offset = Main.rand.NextFloat(MathHelper.TwoPi);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 velocity = (MathHelper.TwoPi * i / 4f + offset).ToRotationVector2();
                    Particle cross = new GlowSparkParticle(Projectile.Center, velocity, false, 12, 0.4f, EyeOfNovaBoom.color, new Vector2(1f, 0.1f), true);
                    GeneralParticleHandler.SpawnParticle(cross);
                }
            }

            Projectile.scale = MathHelper.Lerp(0f, 1f, PiecewiseAnimation(Time / 20f, new CurveSegment[] { new CurveSegment(EasingType.PolyOut, 0f, 0f, 1f, 4) }));
            if (Time < 10f)
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.Next(71, 73 + 1), Main.rand.NextVector2CircularEdge(6f, 6f) * (Main.rand.NextFloat(1f, 1.2f) + Projectile.scale));
                    dust.noGravity = true;
                    dust.noLight = true;
                    dust.scale = Main.rand.NextFloat(0.8f, 1.2f) + Projectile.scale;
                    dust.alpha = Main.rand.Next(120, 180 + 1);
                }
            }

            Time++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MarkedforDeath>(), 480);
            target.AddBuff(ModContent.BuffType<Vaporfied>(), 480);
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 480);
            target.AddBuff(BuffID.Ichor, 480);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.HitDirectionOverride = (Owner.Center.X < target.Center.X).ToDirectionInt();

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CircularHitboxCollision(Projectile.Center, Projectile.width * Projectile.scale, targetHitbox);
    }
}
