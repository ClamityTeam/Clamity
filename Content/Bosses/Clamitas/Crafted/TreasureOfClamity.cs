using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using Clamity.Content.Bosses.Clamitas.Drop;
using Clamity.Content.Items.Accessories;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Clamitas.Crafted
{
    [LegacyName("PearlOfFishCalamity")]
    public class TreasureOfClamity : ToggableAccessory
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
        public override void SafeUpdateAccessory(Player player, bool hideVisual)
        {
            player.fishingSkill += 25;
            player.Calamity().enchantedPearl = true;
            player.Calamity().alluringBait = true;
        }
        public override void ToggledUpdateAccessory(Player player, bool hideVisual)
        {
            ModContent.GetInstance<PearlofEnthrallment>().UpdateAccessory(player, hideVisual);
            player.Calamity().sirenPet = false;
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
