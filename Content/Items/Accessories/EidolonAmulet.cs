using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Accessories
{
    public class EidolonAmulet : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override bool IsLoadingEnabled(Mod mod)
        {
            return false;
        }
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 46;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 15);
            Item.rare = ModContent.RarityType<PureGreen>();
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();

            modPlayer.dOfTheDeep = true;
            modPlayer.dOfTheDeepVisual = !hideVisual;
            modPlayer.WaterDebuffMultiplier += 0.75f;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<DiamondOfTheDeep>()
                .AddIngredient<ReaperTooth>(5)
                .AddIngredient<RuinousSoul>(3)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
