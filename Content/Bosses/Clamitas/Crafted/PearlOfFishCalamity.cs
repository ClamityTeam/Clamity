using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using Clamity.Content.Bosses.Clamitas.Drop;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Clamitas.Crafted
{
    public class PearlOfFishCalamity : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 26;
            Item.rare = ItemRarityID.Lime;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.fishingSkill += 50;
            player.Calamity().enchantedPearl = true;
            player.Calamity().alluringBait = true;
            ModContent.GetInstance<PearlofEnthrallment>().UpdateAccessory(player, hideVisual);
            player.Calamity().sirenWaifu = false;
        }
        public override void UpdateVanity(Player player)
        {
            ModContent.GetInstance<PearlofEnthrallment>().UpdateVanity(player);
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<EnchantedPearl>()
                .AddIngredient<AlluringBait>()
                .AddIngredient<PearlofEnthrallment>()
                .AddIngredient<ClamitousPearl>()
                .AddIngredient<MolluskHusk>(5)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}
