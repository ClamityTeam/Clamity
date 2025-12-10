using CalamityMod;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Potions.Food
{
    public class Barolegs : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Potions.Foods";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
        }
        public override void SetDefaults()
        {
            Item.DefaultToFood(34, 16, GetFoodBuff, 20 * 60 * 60);
            Item.value = Terraria.Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Pink;
        }
        public override void UpdateInventory(Player player)
        {
            Item.buffType = GetFoodBuff;
        }
        private int GetFoodBuff => DownedBossSystem.downedPolterghast ? BuffID.WellFed3 : (DownedBossSystem.downedLeviathan ? BuffID.WellFed2 : BuffID.WellFed);
    }
}
