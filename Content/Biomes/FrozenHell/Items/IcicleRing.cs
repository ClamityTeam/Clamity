using CalamityMod.Rarities;
using Terraria;
using Terraria.ModLoader;

namespace Clamity.Content.Biomes.FrozenHell.Items
{
    public class IcicleRing : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 46;
            Item.accessory = true;
            Item.value = Item.sellPrice(1, 9, 9, 7);
            Item.rare = ModContent.RarityType<BurnishedAuric>();
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.Clamity().icicleRing = true;
            player.manaCost = 0;
        }
    }
}
