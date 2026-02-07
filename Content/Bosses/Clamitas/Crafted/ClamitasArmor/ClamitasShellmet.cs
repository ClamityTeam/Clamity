using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Mollusk;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Typeless;
using Clamity.Content.Bosses.Clamitas.Crafted.Weapons;
using Clamity.Content.Bosses.Clamitas.Drop;
using Clamity;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Mono.Cecil;


namespace Clamity.Content.Bosses.Clamitas.Crafted.ClamitasArmor
{
    [AutoloadEquip(new EquipType[] { EquipType.Head })]
    public class ClamitasShellmet : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Armor.Clamitas";
        /*public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }*/
        public override void SetDefaults()
        {
            Item.width = Item.height = 22;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.defense = 18;
        }
        public override void UpdateEquip(Player player)
        {
            player.ignoreWater = true;
            player.GetDamage<GenericDamageClass>() += 0.06f;
            player.GetCritChance<GenericDamageClass>() += 5;
            AmidiasEffect(player);

        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<ClamitasShellplate>() && legs.type == ModContent.ItemType<ClamitasShelleggings>();
        }
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = this.GetLocalizedValue("SetBonus");

            player.Calamity().wearingRogueArmor = true;

            if (player.whoAmI == Main.myPlayer)
            {
                var clam = player.GetModPlayer<ClamityPlayer>();
                clam.shellfishSetBonus = true;

                // make sure only 1 set-bonus minion exists
                if (clam.shellfishSetBonusProj == -1 ||
                    !Main.projectile[clam.shellfishSetBonusProj].active)
                {
                    int proj = Projectile.NewProjectile(
                        player.GetSource_FromThis(),
                        player.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<HellstoneShellfishStaffMinion>(),
                        130,
                        2f,
                        player.whoAmI
                    );

                    // MARK it as set-bonus
                    Main.projectile[proj].originalDamage = -1;
                    clam.shellfishSetBonusProj = proj;
                }

                player.maxMinions += 4;
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MolluskShellmet>()
                .AddIngredient<HuskOfCalamity>(10)
                .AddIngredient<AshesofCalamity>(5)
                .AddIngredient<AmidiasPendant>()
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
        public const int ShardProjectiles = 2;

        public const float ShardAngleSpread = 120f;

        public int ShardCountdown;
        private void AmidiasEffect(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                var source = player.GetSource_Accessory(Item);
                int speed2 = 25;
                float spawnX = Main.rand.Next(-300, 301) + player.Center.X;
                float spawnY = -1000 + player.Center.Y;
                Vector2 baseSpawn = new Vector2(spawnX, spawnY);
                Vector2 baseVelocity = player.Center - baseSpawn;
                baseVelocity.Normalize();
                baseVelocity *= speed2;
                int spawnOffset = ShardProjectiles * 15;
                float spread = -ShardAngleSpread / 2f;
                for (int i = 0; i < ShardProjectiles; i++)
                {
                    Vector2 spawn = baseSpawn;
                    spawn.X = spawn.X + i * 30 - spawnOffset;
                    Vector2 velocity = baseVelocity.RotatedBy(MathHelper.ToRadians(spread + (ShardAngleSpread * i / (float)ShardProjectiles)));
                    velocity.X = velocity.X + 3 * Main.rand.NextFloat() - 1.5f;

                    int finalDamage = (int)player.GetBestClassDamage().ApplyTo(30);
                    Projectile.NewProjectile(source, spawn.X, spawn.Y, velocity.X / 3, velocity.Y / 2, ModContent.ProjectileType<PearlAuraShard>(), finalDamage, 5f, Main.myPlayer);
                }
            }
        }
    }
}
