using CalamityMod;
using CalamityMod.Dusts;
using CalamityMod.Items;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Pyrogen.Drop.Weapons
{
    [LegacyName("Obsidigun")]
    public class FireworkRifle : ModItem, ILocalizedModType, IModType
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

            //Item.shoot = ModContent.ProjectileType<ObsidigunBullet>();
            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 9f;
            Item.useAmmo = AmmoID.Bullet;

            Item.damage = 48;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 6f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float itemRotation = player.compositeFrontArm.rotation + MathHelper.PiOver2 * player.gravDir;

            Vector2 itemPosition = player.MountedCenter + itemRotation.ToRotationVector2() * 7f;

            /*for (int k = 0; k < 8; k++)
            {
                CritSpark spark = new CritSpark(itemPosition + velocity.RotatedBy(-0.6 * player.direction) + velocity * 0.5f, velocity.RotatedByRandom(0.25) * Main.rand.NextFloat(0.2f, 1.5f), Main.rand.NextBool() ? Color.DeepSkyBlue : Color.LightSkyBlue, Color.White, Main.rand.NextFloat(0.3f, 0.7f), Main.rand.Next(10, 15 + 1), Main.rand.NextFloat(-2f, 2f), 1.5f);
                GeneralParticleHandler.SpawnParticle(spark);
            }*/
            Projectile shardShot = Projectile.NewProjectileDirect(source, itemPosition + velocity.RotatedBy(-0.6 * player.direction) - velocity * 0.5f, velocity, type, damage, knockback, player.whoAmI);
            shardShot.extraUpdates += 1;
            ClamityGlobalProjectile cgp = shardShot.Clamity();
            cgp.fireworkBullet = true;

            return false;
        }
        public override Vector2? HoldoutOffset() => new Vector2(-10f, -5f);
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.ChangeDir(Math.Sign((player.Calamity().mouseWorld - player.Center).X));
            float itemRotation = player.compositeFrontArm.rotation + MathHelper.PiOver2 * player.gravDir;

            float animProgress = 0.5f - player.itemTime / (float)player.itemTimeMax;

            Vector2 itemPosition = player.MountedCenter + itemRotation.ToRotationVector2() * (7f + animProgress);
            Vector2 itemSize = new Vector2(44, 28);
            Vector2 itemOrigin = new Vector2(-24, 3);

            CalamityUtils.CleanHoldStyle(player, itemRotation, itemPosition, itemSize, itemOrigin);

            base.UseStyle(player, heldItemFrame);
        }

        public override void UseItemFrame(Player player)
        {
            player.ChangeDir(Math.Sign((player.Calamity().mouseWorld - player.Center).X));

            float animProgress = 0.5f - player.itemTime / (float)player.itemTimeMax;
            float rotation = (player.Center - player.Calamity().mouseWorld).ToRotation() * player.gravDir + MathHelper.PiOver2;
            if (animProgress < 0.4f)
                rotation += -0.05f * (float)Math.Pow((0.6f - animProgress) / 0.6f, 2) * player.direction;

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation);
        }
    }
    public class FireworkRifleExplosion : ModProjectile, ILocalizedModType, IModType
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public new string LocalizationCategory => "Projectiles.Ranged";
        public int TotalTimeLeft => 120;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 150;
            Projectile.aiStyle = -1;
            AIType = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = TotalTimeLeft + 5;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = TotalTimeLeft;

        }
        public override void AI()
        {
            if (Projectile.timeLeft == TotalTimeLeft)
            {
                int lineCount = 8;

                float random = Main.rand.NextFloat(MathHelper.Pi);
                for (float i = random; i < MathHelper.TwoPi + random; i += (MathHelper.TwoPi / lineCount))
                {
                    for (int j = 0; j < 10; j+=1)
                    {
                        float v = 0.0f;
                        float a = 0.1f;
                        Vector2 vec1 = Vector2.UnitY.RotatedBy(i) * j + new Vector2(0, v * j + a / 2 * MathF.Pow(j, 2));

                        Dust laserDust2 = Dust.NewDustPerfect(Projectile.Center + vec1, ModContent.DustType<LightDust>(), vec1, Scale: Main.rand.NextFloat(0.8f, 1.1f));
                        laserDust2.noGravity = true;
                        laserDust2.color = Color.DarkMagenta;
                        //laserDust2.noLightEmittence = true;
                    }
                }
            }
            if (Projectile.timeLeft < 15)
            {
                for (float i = 0; i < 3; i++)
                {
                    Dust laserDust2 = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2, Projectile.height / 2), ModContent.DustType<LightDust>(), Main.rand.NextVector2Circular(1, 1), Scale: Main.rand.NextFloat(0.8f, 1.1f));
                    laserDust2.noGravity = true;
                    laserDust2.color = Color.DarkMagenta;
                    //laserDust2.
                }
            }
        }

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
