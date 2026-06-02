using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Clamity.Content.Items.Potions.Food;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Accessories
{
    public class SupremeBarrier : RampartofDeities
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 54;
            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.defense = 30;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<HotPink>();
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            base.UpdateAccessory(player, hideVisual);
            ModContent.GetInstance<AsgardianAegis>().UpdateAccessory(player, hideVisual);
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<AsgardianAegis>()
                //.AddIngredient<ShieldoftheHighRuler>()
                .AddIngredient<RampartofDeities>()
                .AddIngredient<ExoBaguette>()
                .AddIngredient<ShadowspecBar>(5)
                .AddIngredient(ItemID.GolfCupFlagWhite)
                .AddTile<DraedonsForge>()
                .Register();
        }
    }
}
