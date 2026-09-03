using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items;
using CalamityMod.Items.BaseItems;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Melee
{
    public class StunGun : CustomUseProjItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {

            Item.width = 124;
            Item.height = 124;
            Item.damage = 25;
            Item.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();
            Item.useAnimation = Item.useTime = 60;
            Item.useTurn = true;
            Item.knockBack = 13f;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;

            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<StunGunHoldout>();
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Shoot;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MysteriousCircuitry>(5)
                .AddIngredient<DubiousPlating>(7)
                .AddRecipeGroup(RecipeSystem.AnyCopperBar, 4)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    public class StunGunHoldout : BaseCustomUseStyleProjectile, ILocalizedModType
    {
        public override int AssignedItemID => ModContent.ItemType<StunGun>();

        public override LocalizedText DisplayName => CalamityUtils.GetItemName<StunGun>();
        public override string Texture => "Clamity/Content/Items/Weapons/Melee/StunGun";
        public override float HitboxOutset => 64;

        public int size = 32;
        public float swingOffset = 32;
        public override Vector2 HitboxSize => new Vector2(48, 48);
        public override Vector2 SpriteOrigin => new(0, size);
        public override float HitboxRotationOffset => MathHelper.ToRadians(-45);

        public Vector2 mousePos;
        public Vector2 aimVel;
        public bool doSwing = false;
        public bool doAttack = false;
        public float attackOffset = 0f;
        public int useAnim;

        public bool chargedSwing = false; // True if you have a charged swing fully charged
        public bool chargedHit = false; //True if charged swing hitted enemy
        public bool releaseFullCharge = false; //True if released charge after charged hit
        public int chargeTimer = 0; // Timer for charging the blade with right click
        public int chargeTimerMax = 120; // This is set to be base don use time on spawn
        public bool playSwingSound = true;
        //public bool FirstIFrameReset = false;
        //public bool SecondIFrameReset = false;
        //public SlotId AudSlot;

        public float chargeProgress = 0f;
        public int chargedDamage = 20;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();
        }
        public override void WhenSpawned()
        {
            CanHit = false;
            Projectile.knockBack = 0;
            Projectile.ai[1] = -1;

            chargedDamage = Owner.HeldItem.damage;
            chargeProgress = 0f;

            // 14NOV2024: Ozzatron: clamped mouse position unnecessary, as Hellkite has no projectiles
            mousePos = Owner.Calamity().mouseWorld;
            aimVel = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitX) * 65;
            useAnim = Owner.itemAnimationMax;

            Projectile.rotation = Projectile.Center.AngleTo(mousePos);

            //chargeTimerMax = useAnim * 5; // Max charge time is set here

            //if (mousePos.X < Owner.Center.X) Owner.direction = -1;
            //else Owner.direction = 1;

            FlipAsSword = Owner.direction == -1 ? true : false;

        }
        public override void UseStyle()
        {
            

            AnimationProgress = Animation % useAnim;

            if (Owner.CantUseHoldout(false) || Owner.HeldItem.type != Owner.HeldItem.type)
                Projectile.Kill();

            if (Owner.CantUseHoldout()) //Attack
            {
                Vector2 c = Projectile.Center + new Vector2(size, size).RotatedBy(Projectile.rotation - MathHelper.PiOver2);
                if (!doAttack)
                {
                    Animation = useAnim - 2;
                    Owner.itemAnimation = Owner.itemAnimationMax - 2;
                    Projectile.timeLeft = Owner.itemAnimation;

                    doAttack = true;
                    CanHit = true;

                    Projectile.damage = chargedDamage;

                    for (int i = 0; i < 5; i++)
                    {
                        Dust.NewDustPerfect(c, DustID.Electric, (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2().RotatedByRandom(MathHelper.PiOver4) * 3);
                    }
                }

                if (AnimationProgress < (int)(useAnim * 0.3f))
                {
                    attackOffset = MathHelper.Lerp(attackOffset, 1, CalamityUtils.ExpInEasing(Utils.Remap(AnimationProgress / (float)useAnim, 0, 0.2f, .7f, 1f), 1));
                    chargeTimer = (int)(Utils.Remap(attackOffset, -1, 1, 1, 0) * chargeTimerMax);
                    if (chargeTimer == 3)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Dust.NewDustPerfect(c, DustID.Electric, Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * 2);
                        }
                    }
                }
                else // if (Animation <= (int)(useAnim * 0.4f))
                {
                    attackOffset = MathHelper.Lerp(attackOffset, -1, CalamityUtils.ExpInEasing(Utils.Remap(AnimationProgress / (float)useAnim, 0.2f, 1, 0f, 1f), 1));
                    if (chargedHit)
                    {
                        attackOffset = MathHelper.Lerp(attackOffset, -1, 1f - CalamityUtils.ExpOutEasing(Utils.Remap(AnimationProgress / (float)useAnim, 0.2f, 1, 0f, 1f), 1));
                        if (!releaseFullCharge)
                        {
                            //Main.NewText("!");
                            for (int i = 0; i < 20; i++)
                            {
                                SparkParticle p = new SparkParticle(c, Vector2.UnitX.RotatedBy(Projectile.rotation - MathHelper.PiOver4).RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(4, 10), false, 30, 0.5f, Color.DodgerBlue, true);
                                GeneralParticleHandler.SpawnParticle(p);
                            }
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), c, Vector2.Zero, ModContent.ProjectileType<StunGunChargedProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                            releaseFullCharge = true;
                        }
                    }
                    else
                    {
                        CanHit = false;
                    }

                }
            }
            else //Charging
            {
                if (chargeTimer < chargeTimerMax)
                    chargeTimer++;
                else if (!chargedSwing)
                {
                    chargedSwing = true;
                    for (int i = 0; i < 6; i++)
                    {
                        Dust.NewDustPerfect(Projectile.Center + new Vector2(size, size).RotatedBy(Projectile.rotation - MathHelper.PiOver2), DustID.Electric, Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * 2);
                    }
                }

                //Offset = -Projectile.Size.RotatedBy(Projectile.rotation - MathHelper.PiOver2) / 4 * (chargeTimer / (float)chargeTimerMax);

                attackOffset = -chargeTimer / (float)chargeTimerMax;

                // Store how much the weapon has charged.
                float chargeProgress = chargeTimer / (float)chargeTimerMax;

                float baseDamage = Owner.HeldItem.damage;
                float maxDamage = baseDamage * 5f;

                chargedDamage = (int)MathHelper.Lerp(
                    baseDamage,
                    maxDamage,
                    chargeProgress
                );

                Projectile.rotation = Projectile.rotation.AngleLerp(Owner.AngleTo(mousePos) + MathHelper.PiOver4, 0.3f);
                //RotationOffset = MathHelper.Lerp(RotationOffset, MathHelper.ToRadians(120f * Projectile.ai[1] * Owner.direction), 0.05f);

                mousePos = Owner.Calamity().mouseWorld;
                aimVel = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitX) * 65;
                CanHit = false; 
                Animation = 0;
                Owner.itemAnimation++;
                Projectile.timeLeft++;
            }

            ArmRotationOffset = MathHelper.ToRadians(-140f);
            ArmRotationOffsetBack = MathHelper.ToRadians(-140f);

            Offset = Projectile.Size.RotatedBy(Projectile.rotation - MathHelper.PiOver2) / 4 * attackOffset;


        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Only draw the projectile if the projectile's owner is currently using the item this projectile is attached to.
            if (Owner.itemAnimation > 0 || DrawUnconditionally)
            {
                Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
                Asset<Texture2D> tex2 = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");

                float r = FlipAsSword ? MathHelper.ToRadians(90) : 0f;

                Vector2 cen = Projectile.Center /*+ new Vector2(HitboxOutset * Projectile.scale, HitboxOutset * Projectile.scale).RotatedBy(FinalRotation + HitboxRotationOffset)*/;
                Main.EntitySpriteDraw(tex.Value, cen - Main.screenPosition + new Vector2(0, Owner.gfxOffY), tex.Frame(1, FrameCount, 0, Frame), lightColor, Projectile.rotation + RotationOffset + r, FlipAsSword ? new Vector2(tex.Width() - SpriteOrigin.X, SpriteOrigin.Y) : SpriteOrigin, Projectile.scale, spriteEffects != SpriteEffects.None ? spriteEffects : (FlipAsSword ? SpriteEffects.FlipHorizontally : SpriteEffects.None));

                //float chargeScale = Utils.GetLerpValue(0, MagnaCannon.FullChargeFrames, CurrentChargingFrames, true) : 0;
                float chargeScale = chargedSwing ? chargeTimer / (float)chargeTimerMax : 0;
                for (int i = 0; i < 3; i++)
                    Main.EntitySpriteDraw(tex2.Value, 
                        Projectile.Center + new Vector2(size, size).RotatedBy(Projectile.rotation - MathHelper.PiOver2) - Main.screenPosition, null, 
                        Color.Lerp(Color.DodgerBlue, Color.White, i * 0.25f) with { A = 0 } * 0.8f, 
                        Main.rand.NextFloat(-5, 5), tex2.Size() * 0.5f, 
                        new Vector2(1.35f, 1f) * Projectile.scale * chargeScale * (1 - 0.27f * i) * 0.25f, 
                        SpriteEffects.None, 0);


            }
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            chargedHit = chargedSwing ? true : false;

            if (chargedHit)
            {
                target.AddBuff(BuffID.Electrified, 300);
                target.AddBuff(ModContent.BuffType<GalvanicCorrosion>(), 60);
            }
            else
            {
                target.AddBuff(ModContent.BuffType<StaticDischarge>(), 300);
            }
        }
    }
    public class StunGunChargedProjectile : ModProjectile, ILocalizedModType
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<StunGun>();
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 128;
            Projectile.aiStyle = -1;
            AIType = -1;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.DamageType = DamageClass.Melee;
        }
    }
}
