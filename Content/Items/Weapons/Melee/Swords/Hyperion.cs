using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Melee.Swords
{
    public class Hyperion : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override bool IsLoadingEnabled(Mod mod) => false;
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 7));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 72;
            Item.height = 72;
            Item.damage = 500;
            Item.DamageType = DamageClass.Melee;
            Item.useAnimation = Item.useTime = 20;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4.25f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<Violet>();
            Item.shoot = ModContent.ProjectileType<HyperionProjectile>();
            Item.shootSpeed = 16f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 direction = Main.MouseWorld - player.Center;
            float[] laserLengthSamplePoints = new float[24];
            Collision.LaserScan(player.Center, direction.SafeNormalize(Vector2.Zero), 16f, MathHelper.Clamp(direction.Length(), 1, 400), laserLengthSamplePoints);
            float distance = laserLengthSamplePoints.Average();
            Vector2 offset = direction.SafeNormalize(Vector2.Zero) * distance;
            player.Center += offset;
            Projectile.NewProjectile(source, position + offset, Vector2.Zero, type, damage, knockback, player.whoAmI);
            player.immuneTime = Item.useTime + 1;
            return false;
        }
    }
    public class HyperionProjectile : TerratomereExplosion, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override bool IsLoadingEnabled(Mod mod) => false;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.scale *= 4f;
        }

        public new void AdditiveDraw(SpriteBatch spriteBatch)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Texture2D lightTexture = ModContent.Request<Texture2D>("CalamityMod/Skies/XerocLight").Value;
            Rectangle frame = texture.Frame(3, 6, Projectile.frame / 6, Projectile.frame % 6);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = frame.Size() * 0.5f;

            for (int i = 0; i < 2; i++)
            {
                Vector2 lightDrawPosition = drawPosition + (MathHelper.TwoPi * i / 36f + Main.GlobalTimeWrappedHourly * 5f).ToRotationVector2() * Projectile.scale * 12f;
                Color lightBurstColor = CalamityUtils.MulticolorLerp(Projectile.timeLeft / 144f, new Color(191, 263, 255), new Color(109, 313, 255));
                lightBurstColor = Color.Lerp(lightBurstColor, Color.White, 0.4f) * Projectile.Opacity * 0.24f;
                Main.spriteBatch.Draw(lightTexture, lightDrawPosition, null, lightBurstColor, 0f, lightTexture.Size() * 0.5f, Projectile.scale * 1.32f, SpriteEffects.None, 0);
            }
            if (Projectile.timeLeft < 149)
                Main.spriteBatch.Draw(texture, drawPosition, frame, Color.White, 0f, origin, Projectile.scale * 1.6f, SpriteEffects.None, 0);
        }
    }
}
