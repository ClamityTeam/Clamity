using CalamityMod.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Accessories.Sentry
{
    public class CyanPearl : ModItem, ILocalizedModType, IModType, IHoldShiftTooltipItem
    {
        public new string LocalizationCategory => "Items.Accessories";
        //public bool HasFlavorTooltip => true;
        public Color? TooltipExtensionColor => new(195, 223, 255);
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 22;
            Item.accessory = true;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage<SummonDamageClass>() += 0.07f;
            player.Clamity().cyanPearl = true;
        }
    }
}
