using CalamityMod;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Typeless;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Typeless
{
    public class Celesticus : Aestheticus, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Weapons.Typeless";
        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.damage = 200;
            Item.useTime = Item.useAnimation = 10;
            base.Item.Calamity().donorItem = false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<Aestheticus>()
                .AddIngredient<TrashOfMagnus>()
                .AddIngredient<DivineGeode>(5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
