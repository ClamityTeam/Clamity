using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.Enums;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Melee;
using Clamity.Content.Bosses.Clamitas.Drop;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;


namespace Clamity.Content.Bosses.Clamitas.Crafted.Weapons
{
    public class ClamitasCrusher : ClamCrusher
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.rare = ItemRarityID.Lime;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.damage = 200;
            Item.shoot = ModContent.ProjectileType<ClamitasCrusherProjectile>();
            Item.shootSpeed = 25f;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ClamCrusher>()
                .AddIngredient<HuskOfCalamity>(12)
                .AddIngredient<AshesofCalamity>(6)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class ClamitasCrusherProjectile : BaseMaceFlailProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<ClamitasCrusher>();
        public override int SpinIFrames => 24;
        public override float SpinSpeed => 5f;
        public override float SpinHitboxRadius => 96f;
        public override float SpinVisualRadius => 64f;
        public override int AfterimageLength => 8;
        public override float LaunchSpeed => 60f;
        public override int LaunchLifespan => 12;
        public override float MaxDropRange => 960f;
        public override float MaxRetractSpeed => 90f;
        public override float RetractAcceleration => 4.5f;

        public float BaseMaxChargeOfSwing = 180; //hummus check this
        public float MaxChargeOfSwing => BaseMaxChargeOfSwing / (1f + Owner.GetAttackSpeed<MeleeDamageClass>());

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 74;
            base.SetDefaults();

        }

        public override void AI()
        {
            if (ChargingSwing < 0 && ChargingSwing > -MaxChargeOfSwing)
            {
                ChargingSwing--;
            }
            base.AI();
        }
        public ref float ChargingSwing => ref Projectile.ai[2];
        public override void SpinAI(float launchSpeed)
        {
            Projectile.ownerHitCheck = true;
            if (Projectile.owner == Main.myPlayer)
            {
                // 14NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
                Vector2 toMouse = Owner.MountedCenter.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.UnitX * Owner.direction);
                Owner.ChangeDir((toMouse.X > 0f).ToDirectionInt());
                if (!Owner.channel && ChargingSwing == 0)
                {
                    ChargingSwing = -1;
                    Projectile.netUpdate = true;
                    StateTimer = StateTimer % 60;
                }
                if (ChargingSwing <= -MaxChargeOfSwing)
                {
                    CurrentFlailState = FlailState.LaunchingForward;
                    StateTimer = 0f;
                    Projectile.Center = Owner.MountedCenter;
                    Projectile.velocity = toMouse * launchSpeed;
                    Projectile.netUpdate = true;
                    Projectile.ResetLocalNPCHitImmunity();
                    Projectile.localNPCHitCooldown = LaunchIFrames * Projectile.MaxUpdates;
                    Projectile.ownerHitCheck = false;
                    EffectBeforeLaunch?.Invoke(Projectile);
                    return;
                }
            }
            StateTimer++;
            Vector2 spinOffset = new Vector2(Owner.direction).RotatedBy(MathHelper.Pi * SpinSpeed * (1 - ChargingSwing / MaxChargeOfSwing) * (StateTimer / 60f) * Owner.direction);

            spinOffset.Y *= SpinVerticalFactor;
            if (spinOffset.Y * Owner.gravDir > 0f)
                spinOffset.Y *= 0.5f;

            Projectile.Center = Owner.MountedCenter + spinOffset * SpinVisualRadius + new Vector2(0, Owner.gfxOffY);
            Projectile.velocity = Vector2.Zero;
            Projectile.localNPCHitCooldown = SpinIFrames * Projectile.MaxUpdates;

            //Main.NewText(ChargingSwing);

        }
        public override void RetractAI(bool forced, float maxSpeed, float acceleration)
        {
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            float forceMult = forced ? 2f : 1f;
            Vector2 toPlayer = Projectile.SafeDirectionTo(Owner.MountedCenter);
            Vector2 value = Owner.MountedCenter.DirectionFrom(Projectile.Center + Projectile.velocity).SafeNormalize(Vector2.Zero);
            if (Projectile.Distance(Owner.MountedCenter) <= maxSpeed * forceMult || (forced && Vector2.Dot(toPlayer, value) < 0f))
            {
                Projectile.Kill();
                return;
            }

            Projectile.velocity *= 0.98f;
            Projectile.velocity = Projectile.velocity.MoveTowards(toPlayer * maxSpeed * forceMult, acceleration * forceMult);
            Owner.ChangeDir((Owner.Center.X < Projectile.Center.X).ToDirectionInt());
        }
        public void HeyDoSomethingOnHit(IEntitySource source)
        {

            if (CurrentFlailState == FlailState.LaunchingForward)
            {
                float ExplosionRadius = 100;
                Color color1 = Color.Orange;

                Particle explosion = new DetailedExplosion(Projectile.Center, Vector2.Zero, color1, Vector2.One, Main.rand.NextFloat(-5, 5), 0f, ExplosionRadius * 0.0065f + 0.1f, Main.rand.Next(15, 22));
                GeneralParticleHandler.SpawnParticle(explosion, false, GeneralDrawLayer.AfterEverything);
                Particle explosion2 = new DetailedExplosion(Projectile.Center, Vector2.Zero, Color.White, Vector2.One, Main.rand.NextFloat(-5, 5), 0f, ExplosionRadius * 0.0045f + 0.1f, Main.rand.Next(15, 22), false);
                GeneralParticleHandler.SpawnParticle(explosion2);
                Particle explosion3 = new DetailedExplosion(Projectile.Center, Vector2.Zero, Color.White, Vector2.One, Main.rand.NextFloat(-5, 5), 0f, ExplosionRadius * 0.0030f + 0.1f, Main.rand.Next(15, 22), false);
                GeneralParticleHandler.SpawnParticle(explosion3);

                for (int i = 0; i < 4; i++)
                {
                    Particle blastRing = new CustomPulse(Projectile.Center, Vector2.Zero, color1, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 0, ExplosionRadius * 0.005f + 0.05f, 25);
                    GeneralParticleHandler.SpawnParticle(blastRing, false, GeneralDrawLayer.AfterEverything);
                }

                /*float numberOfDusts = ExplosionRadius * 0.1f + 10;
                float rotFactor = 360f / numberOfDusts;
                for (int i = 0; i < numberOfDusts; i++)
                {
                    float rot = MathHelper.ToRadians(i * rotFactor);
                    Vector2 offset = (Vector2.UnitX * Main.rand.NextFloat(ExplosionRadius * 0.2f, 3.1f)).RotatedBy(rot * Main.rand.NextFloat(1.1f, 9.1f));
                    Vector2 velOffset = (Vector2.UnitX * Main.rand.NextFloat(ExplosionRadius * 0.2f, 3.1f)).RotatedBy(rot * Main.rand.NextFloat(1.1f, 9.1f));
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, Main.rand.NextBool(4) ? ModContent.DustType<LightDust>() : Projectile.ai[1] == 5 ? 278 : ModContent.DustType<VoidDustInverted>(), velOffset);
                    dust.noGravity = dust.type == 278 ? false : true;
                    dust.color = color1;
                    dust.velocity = velOffset;
                    dust.scale = dust.type == 278 ? Main.rand.NextFloat(0.7f, 1.3f) : Main.rand.NextFloat(1.6f, 2.2f);
                }*/

                for (int i = 0; i < (int)Utils.Remap((Projectile.Center - Owner.Center).Length(), 0, 16 * 20, 1, 15); i++)
                {
                    Projectile.NewProjectile(source, Projectile.Center, -Vector2.UnitY * 10 - Main.rand.NextVector2Circular(5, 5), ModContent.ProjectileType<ClamitasCrusherWisp>(), Projectile.damage / 10, Projectile.knockBack / 2, Projectile.owner);
                }

                Projectile.Kill();
                /*CurrentFlailState = FlailState.Retracting;
                StateTimer = 0f;
                Projectile.netUpdate = true;
                Projectile.velocity = -Projectile.velocity;
                //Projectile.velocity *= 0.3f;
                EffectBeforePullback?.Invoke(Projectile);*/
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            HeyDoSomethingOnHit(Projectile.GetSource_OnHit(target));
            target.AddBuff(ModContent.BuffType<Eutrophication>(), 60);
            if (CurrentFlailState == FlailState.Spinning)
            {
                Projectile.NewProjectile(Projectile.GetSource_OnHit(target), target.Center, -Vector2.UnitY * 10 - Main.rand.NextVector2Circular(5, 5), ModContent.ProjectileType<ClamitasCrusherWisp>(), Projectile.damage / 10, Projectile.knockBack / 2, Projectile.owner);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            HeyDoSomethingOnHit(Projectile.GetSource_FromThis());
            return base.OnTileCollide(oldVelocity);
        }

        public override void PostDraw(Color lightColor)
        {
            //THIS IS AAANGER IN A KETTLE!!
            float ratio = (-ChargingSwing / MaxChargeOfSwing);
            Texture2D t = ModContent.Request<Texture2D>(Texture).Value;
            Color color = new Color(255, 140, 63, 100) * (ratio / 2);
            SpriteEffects effects = ((base.Projectile.spriteDirection != 1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            Vector2 origin = t.Size() / 2;
            int i_max = 4;
            for (int i = 0; i < i_max; i++)
            {
                Vector2 position = Projectile.oldPos[0] + new Vector2(0, 4).RotatedBy(MathHelper.TwoPi / i_max * i) * ratio 
                    - Main.screenPosition + origin * base.Projectile.scale + new Vector2(0f, base.Projectile.gfxOffY);

                Main.spriteBatch.Draw(t, position, null, color, Projectile.rotation, origin, Projectile.scale, effects, 0);
            }
        }
    }
    public class ClamitasCrusherWisp : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public ref float SpriteType => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 48;
            Projectile.aiStyle = -1;
            AIType = -1;
            Projectile.timeLeft = 600;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.scale = 0.5f;
        }
        public override void AI()
        {
            if (Projectile.ai[1] == 0)
            {
                Projectile.velocity.Y += 0.2f;
                if (Projectile.velocity.Y > 0) Projectile.ai[1] = 1;
            }
            else //Projectile.ai[1] == 1
            {
                CalamityUtils.HomeInOnNPC(Projectile, true, 1000, 10, 20);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

        }
        public override bool? CanHitNPC(NPC target)
        {
            return Projectile.ai[1] == 1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                WeightedRandom<int> typeDecider = new WeightedRandom<int>((int)Main.GlobalTimeWrappedHourly + Projectile.whoAmI);
                typeDecider.Add(ModContent.ProjectileType<RedirectingLostSoul>(), 0.75f);
                typeDecider.Add(ModContent.ProjectileType<RedirectingVengefulSoul>(), 0.4f);
                typeDecider.Add(ModContent.ProjectileType<RedirectingGildedSoul>(), 0.2f);
                SpriteType = typeDecider.Get();
                Projectile.netUpdate = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (SpriteType != -1)
            {
                Texture2D t = TextureAssets.Projectile[(int)SpriteType].Value;
                Rectangle frame = t.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
                SpriteEffects direction = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                for (int j = 0; j < Projectile.oldPos.Length / 2; j++)
                {
                    float fade = (float)Math.Pow(1f - Utils.GetLerpValue(0f, Projectile.oldPos.Length / 2, j, true), 2D);
                    Color drawColor = Projectile.GetAlpha(lightColor) * fade;
                    Vector2 drawPosition = Projectile.oldPos[j] + Projectile.Size * 0.5f - Main.screenPosition;
                    float rotation = Projectile.oldRot[j];

                    Main.EntitySpriteDraw(t, drawPosition, frame, drawColor, rotation, frame.Size() * 0.5f, Projectile.scale, direction, 0);
                }
                //Main.spriteBatch.Draw(t, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, direction, 0);
            }
            return false;
        }

    }
}
