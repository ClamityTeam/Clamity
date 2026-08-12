/*using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.BaseItems;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Melee;
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
            Item.damage = 20;
            Item.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();
            Item.useAnimation = Item.useTime = 71;
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
    }
    public class StunGunHoldout : BaseCustomUseStyleProjectile, ILocalizedModType
    {
        public override int AssignedItemID => ModContent.ItemType<Hellkite>();

        public override LocalizedText DisplayName => CalamityUtils.GetItemName<Hellkite>();
        public override string Texture => "CalamityMod/Items/Weapons/Melee/Hellkite";
        public override float HitboxOutset => swingOffset;

        public int size = 32;
        public float swingOffset = 100;
        public override Vector2 HitboxSize => new Vector2(size, size);
        public override Vector2 SpriteOrigin => new(0, size);
        public override float HitboxRotationOffset => MathHelper.ToRadians(-45);

        public Vector2 mousePos;
        public Vector2 aimVel;
        public bool doSwing = false;
        public bool postSwing = false;
        public float fadeIn = 0f;
        public int useAnim;

        public bool chargedSwing = false; // True if you have a charged swing fully charged
        public int chargeTimer = 0; // Timer for charging the blade with right click
        public int chargeTimerMax = 120; // This is set to be base don use time on spawn
        public bool playSwingSound = true;
        //public bool FirstIFrameReset = false;
        //public bool SecondIFrameReset = false;
        //public SlotId AudSlot;
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

            // 14NOV2024: Ozzatron: clamped mouse position unnecessary, as Hellkite has no projectiles
            mousePos = Owner.Calamity().mouseWorld;
            aimVel = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitX) * 65;
            useAnim = Owner.itemAnimationMax;

            chargeTimerMax = useAnim * 5; // Max charge time is set here

            if (mousePos.X < Owner.Center.X) Owner.direction = -1;
            else Owner.direction = 1;

            FlipAsSword = Owner.direction == -1 ? true : false;

        }
        public override void UseStyle()
        {
            AnimationProgress = Animation % useAnim;

            bool cantUse = (Owner == null || !Owner.active || Owner.dead || (Projectile.ai[2] == 0 && !Owner.channel) || Owner.CCed || Owner.noItems);

            if (CanHit || postSwing)
                mousePos = Owner.Center - aimVel;
            else
            {
                mousePos = Owner.Calamity().mouseWorld;
            }

            if (cantUse)
            {
                chargeTimer = 0;
            }

            if (!doSwing)
            {
                mousePos = Owner.Calamity().mouseWorld;
                aimVel = (Owner.Center - Owner.Calamity().mouseWorld).SafeNormalize(Vector2.UnitX) * 65;
                CanHit = false;
                if (mousePos.X < Owner.Center.X) Owner.direction = -1;
                else Owner.direction = 1;
                FlipAsSword = Owner.direction == -1 ? true : false;

                Vector2 bladePos = new Vector2(0, 0); //60, 0
                Vector2 particlePos = Owner.Center + (bladePos).RotatedBy(FinalRotation + MathHelper.ToRadians(-45)) * Projectile.scale;

                RotationOffset = MathHelper.Lerp(RotationOffset, MathHelper.ToRadians(120f * Projectile.ai[1] * Owner.direction), 0.05f);

                float rotationValue = (25 * Utils.GetLerpValue(0, chargeTimerMax, chargeTimer, true)) * (FlipAsSword ? 1 : -1) * -Projectile.ai[1];
                Projectile.rotation = Projectile.rotation.AngleLerp(Owner.AngleTo(mousePos) + MathHelper.ToRadians(rotationValue), 0.3f);
                Animation = 0;
                Owner.itemAnimation++;
                Projectile.timeLeft++;

                if (chargeTimer < chargeTimerMax && !chargedSwing)
                    chargeTimer++;


                if (chargeTimer == chargeTimerMax)
                {
                    chargedSwing = true;
                    //useAnim = storedUseAnim / 3;
                    chargeTimer++;

                    //Booo charged particles
                }

                if (chargeTimer == 0)
                {
                    for (int i = 0; i < Main.maxNPCs; i++)
                        Projectile.localNPCImmunity[i] = 0;

                    Projectile.numHits = 0;
                    //pierceReduction = 0;
                    doSwing = true;
                }
            }
            else if (chargeTimer == 0) //swing
            {
                

            }




                ArmRotationOffset = MathHelper.ToRadians(-140f);
            ArmRotationOffsetBack = MathHelper.ToRadians(-140f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Only draw the projectile if the projectile's owner is currently using the item this projectile is attached to.
            if (Owner.itemAnimation > 0 || DrawUnconditionally)
            {
                Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);

                float r = FlipAsSword ? MathHelper.ToRadians(90) : 0f;

                Vector2 cen = Projectile.Center + new Vector2(HitboxOutset * Projectile.scale, HitboxOutset * Projectile.scale).RotatedBy(FinalRotation + HitboxRotationOffset);
                Main.EntitySpriteDraw(tex.Value, cen - Main.screenPosition + new Vector2(0, Owner.gfxOffY), tex.Frame(1, FrameCount, 0, Frame), lightColor, Projectile.rotation + RotationOffset + r, FlipAsSword ? new Vector2(tex.Width() - SpriteOrigin.X, SpriteOrigin.Y) : SpriteOrigin, Projectile.scale, spriteEffects != SpriteEffects.None ? spriteEffects : (FlipAsSword ? SpriteEffects.FlipHorizontally : SpriteEffects.None));

            }
            return false;
        }
    }
}*/
