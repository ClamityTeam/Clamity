using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Items;
using CalamityMod.Items.Placeables;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Projectiles.Ranged;
using Clamity.Content.Bosses.Clamitas.Drop;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;


namespace Clamity.Content.Bosses.Clamitas.Crafted.Weapons
{
    public class DepthsEchoRifle : ClamorRifle
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.rare = ItemRarityID.Lime;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.damage = 40;
            Item.useTime = Item.useAnimation = 10;
            Item.shoot = ModContent.ProjectileType<DepthsEchoRifleProjectile>();
            Item.shootSpeed = 20f;

        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (type == 14)
            {
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<DepthsEchoRifleProjectile>(), damage, knockback, player.whoAmI);
            }
            else
            {
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            }

            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ClamorRifle>()
                .AddIngredient<HuskOfCalamity>(6)
                .AddIngredient<BrimstoneSlag>(15)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

    }
    public class DepthsEchoRifleProjectile : ClamorRifleProj
    {
        public override void AI()
        {
            Projectile.rotation += 0.15f;
            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] > 3f)
            {
                Lighting.AddLight(Projectile.Center, new Vector3(191f, 0, 0) * 0.005098039f);
                for (int i = 0; i < 2; i++)
                {
                    int num = 14;
                    int num2 = Dust.NewDust(new Vector2(Projectile.Center.X, Projectile.Center.Y), Projectile.width - num * 2, Projectile.height - num * 2, ModContent.DustType<BrimstoneFlame>(), 0f, 0f, 100);
                    Main.dust[num2].noGravity = true;
                    Main.dust[num2].velocity *= 0.1f;
                    Main.dust[num2].velocity += Projectile.velocity * 0.5f;
                }
            }

            CalamityUtils.HomeInOnNPC(Projectile, !Projectile.tileCollide, 150f, 12f, 25f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 180);
        }
        public override void OnKill(int timeLeft)
        {
            int num = 3;
            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < num; i++)
                {
                    Vector2 velocity = CalamityUtils.RandomVelocity(100f, 70f, 100f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<DepthsEchoRifleProjectileSplit>(), (int)(Projectile.damage * 0.4), 0f, Projectile.owner);
                }
            }

            SoundEngine.PlaySound(in SoundID.Item118, Projectile.Center);
        }
    }
    public class DepthsEchoRifleProjectileSplit : ClamorRifleProjSplit
    {
        public override string Texture => ModContent.GetInstance<DepthsEchoRifleProjectile>().Texture;
        public override void AI()
        {
            Projectile.rotation += 0.15f;
            Lighting.AddLight(Projectile.Center, new Vector3(191f, 0, 0) * 0.005098039f);
            for (int i = 0; i < 2; i++)
            {
                int num = 14;
                int num2 = Dust.NewDust(new Vector2(Projectile.Center.X, Projectile.Center.Y), Projectile.width - num * 2, Projectile.height - num * 2, ModContent.DustType<BrimstoneFlame>(), 0f, 0f, 100);
                Main.dust[num2].noGravity = true;
                Main.dust[num2].velocity *= 0.1f;
                Main.dust[num2].velocity += Projectile.velocity * 0.5f;
            }

            if (Projectile.timeLeft < 150)
            {
                CalamityUtils.HomeInOnNPC(Projectile, !Projectile.tileCollide, 450f, 12f, 25f);
            }
        }
    }
}
