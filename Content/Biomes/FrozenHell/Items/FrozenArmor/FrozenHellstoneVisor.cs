using CalamityMod;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Biomes.FrozenHell.Items.FrozenArmor
{
    [AutoloadEquip(EquipType.Head)]
    public class FrozenHellstoneVisor : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Armor.FrozenHellstone";

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ModContent.RarityType<BurnishedAuric>();
            Item.defense = 60;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<FrozenHellstoneChestplate>() && legs.type == ModContent.ItemType<FrozenHellstoneGreaves>();

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(ModContent.GetInstance<TrueMeleeDamageClass>()) += 0.2f;
            player.Clamity().inflicingMeleeFrostburn = true;
        }

        public override void UpdateArmorSet(Player player)
        {
            var hotkey = CalamityKeybinds.ArmorSetBonusHotKey.TooltipHotkeyString();
            player.setBonus = this.GetLocalization("SetBonus").Format(hotkey);

            //player.setBonus = "Cannot be frozen.\nPress Armor Set Bonus to create an ice shield that parries attacks.[WIP]\nFailing to parry will cause you to overcool.[WIP]";
            player.Clamity().frozenParrying = true;
            player.buffImmune[44] = true;
            player.buffImmune[324] = true;
            player.buffImmune[47] = true;
            player.aggro += 400;
        }

        public static void HandleParryCountdown(Player player)
        {
            player.Clamity().frozenParryingTime--;

            if (player.Clamity().frozenParryingTime > 0)
            {
                /*player.controlJump = false;
                player.controlDown = false;
                player.controlLeft = false;
                player.controlRight = false;
                player.controlUp = false;
                player.controlUseItem = false;
                player.controlUseTile = false;
                player.controlThrow = false;
                player.gravDir = 1f;
                player.velocity = Vector2.Zero;
                player.velocity.Y = -0.1f; //if player velocity is 0, the flight meter gets reset
                player.RemoveAllGrapplingHooks();*/
            }
            else
            {
                /*for (int i = 0; i < 8; i++)
                {
                    int theDust = Dust.NewDust(player.position, player.width, player.height, (int)CalamityDusts.Brimstone, 0f, 0f, 100, new Color(255, 255, 255), 2f);
                    Main.dust[theDust].noGravity = true;
                }*/
            }
        }


        public override void AddRecipes() => CreateRecipe().AddIngredient(ItemID.MoltenHelmet)
                                                           .AddIngredient(ItemID.FrostHelmet)
                                                           .AddIngredient<EndobsidianBar>(10)
                                                           .AddIngredient<EndothermicEnergy>(18)
                                                           .AddTile(TileID.Hellforge)
                                                           .Register();
    }
}
