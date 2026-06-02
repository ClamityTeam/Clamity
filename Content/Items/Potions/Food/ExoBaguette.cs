using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Potions.Food
{
    //Increase threshold of alchohol poisoning by 1 buff
    public class ExoBaguette : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Potions.Foods";

        public static int RedWineBuffedHealValue = 250;
        public static int RedWineBuffedRegenLoss = 4;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RedWineBuffedHealValue, RedWineBuffedRegenLoss.ToRegenPerSecond());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            ItemID.Sets.FoodParticleColors[Type] = new Color[3] {
                new Color(231, 137, 159),
                new Color(179, 104, 56),
                new Color(108, 47, 16)
            };
            ItemID.Sets.IsFood[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(52, 38, BuffID.WellFed3, CalamityUtils.MinutesToFrames(5));
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<BurnishedAuric>(); ;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.Food;
        }

        public override void OnConsumeItem(Player player)
        {
            player.AddBuff(ModContent.BuffType<ExoBaguetteBuff>(), Item.buffTime);
            player.AddBuff(BuffID.WellFed3, Item.buffTime);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SoulBaguette>()
                .AddIngredient<ExoPrism>()
                .AddIngredient<AuricBar>()
                .AddTile<DraedonsForge>()
                .Register();
        }
    }

    public class ExoBaguetteBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().alcoholPoisonLevel--;
            player.Calamity().baguette = true;
        }
    }
}
