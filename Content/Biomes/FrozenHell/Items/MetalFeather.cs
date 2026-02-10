using CalamityMod.Rarities;
using Terraria;
using Terraria.ModLoader;

namespace Clamity.Content.Biomes.FrozenHell.Items
{
    public class MetalFeather : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Materials";
        public override void SetDefaults()
        {
            Item.width = 56;
            Item.height = 66;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(0, 20);
            Item.rare = ModContent.RarityType<BurnishedAuric>();
        }
    }
}
