using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.Cooldowns;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Accessories
{
    public class SkullOfTheBloodGod : ToggableAccessory
    {
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 4));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = Item.height = 48;
            Item.accessory = true;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<CosmicPurple>();
            Item.expert = true;
        }
        int cooldown = 0;

        public override void ToggledUpdateAccessory(Player player, bool hideVisual)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                var source = player.GetSource_Accessory(Item);
                if (player.immune)
                {
                    if (player.miscCounter % 10 == 0)
                    {
                        int damage = (int)player.GetBestClassDamage().ApplyTo(120);
                        CalamityUtils.ProjectileRain(source, player.Center, 400f, 100f, 500f, 800f, 22f, ModContent.ProjectileType<StandingFire>(), damage, 5f, player.whoAmI);
                    }
                }
            }
        }

        public override void SafeUpdateAccessory(Player player, bool hideVisual)
        {
            player.Clamity().skullOfBloodGod = true;

            CalamityPlayer modPlayer = player.Calamity();

            modPlayer.fleshTotem = true;
            modPlayer.fleshTotemMinion = true;
            modPlayer.fleshTotemVisual = !hideVisual;
            player.statManaMax2 += 20;
            player.GetCritChance<MagicDamageClass>() += 7;
            if (player.whoAmI == Main.myPlayer)
            {
                var source = player.GetSource_Accessory(Item);
                if (player.ownedProjectileCounts[ModContent.ProjectileType<FleshTotemMinion>()] < 1)
                {
                    int damage = (int)player.GetTotalDamage<MagicDamageClass>().ApplyTo(FleshTotem.lostSoulDamage);

                    int effigy = Projectile.NewProjectile(source, player.Center, -Vector2.UnitY, ModContent.ProjectileType<FleshTotemMinion>(), damage, 2f, Main.myPlayer);
                    if (Main.projectile.IndexInRange(effigy))
                        Main.projectile[effigy].originalDamage = FleshTotem.lostSoulDamage;
                }
            }

            modPlayer.apollyon = true;
            modPlayer.abaddonEffectVisual = !hideVisual;

            modPlayer.voidOfCalamity = true;
            player.GetDamage<GenericDamageClass>() += 0.15f;
            if (player.whoAmI == Main.myPlayer)
            {
                var source = player.GetSource_Accessory(Item);
                if (player.HasIFrames())
                {
                    if (cooldown <= 0)
                    {
                        cooldown = 20;
                        int damage = (int)player.GetBestClassDamage().ApplyTo(VoidofCalamity.BrimstoneFlamesDmg);
                        for (var i = 0; i < 2; i++)
                            CalamityUtils.ProjectileRain(source, player.Center, 400f, 100f, 500f, 800f, 5.5f, ModContent.ProjectileType<StandingFire>(), damage, 5f, player.whoAmI);
                    }
                }
                cooldown--;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<FleshTotem>()
                .AddIngredient<Apollyon>()
                .AddIngredient<VoidofCalamity>()
                .AddIngredient<BloodstoneCore>(4)
                .AddIngredient<AscendantSpiritEssence>(5)
                .AddTile<CosmicAnvil>()
                .Register();
        }
    }
}
