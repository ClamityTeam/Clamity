using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Mollusk;
using CalamityMod.Items.Materials;
using Clamity.Content.Bosses.Clamitas.Drop;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace Clamity.Content.Bosses.Clamitas.Crafted.ClamitasArmor
{
    [AutoloadEquip(new EquipType[] { EquipType.Body })]
    public class ClamitasShellplate : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Armor.Clamitas";
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 22;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.defense = 22;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<GenericDamageClass>() += 0.12f;
            player.GetCritChance<GenericDamageClass>() += 7;
            player.Calamity().giantPearl = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MolluskShellplate>()
                .AddIngredient<HuskOfCalamity>(20)
                .AddIngredient<AshesofCalamity>(8)
                .AddIngredient<GiantPearl>()
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
