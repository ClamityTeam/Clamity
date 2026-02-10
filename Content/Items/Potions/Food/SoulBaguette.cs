using CalamityMod;
using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Potions.Food;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Potions.Food
{
    public class SoulBaguette : Baguette, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Potions.Foods";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.buffTime *= 2;
            Item.rare = ItemRarityID.Pink;
            Item.value += Item.sellPrice(0, 2, 40);
            Item.Calamity().donorItem = false;
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
        public override void OnConsumeItem(Player player)
        {
            player.AddBuff(ModContent.BuffType<BaguetteBuff>(), CalamityUtils.SecondsToFrames(600f));
            player.AddBuff(BuffID.WellFed2, CalamityUtils.SecondsToFrames(300f));
        }
    }
}
