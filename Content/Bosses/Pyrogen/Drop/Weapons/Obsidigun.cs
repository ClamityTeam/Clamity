using CalamityMod;
using CalamityMod.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Pyrogen.Drop.Weapons
{
    public class Obsidigun : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetDefaults()
        {
            Item.width = 74;
            Item.height = 32;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;

            Item.useTime = Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item11;
            Item.noMelee = true;

            Item.shoot = ModContent.ProjectileType<ObsidigunBullet>();
            Item.shootSpeed = 9f;
            Item.useAmmo = AmmoID.Bullet;

            Item.damage = 48;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 6f;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (type == ProjectileID.Bullet)
                type = ModContent.ProjectileType<ObsidigunBullet>();
        }
        public override Vector2? HoldoutOffset() => new Vector2(-10f, -5f);
    }
    public class ObsidigunBullet : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override void SetDefaults()
        {
            //Projectile.CloneDefaults(ProjectileID.Bullet);
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 3;
            AIType = ProjectileID.Bullet;
            //Projectile.Calamity().pointBlankShotDuration = 18;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2CircularEdge(20, 20) * Main.rand.NextFloat(0.7f, 1f), ModContent.ProjectileType<ObsidigunBulletShard>(), Projectile.damage / 10, Projectile.knockBack / 5, Projectile.owner);

            base.OnKill(timeLeft);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Projectile.DrawProjectileWithBackglow(new Color(98, 101, 180), lightColor, 2f);
            return base.PreDraw(ref lightColor);
        }
    }
    public class ObsidigunBulletShard : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.CrystalShard);
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.ai[0] == 1)
                Projectile.DamageType = DamageClass.Melee;
        }
        public override void PostAI()
        {
            base.PostAI();
            if (Projectile.DamageType == ModContent.GetInstance<RogueDamageClass>())
            {
                Projectile.velocity.Y += 0.2f;
                if (Projectile.Clamity().extraAI[0] > 0)
                    Projectile.Clamity().extraAI[0]--;
                else
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Flare, Projectile.velocity.RotatedByRandom(0.3f) / 4f, Scale: 2f);
                    dust.noGravity = true;
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Projectile.DrawProjectileWithBackglow(new Color(98, 101, 180), lightColor, 2f);
            return base.PreDraw(ref lightColor);
        }
    }
}
