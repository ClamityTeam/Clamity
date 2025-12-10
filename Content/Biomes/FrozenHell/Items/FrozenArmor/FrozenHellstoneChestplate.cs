using CalamityMod;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Biomes.FrozenHell.Items.FrozenArmor
{
    [AutoloadEquip(EquipType.Body)]
    public class FrozenHellstoneChestplate : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Armor.FrozenHellstone";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 22;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ModContent.RarityType<Violet>();
            Item.defense = 64;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(ModContent.GetInstance<TrueMeleeDamageClass>()) += 0.2f;
            player.GetAttackSpeed<MeleeDamageClass>() += 0.2f;
            player.aggro += 400;
        }

        public override void AddRecipes() => CreateRecipe().AddIngredient(ItemID.MoltenBreastplate)
                                                           .AddIngredient(ItemID.FrostBreastplate)
                                                           .AddIngredient<EnchantedMetal>(20)
                                                           .AddIngredient<EndothermicEnergy>(32)
                                                           .AddTile(TileID.Hellforge)
                                                           .Register();
    }
}
