using CalamityMod.Items;
using CalamityMod.Items.Fishing.FishingRods;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Tools.FishingPoles
{
    public class WulfrumLure : ModItem, ILocalizedModType, IModType
    {
        public int charge = 0;
        public new string LocalizationCategory => "Items.Fishing";

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;

            Item.useAnimation = 8;
            Item.useTime = 8;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = new SoundStyle?(SoundID.Item1);

            Item.shootSpeed = 13f;
            Item.shoot = ModContent.ProjectileType<WulfrumLureBobber>();

            Item.fishingPole = 30;
        }
        public override bool AltFunctionUse(Player player) => charge == 0;
        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                if (player.HasItem(ModContent.ItemType<EnergyCore>()) && charge == 0)
                {
                    player.ConsumeItem(ModContent.ItemType<EnergyCore>());
                    charge = 10;
                }
                else
                    return false;
                Item.shoot = ProjectileID.None;
            }
            else
            {
                Item.shoot = ModContent.ProjectileType<WulfrumLureBobber>();
                if (charge > 0) charge--;
            }
            return base.UseItem(player);
        }
        public override void UpdateInventory(Player player)
        {
            if (charge > 0)
                Item.fishingPole = 70;
            else
                Item.fishingPole = 30;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int num = 2;
            if (charge > 0) num += 4;
            for (int index = 0; index < num; ++index)
            {
                float num1 = velocity.X + Main.rand.NextFloat(-3.75f, 3.75f);
                float num2 = velocity.Y + Main.rand.NextFloat(-3.75f, 3.75f);
                Projectile.NewProjectile(source, position.X, position.Y, num1, num2, type, 0, 0.0f, player.whoAmI, 0.0f, 0.0f, 0.0f);
            }
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }
        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Player player = Main.player[Main.myPlayer];
            if (player.inventory[player.selectedItem].type == Item.type)
            {
                string text = charge.ToString();

                spriteBatch.DrawString(FontAssets.ItemStack.Value, text, position, new Color(255, 0, 255), 0f, new Vector2(-7f, 10f), 1.1f, SpriteEffects.None, 0f);
            }
        }
        public override void ModifyFishingLine(Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor)
        {
            lineOriginOffset = new Vector2(45, -37);
            lineColor = new Color(255, 251, 189, 100);
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NavyFishingRod>()
                .AddIngredient<WulfrumRod>()
                .AddIngredient(ItemID.Bone, 20)
                .AddIngredient<EnergyCore>()
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    public class WulfrumLureBobber : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = 61;
            Projectile.bobber = true;
            Projectile.penetrate = -1;
        }
    }
}
