using CalamityMod.Tiles.BaseTiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Pyrogen.Drop
{
    public class PyrogenRelic : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Placeables.Relics";

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<PyrogenRelicTile>(), 0);
            Item.width = 30;
            Item.height = 40;
            Item.maxStack = 9999;
            Item.rare = ItemRarityID.Master;
            Item.master = true;
            Item.value = Item.buyPrice(0, 5, 0, 0);
        }
    }
    public class PyrogenRelicTile : BaseBossRelic
    {
        public override string RelicTextureName => (GetType().Namespace + "." + Name).Replace('.', '/');
        public override int AssociatedItem => ModContent.ItemType<PyrogenRelic>();
    }
}
