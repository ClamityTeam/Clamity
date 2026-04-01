using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Materials
{
    public class RarePlanteraFruit : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Materials";
        public override bool IsLoadingEnabled(Mod mod)
        {
            return false;
        }
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 24;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(0, 10);
            Item.rare = ItemRarityID.Red;
        }
    }
}
