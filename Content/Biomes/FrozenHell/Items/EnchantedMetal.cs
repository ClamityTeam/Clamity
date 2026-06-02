using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Biomes.FrozenHell.Items
{
    public class EnchantedMetal : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Materials";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.SortingPriorityMaterials[Type] = 106;
        }

        public override void SetDefaults()
        {
            //Item.createTile = ModContent.TileType<>();
            Item.width = 15;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(0, 15, 0, 0);
            Item.rare = ModContent.RarityType<BurnishedAuric>();

            //Item.useStyle = 1;
            //Item.useTurn = true;
            //Item.useAnimation = 15;
            //Item.useTime = 10;
            //Item.autoReuse = true;
            //Item.consumable = true;
        }
        public override void AddRecipes()
        {
            CreateRecipe(2)
                .AddIngredient<FrozenHellstone>(3)
                .AddIngredient(ItemID.Obsidian)
                .AddTile(TileID.AdamantiteForge)
                .Register();
        }
    }
}
