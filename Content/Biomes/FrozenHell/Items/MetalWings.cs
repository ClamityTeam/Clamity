using CalamityMod.Items;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Biomes.FrozenHell.Items
{
    [AutoloadEquip(EquipType.Wings)]
    public class MetalWings : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories.Wings";
        public override void SetStaticDefaults()
        {
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(280, 10.5f, 2.8f);
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 26;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<BurnishedAuric>();
            Item.accessory = true;
        }
        //When you getting hit, you receive flight time percentages equal to half of the ratio of damage received to max HP
        //While wearing Frozen Armor, you and players in your team gets a 8 increased defence
        public override void UpdateAccessory(Player player, bool hideVisual)
        {

            if (player.controlJump && player.wingTime > 0f && player.jump == 0)
            {
                bool hovering = player.TryingToHoverDown && !player.merman;
                if (hovering)
                {
                    player.velocity.Y *= 0.2f;
                    if (player.velocity.Y > -2f && player.velocity.Y < 2f)
                    {
                        // I can't get the player to have zero y velocity (setting it to 0 doesn't work and I tried a lot of numbers)
                        player.velocity.Y = 0.105f;
                    }
                }

                if (player.velocity.Y != 0f && !hideVisual)
                {
                    float xOffset = 4f;
                    if (player.direction == 1)
                    {
                        xOffset = -40f;
                    }
                    if (!hovering || Main.rand.NextBool(3))
                    {
                        int idx = Dust.NewDust(new Vector2(player.Center.X + xOffset, player.Center.Y - 15f), 30, 30, DustID.GemAmethyst, 0f, 0f, 100, default, 1.75f);
                        Main.dust[idx].noGravity = true;
                        Main.dust[idx].velocity *= 0.3f;
                        if (Main.rand.NextBool(10))
                        {
                            Main.dust[idx].fadeIn = 2f;
                        }
                        Main.dust[idx].shader = GameShaders.Armor.GetSecondaryShader(player.cWings, player);
                    }
                }
            }
            player.noFallDmg = true;


            player.Clamity().metalWings = true;
            if (player.Clamity().frozenParrying)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (player.team == Main.player[i].team)
                        player.statDefense += 20;

                }
            }
            //player.statDefense += 8;

        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            ascentWhenFalling = 0.90f;
            ascentWhenRising = 0.16f;
            maxCanAscendMultiplier = 1.1f;
            maxAscentMultiplier = 3.2f;
            constantAscend = 0.145f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SoulofFlight, 20)
                .AddIngredient<MetalFeather>()
                .AddIngredient<EnchantedMetal>(5)
                .AddTile<CosmicAnvil>()
                .Register();
        }
    }
}
