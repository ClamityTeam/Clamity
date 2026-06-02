using CalamityMod.Items.LoreItems;
using CalamityMod.Rarities;
using Terraria;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.WoB.Drop
{
    public class LoreWallOfBronze : LoreItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 26;
            Item.rare = ModContent.RarityType<BurnishedAuric>();
            Item.consumable = false;
        }
    }
}
