using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Tools.BreakingTool
{
    public class CosmiliteDrill : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Tools";
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 38;
            Item.damage = 200;
            Item.knockBack = 0f;
            Item.useTime = 2;
            Item.useAnimation = 16;
            Item.pick = 275;
            Item.tileBoost += 2;

            Item.DamageType = ModContent.GetInstance<TrueMeleeNoSpeedDamageClass>();
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<CosmicPurple>();
            Item.UseSound = SoundID.Item23;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<CosmiliteDrillProj>();

            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Shoot;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<CosmiliteBar>(8)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    public class CosmiliteDrillProj : ModProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<CosmiliteDrill>();
        public override string Texture => ModContent.GetInstance<CosmiliteDrill>().Texture;

        public Player Owner => Main.player[Projectile.owner];

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.aiStyle = ProjAIStyleID.Drill;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = ModContent.GetInstance<TrueMeleeNoSpeedDamageClass>();
            Projectile.scale = 0.93f;
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            float bladeLength = 70f * Projectile.scale;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + (Projectile.velocity * bladeLength), 24, ref collisionPoint);
        }

        public override void AI()
        {
            if (!Owner.channel)
            {
                Projectile.Kill();
                Projectile.active = false;
                return;
            }

            Projectile.timeLeft = 2;

            //Vroom
            if (Projectile.soundDelay <= 0)
            {
                SoundEngine.PlaySound(SoundID.Item22, Owner.Center);
                Projectile.soundDelay = 30;
            }

            Projectile.velocity = (Owner.Calamity().mouseWorld - Owner.MountedCenter).SafeNormalize(Vector2.One);
            //Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
            Owner.heldProj = Projectile.whoAmI;
            Owner.ChangeDir(Math.Sign(Projectile.velocity.X));
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() * Owner.gravDir - MathHelper.PiOver2);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() * Owner.gravDir - MathHelper.PiOver2 - MathHelper.PiOver4 * 0.5f * Owner.direction);

            Owner.SetDummyItemTime(2);
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.Center = Owner.MountedCenter + Projectile.velocity * 12f;

            Projectile.velocity.X *= 1f + (float)Main.rand.Next(-3, 4) * 0.01f;

            if (Main.rand.NextBool(6))
            {
                Dust whirrDust = Dust.NewDustDirect(Projectile.position + Projectile.velocity * Main.rand.Next(6, 10) * 0.1f, Projectile.width, Projectile.height, Main.rand.NextBool() ? DustID.GemSapphire : DustID.GemAmethyst, 0f, 0f, 80, Scale: 1.4f);
                whirrDust.position.X -= 4f;
                whirrDust.noGravity = true;
                whirrDust.velocity *= 0.2f;
                whirrDust.velocity.Y = (float)(-Main.rand.Next(7, 13)) * 0.15f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            //Necessary or else itll glitch out for a final frame???
            if (!Projectile.active)
                return false;

            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(9f, tex.Height / 2f);
            Vector2 shake = Main.rand.NextVector2Circular(1f, 1f) * ((float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.25f + 0.75f);
            SpriteEffects effect = SpriteEffects.None;
            if (Owner.direction * Owner.gravDir < 0)
                effect = SpriteEffects.FlipVertically;

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition + shake, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, effect, 0);
            return false;
        }
    }
}
