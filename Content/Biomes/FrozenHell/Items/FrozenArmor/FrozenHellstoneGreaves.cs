using CalamityMod;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Clamity.Content.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Biomes.FrozenHell.Items.FrozenArmor
{
    [AutoloadEquip(EquipType.Legs)]
    public class FrozenHellstoneGreaves : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Armor.FrozenHellstone";
        public override void SetStaticDefaults()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                ArmorIDs.Legs.Sets.HidesBottomSkin[EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs)] = true;
            }
        }
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 22;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ModContent.RarityType<EvercoldCyan>();
            Item.defense = 36;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(ModContent.GetInstance<TrueMeleeDamageClass>()) += 0.2f;
            player.statLifeMax2 += 75;
            player.aggro += 400;
        }

        public override void AddRecipes() => CreateRecipe().AddIngredient(ItemID.MoltenGreaves)
                                                           .AddIngredient(ItemID.FrostLeggings)
                                                           .AddIngredient<EndobsidianBar>(15)
                                                           .AddIngredient<EndothermicEnergy>(24)
                                                           .AddTile(TileID.Hellforge)
                                                           .Register();
    }
}
