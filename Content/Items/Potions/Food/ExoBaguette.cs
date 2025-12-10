using CalamityMod;
using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Potions;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Potions.Food
{
    //Increase threshold of alchohol poisoning by 1 buff
    public class ExoBaguette : Baguette, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Potions.Foods";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.buffType = ModContent.BuffType<ExoBaguetteBuff>();
            Item.rare = ModContent.RarityType<Violet>();
            Item.value += Item.sellPrice(0, 2, 40) + ModContent.GetInstance<ExoPrism>().Item.value + ModContent.GetInstance<AuricBar>().Item.value;
            Item.Calamity().donorItem = false;
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
        public override void OnConsumeItem(Player player)
        {
            player.AddBuff(ModContent.BuffType<ExoBaguetteBuff>(), CalamityUtils.SecondsToFrames(300f));
            player.AddBuff(ModContent.BuffType<BaguetteBuff>(), CalamityUtils.SecondsToFrames(600f));
            player.AddBuff(BuffID.WellFed3, CalamityUtils.SecondsToFrames(300f));
        }
    }
    public class ExoBaguetteBuff : BaguetteBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            base.Update(player, ref buffIndex);
            player.Calamity().alcoholPoisonLevel--;
        }
    }
}
