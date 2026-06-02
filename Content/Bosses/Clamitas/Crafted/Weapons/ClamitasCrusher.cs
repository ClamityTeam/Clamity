using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.Melee;
using Clamity.Content.Bosses.Clamitas.Drop;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace Clamity.Content.Bosses.Clamitas.Crafted.Weapons
{
    public class ClamitasCrusher : ClamCrusher
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.rare = ItemRarityID.Lime;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.damage = 130;
            Item.shoot = ModContent.ProjectileType<ClamitasCrusherProjectile>();
            Item.shootSpeed = 25f;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ClamCrusher>()
                .AddIngredient<HuskOfCalamity>(12)
                .AddIngredient<AshesofCalamity>(6)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class ClamitasCrusherProjectile : ClamCrusherFlail
    {
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 180);
            if (Projectile.ai[0] == 0)
                for (int i = 0; i < 5; i++)
                {
                    Vector2 velocity = CalamityUtils.RandomVelocity(100f, 70f, 100f);
                    int index = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<DepthsEchoRifleProjectileSplit>(), (int)(Projectile.damage * 0.2), 0f, Projectile.owner);
                    Main.projectile[index].DamageType = DamageClass.MeleeNoSpeed;
                }
            base.OnHitNPC(target, hit, damageDone);

        }
        public override void PostAI()
        {
            Projectile.ai[2]++;
            if (Projectile.ai[0] == 0 && Projectile.ai[2] % 3 == 0)
            {
                int index = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedByRandom(MathHelper.PiOver4) / 2f, ModContent.ProjectileType<DepthsEchoRifleProjectileSplit>(), (int)(Projectile.damage * 0.1), 0f, Projectile.owner);
                Main.projectile[index].DamageType = DamageClass.MeleeNoSpeed;

            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 velocity = CalamityUtils.RandomVelocity(100f, 70f, 100f);
                int index = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<DepthsEchoRifleProjectileSplit>(), (int)(Projectile.damage * 0.4), 0f, Projectile.owner);
                Main.projectile[index].DamageType = DamageClass.MeleeNoSpeed;
            }
            return base.OnTileCollide(oldVelocity);
        }
    }
}
