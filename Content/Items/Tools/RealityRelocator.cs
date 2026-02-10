using CalamityMod;
using CalamityMod.Items.Placeables.Ores;
using CalamityMod.Items.Placeables.Plates;
using CalamityMod.Items.Tools;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;

namespace Clamity.Content.Items.Tools
{
    public class RealityRelocator : NormalityRelocator
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.Calamity().donorItem = false;
            //ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<NormalityRelocator>()] = Item.type;
        }
        public override void UpdateInventory(Player player)
        {
            player.Clamity().realityRelocator = true;

        }
        public override void AddRecipes()
        {
            CreateRecipe().AddIngredient(ItemID.RodOfHarmony).AddIngredient<Cinderplate>(5).AddIngredient<ExodiumCluster>(10)
                .AddIngredient(ItemID.FragmentStardust, 30)
                .AddTile<DraedonsForge>()
                .Register();
        }
    }
}
