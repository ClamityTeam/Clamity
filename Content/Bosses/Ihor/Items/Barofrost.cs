using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Ihor.Items
{
    public class Barofrost : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 44;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;

            Item.useTime = 12;
            Item.useLimitPerAnimation = 3;
            Item.useAnimation = Item.useTime * Item.useLimitPerAnimation.Value;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.reuseDelay = Item.useTime;

            Item.damage = 100;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 2.2f;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;

            Item.shoot = ModContent.ProjectileType<BarofrostProj>();
            Item.shootSpeed = 15f;

            Item.useAmmo = AmmoID.Arrow;
            //Item.Calamity().canFirePointBlankShots = true;
        }
        public override Vector2? HoldoutOffset() => new Vector2(0, 4);
        public int shootCombo = 0;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (shootCombo == 2)
            {
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<BarofrostProj2>(), damage * 2, knockback, player.whoAmI);
            }
            else
            {
                Projectile.NewProjectile(source, position + velocity.RotatedBy(-0.55f), velocity.RotatedBy(0.025f), ModContent.ProjectileType<BarofrostProj>(), damage, knockback, player.whoAmI);
                Projectile.NewProjectile(source, position + velocity.RotatedBy(0.55f), velocity.RotatedBy(-0.025f), ModContent.ProjectileType<BarofrostProj>(), damage, knockback, player.whoAmI);
            }
            shootCombo++;
            shootCombo %= 3;
            return false;
        }
    }
    public class BarofrostProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.arrow = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 2;
            //Projectile.Calamity().pointBlankShotDuration = CalamityGlobalProjectile.DefaultPointBlankDuration;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);

            Vector3 DustLight = new Vector3(0.171f, 0.124f, 0.086f);
            Lighting.AddLight(Projectile.Center, DustLight * 3);

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.timeLeft == 300)
            {
                for (int i = 0; i <= 5; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(3) ? DustID.Snow : DustID.Ice, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.1f, 0.6f), 0, default, Main.rand.NextFloat(1.2f, 1.6f));
                    dust.noGravity = true;
                }

            }

            if (Projectile.timeLeft % 2 == 0 && Projectile.timeLeft < 295 && targetDist < 1400f)
            {
                SparkParticle spark = new SparkParticle(Projectile.Center - Projectile.velocity * 2f, -Projectile.velocity * 0.1f, false, 9, 1.5f, Color.White * 0.2f);
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }
        /*public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
        }*/
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);

            for (int i = 0; i <= 5; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(3) ? DustID.Ice : DustID.SnowBlock, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(8f)) * Main.rand.NextFloat(0.1f, 0.6f), 0, default, Main.rand.NextFloat(0.9f, 1.2f));
                dust.noGravity = false;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor * 2, 1);
            return true;
        }
    }
    public class BarofrostProj2 : BarofrostProj
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.penetrate = 5;
        }
    }
}
