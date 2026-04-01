using CalamityMod;
using CalamityMod.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Typeless
{
    public class GoldenGun : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Typeless";
        public override void SetDefaults()
        {
            Item.damage = 25;
            Item.DamageType = ModContent.GetInstance<AverageDamageClass>();
            Item.width = 78;
            Item.height = 36;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<GoldenGunProj>();
            Item.shootSpeed = 12f;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = (ContentSamples.CreativeHelper.ItemGroup)CalamityResearchSorting.ClasslessWeapon;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-5, 0);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, 0f);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.HellstoneBar, 10).
                AddIngredient(ItemID.Ichor, 15).
                AddTile(TileID.Anvils).
                Register();
        }
    }
    public class GoldenGunProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Classless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.alpha = 255;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] < 5f)
            {
                Projectile.localAI[0] += 1f;
                return;
            }
            int inc;
            for (int i = 0; i < 1; i = inc + 1)
            {
                for (int j = 0; j < 6; j = inc + 1)
                {
                    float dustX = Projectile.velocity.X / 6f * (float)j;
                    float dustY = Projectile.velocity.Y / 6f * (float)j;
                    int dustPosMod = 6;
                    int goldenDust = Dust.NewDust(new Vector2(Projectile.position.X + (float)dustPosMod, Projectile.position.Y + (float)dustPosMod), Projectile.width - dustPosMod * 2, Projectile.height - dustPosMod * 2, 170, 0f, 0f, 75, default, 1.2f);
                    Dust dust = Main.dust[goldenDust];
                    if (Main.rand.NextBool())
                    {
                        dust.alpha += 25;
                    }
                    if (Main.rand.NextBool())
                    {
                        dust.alpha += 25;
                    }
                    if (Main.rand.NextBool())
                    {
                        dust.alpha += 25;
                    }
                    dust.noGravity = true;
                    dust.velocity *= 0.3f;
                    dust.velocity += Projectile.velocity * 0.5f;
                    dust.position = Projectile.Center;
                    dust.position.X -= dustX;
                    dust.position.Y -= dustY;
                    dust.velocity *= 0.2f;
                    inc = j;
                }
                if (Main.rand.NextBool(4))
                {
                    int dustPosMod2 = 6;
                    int moreGoldenDust = Dust.NewDust(new Vector2(Projectile.position.X + (float)dustPosMod2, Projectile.position.Y + (float)dustPosMod2), Projectile.width - dustPosMod2 * 2, Projectile.height - dustPosMod2 * 2, 170, 0f, 0f, 75, default, 0.65f);
                    Dust dust = Main.dust[moreGoldenDust];
                    dust.velocity *= 0.5f;
                    dust.velocity += Projectile.velocity * 0.5f;
                }
                inc = i;
            }
        }

        public override void OnKill(int timeLeft)
        {
            Projectile.velocity = Projectile.oldVelocity * 0.2f;
            int inc;
            for (int k = 0; k < 100; k = inc + 1)
            {
                int deathGoldenDust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 170, 0f, 0f, 75, default, 1.2f);
                Dust dust = Main.dust[deathGoldenDust];
                if (Main.rand.NextBool())
                {
                    dust.alpha += 25;
                }
                if (Main.rand.NextBool())
                {
                    dust.alpha += 25;
                }
                if (Main.rand.NextBool())
                {
                    dust.alpha += 25;
                }
                if (Main.rand.NextBool())
                {
                    dust.scale = 0.6f;
                }
                else
                {
                    dust.noGravity = true;
                }
                dust.velocity *= 0.3f;
                dust.velocity += Projectile.velocity;
                dust.velocity *= 1f + (float)Main.rand.Next(-100, 101) * 0.01f;
                dust.velocity.X += (float)Main.rand.Next(-50, 51) * 0.015f;
                dust.velocity.Y += (float)Main.rand.Next(-50, 51) * 0.015f;
                dust.position = Projectile.Center;
                inc = k;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.Ichor, 480);

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.Ichor, 480);
    }
}
