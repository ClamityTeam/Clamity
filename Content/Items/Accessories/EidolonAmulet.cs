using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Accessories
{
    public class EidolonAmulet : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 46;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 15);
            Item.rare = ItemRarityID.Pink;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer calamityPlayer = player.Calamity();

            calamityPlayer.oceanCrest = true;

            calamityPlayer.sSpiritAmulet = true;
            calamityPlayer.dOfTheDeep = true;
            player.buffImmune[ModContent.BuffType<RiptideDebuff>()] = true;
            player.buffImmune[ModContent.BuffType<CrushDepth>()] = true;

            player.Clamity().eidolonAmulet = true;
            player.buffImmune[ModContent.BuffType<Irradiated>()] = true;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ScionsCurio>()
                .AddIngredient<DiamondOfTheDeep>()
                .AddIngredient<OceanCrest>()
                .AddIngredient<CorrodedFossil>(5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
