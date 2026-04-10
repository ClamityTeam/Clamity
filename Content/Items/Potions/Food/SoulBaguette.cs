using CalamityMod;
using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Potions.Food;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Potions.Food
{
    public class SoulBaguette : ModItem, ILocalizedModType, IModType
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
            Item.DefaultToFood(52, 38, BuffID.WellFed2, CalamityUtils.MinutesToFrames(5));
            Item.value = Item.sellPrice(0, 2, 41);
            Item.rare = ItemRarityID.Pink;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.Food;
        }

        public override void OnConsumeItem(Player player)
        {
            player.AddBuff(BuffID.WellFed2, Item.buffTime);
            player.AddBuff(ModContent.BuffType<BaguetteBuff>(), Item.buffTime);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<Baguette>()
                .AddIngredient(ItemID.SoulofMight)
                .AddIngredient(ItemID.SoulofSight)
                .AddIngredient(ItemID.SoulofFright)
                .AddTile(TileID.AdamantiteForge)
                .Register();
        }
    }
}
