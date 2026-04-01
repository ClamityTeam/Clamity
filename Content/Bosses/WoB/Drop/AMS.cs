using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.WoB.Drop
{
    public class AMS : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Weapons.Classless";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 34;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<BurnishedAuric>();

            Item.useTime = Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.UseSound = new SoundStyle?(SoundID.Item1);
            Item.autoReuse = false;

            Item.damage = 100;
            Item.knockBack = 0;
            Item.mana = 12;

            Item.shoot = ModContent.ProjectileType<AMSProj>();
            Item.shootSpeed = 5f;
        }
        public int Power = 1;
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Power++;
                if (Power > 5)
                    Power = 1;
                CombatText.NewText(new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height), Color.Orange, Power);
                return true;
            }
            return base.UseItem(player);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse != 2)
            {
                int index = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                Main.projectile[index].Clamity().extraAI[0] = Power;
            }
            return false;
        }
    }
    public class AMSProj : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Classless";
        public override void SetStaticDefaults()
        {

        }
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Bomb);
            Projectile.width = Projectile.height = 32;
            Projectile.timeLeft = 300;
            AIType = ProjectileID.Bomb;
        }
        public override void PostAI()
        {
            if (Projectile.timeLeft < 2)
            {
                Projectile.damage = 100;
                Projectile.knockBack = 10f;
            }
        }
        public override void OnKill(int timeLeft)
        {
            Projectile.ExpandHitboxBy(100);
            Projectile.maxPenetrate = Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.Damage();
            SoundEngine.PlaySound(in SoundID.Item14, Projectile.Center);
            for (int i = 0; i < 40; i++)
            {
                int num = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
                Main.dust[num].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[num].scale = 0.5f;
                    Main.dust[num].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                }
            }

            for (int j = 0; j < 70; j++)
            {
                int num2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3f);
                Main.dust[num2].noGravity = true;
                Main.dust[num2].velocity *= 5f;
                num2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 2f);
                Main.dust[num2].velocity *= 2f;
            }

            if (Main.netMode != NetmodeID.Server)
            {
                Vector2 center = Projectile.Center;
                int num3 = 3;
                Vector2 position = new Vector2(center.X - 24f, center.Y - 24f);
                for (int k = 0; k < num3; k++)
                {
                    float num4 = 0.33f;
                    if (k < num3 / 3)
                    {
                        num4 = 0.66f;
                    }

                    if (k >= 2 * num3 / 3)
                    {
                        num4 = 1f;
                    }

                    int type = Main.rand.Next(61, 64);
                    int num5 = Gore.NewGore(Projectile.GetSource_Death(), position, default, type);
                    Gore obj = Main.gore[num5];
                    obj.velocity *= num4;
                    obj.velocity.X += 1f;
                    obj.velocity.Y += 1f;
                    type = Main.rand.Next(61, 64);
                    num5 = Gore.NewGore(Projectile.GetSource_Death(), position, default, type);
                    Gore obj2 = Main.gore[num5];
                    obj2.velocity *= num4;
                    obj2.velocity.X -= 1f;
                    obj2.velocity.Y += 1f;
                    type = Main.rand.Next(61, 64);
                    num5 = Gore.NewGore(Projectile.GetSource_Death(), position, default, type);
                    Gore obj3 = Main.gore[num5];
                    obj3.velocity *= num4;
                    obj3.velocity.X += 1f;
                    obj3.velocity.Y -= 1f;
                    type = Main.rand.Next(61, 64);
                    num5 = Gore.NewGore(Projectile.GetSource_Death(), position, default, type);
                    Gore obj4 = Main.gore[num5];
                    obj4.velocity *= num4;
                    obj4.velocity.X -= 1f;
                    obj4.velocity.Y -= 1f;
                }
            }

            Projectile.ExpandHitboxBy(15);
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.ExplodeTiles(5 + (int)(Projectile.Clamity().extraAI[0] - 1) * 2, false);
            }
        }
    }
}
