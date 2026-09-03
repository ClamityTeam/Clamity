using CalamityMod.Items;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Accessories
{
    public class IgnitedSubcommunity : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override bool IsLoadingEnabled(Mod mod)
        {
            return false;
        }
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 4));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 64;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.accessory = true;
            Item.rare = ItemRarityID.Lime;
        }
    }
}
