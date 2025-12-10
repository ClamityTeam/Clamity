using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Mollusk;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Typeless;
using Clamity.Content.Bosses.Clamitas.Crafted.Weapons;
using Clamity.Content.Bosses.Clamitas.Drop;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;


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


            player.maxMinions += 8;
            if (player.whoAmI == Main.myPlayer)
            {
                IEntitySource source_ItemUse = player.GetSource_ItemUse(Item);
                if (player.FindBuffIndex(ModContent.BuffType<HellstoneShellfishStaffBuff>()) == -1)
                {
                    player.AddBuff(ModContent.BuffType<HellstoneShellfishStaffBuff>(), 3600);
                }

                if (player.ownedProjectileCounts[ModContent.ProjectileType<HellstoneShellfishStaffMinion>()] < 2)
                {
                    Projectile.NewProjectileDirect(source_ItemUse, player.Center, -Vector2.UnitY, ModContent.ProjectileType<HellstoneShellfishStaffMinion>(), 130, 0f, player.whoAmI).originalDamage = 130;
                }
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
            if (ShardCountdown <= 0)
            {
                ShardCountdown = 140;
            }

            if (ShardCountdown <= 0)
            {
                return;
            }

            ShardCountdown -= Main.rand.Next(1, 4);
            if (ShardCountdown > 0 || player.whoAmI != Main.myPlayer)
            {
                return;
            }

            IEntitySource source_Accessory = player.GetSource_Accessory(Item);
            int num = 25;
            float x = Main.rand.Next(1000) - 500 + player.Center.X;
            float y = -1000f + player.Center.Y;
            Vector2 vector = new Vector2(x, y);
            Vector2 spinningpoint = player.Center - vector;
            spinningpoint.Normalize();
            spinningpoint *= num;
            int num2 = 30;
            float num3 = -60f;
            for (int i = 0; i < 2; i++)
            {
                Vector2 vector2 = vector;
                vector2.X = vector2.X + i * 30 - num2;
                Vector2 vector3 = spinningpoint.RotatedBy(MathHelper.ToRadians(num3 + 120f * i / 2f));
                vector3.X = vector3.X + 3f * Main.rand.NextFloat() - 1.5f;
                int type = 0;
                int num4 = 0;
                switch (Main.rand.Next(3))
                {
                    case 0:
                        type = ModContent.ProjectileType<PendantProjectile1>();
                        num4 = 15;
                        break;
                    case 1:
                        type = ModContent.ProjectileType<PendantProjectile2>();
                        num4 = 15;
                        break;
                    case 2:
                        type = ModContent.ProjectileType<PendantProjectile3>();
                        num4 = 30;
                        break;
                }

                int damage = (int)player.GetBestClassDamage().ApplyTo(num4);
                Projectile.NewProjectile(source_Accessory, vector2.X, vector2.Y, vector3.X / 3f, vector3.Y / 2f, type, damage, 5f, Main.myPlayer);
            }
        }
    }
}
