using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.ForbiddenLantern.Items
{
    public class AquarelPistol : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetStaticDefaults()
        {
            //CalamityItemSets.ExtraDebuffTooltip_Enemy[Type] = [BuffID.Wet, BuffID.Frostburn2];
        }
        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 40;
            Item.scale = 0.75f;
            Item.damage = 87;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2.25f;
            Item.value = CalamityGlobalItem.RarityRedBuyPrice;
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item41;
            Item.autoReuse = true;
            Item.shootSpeed = 12f;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.useAmmo = AmmoID.Bullet;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float itemRotation = player.compositeFrontArm.rotation + MathHelper.PiOver2 * player.gravDir;

            Vector2 itemPosition = player.MountedCenter + itemRotation.ToRotationVector2() * 7f;

            for (int k = 0; k < 8; k++)
            {
                CritSpark spark = new CritSpark(itemPosition + velocity.RotatedBy(-0.6 * player.direction) + velocity * 0.5f, velocity.RotatedByRandom(0.25) * Main.rand.NextFloat(0.2f, 1.5f), Main.rand.NextBool() ? Color.DeepSkyBlue : Color.LightSkyBlue, Color.White, Main.rand.NextFloat(0.3f, 0.7f), Main.rand.Next(10, 15 + 1), Main.rand.NextFloat(-2f, 2f), 1.5f);
                GeneralParticleHandler.SpawnParticle(spark);
            }
            Projectile shardShot = Projectile.NewProjectileDirect(source, itemPosition + velocity.RotatedBy(-0.6 * player.direction) - velocity * 0.5f, velocity, type, damage, knockback, player.whoAmI);
            ClamityGlobalProjectile cgp = shardShot.Clamity();
            cgp.aquarelBullet = true;

            // Ice bullet
            /*if (!swapType)
            {
                for (int k = 0; k < 8; k++)
                {
                    CritSpark spark = new CritSpark(itemPosition + velocity.RotatedBy(-0.6 * player.direction) + velocity * 0.5f, velocity.RotatedByRandom(0.25) * Main.rand.NextFloat(0.2f, 1.5f), Main.rand.NextBool() ? Color.DeepSkyBlue : Color.LightSkyBlue, Color.White, Main.rand.NextFloat(0.3f, 0.7f), Main.rand.Next(10, 15 + 1), Main.rand.NextFloat(-2f, 2f), 1.5f);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                Projectile iceShot = Projectile.NewProjectileDirect(source, itemPosition + velocity.RotatedBy(-0.6 * player.direction) - velocity * 0.5f, velocity, type, damage, knockback, player.whoAmI);
                CalamityGlobalProjectile cgp = iceShot.Calamity();
                cgp.iceBullet = true;
            }
            // Fire bullet
            if (swapType)
            {
                for (int k = 0; k < 8; k++)
                {
                    CritSpark spark = new CritSpark(itemPosition + velocity.RotatedBy(-0.6 * player.direction) + velocity * 0.5f, velocity.RotatedByRandom(0.25) * Main.rand.NextFloat(0.2f, 1.5f), Main.rand.NextBool() ? Color.Orange : Color.OrangeRed, Color.Yellow, Main.rand.NextFloat(0.3f, 0.7f), Main.rand.Next(10, 15 + 1), Main.rand.NextFloat(-2f, 2f), 1.5f);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                Projectile fireShot = Projectile.NewProjectileDirect(source, itemPosition + velocity.RotatedBy(-0.6 * player.direction) - velocity * 0.5f, velocity, type, damage, knockback, player.whoAmI);
                CalamityGlobalProjectile cgp = fireShot.Calamity();
                cgp.fireBullet = true;
            }
            for (int k = 0; k < 4; k++)
            {
                Vector2 spawnPosition = itemPosition + velocity.RotatedBy(-0.6 * player.direction) + velocity * 0.5f;
                Vector2 smokeVel = velocity.RotatedByRandom(0.25) * Main.rand.NextFloat(0.2f, 1f);
                Particle smoke = new HeavySmokeParticle(spawnPosition, smokeVel, Color.White, Main.rand.Next(40, 60 + 1), Main.rand.NextFloat(0.2f, 0.45f), 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                GeneralParticleHandler.SpawnParticle(smoke);

                Dust dust = Dust.NewDustPerfect(spawnPosition, DustID.SteampunkSteam, smokeVel.RotatedByRandom(0.15f), 80, default, Main.rand.NextFloat(0.25f, 1f));
                dust.noGravity = false;
                dust.color = Color.White;
            }*/


            return false;
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.ChangeDir(Math.Sign((player.Calamity().mouseWorld - player.Center).X));
            float itemRotation = player.compositeFrontArm.rotation + MathHelper.PiOver2 * player.gravDir;

            Vector2 itemPosition = player.MountedCenter + itemRotation.ToRotationVector2() * 7f;
            Vector2 itemSize = new Vector2(50, 32);
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
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ShatteredPistol>()
                .AddIngredient(ItemID.FragmentVortex, 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    public class AquarelPistolShard : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";


        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.timeLeft = 1;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void OnKill(int timeLeft)
        {
            Color color = Color.Cyan;

            SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.Zero) * 1.5f, false, 10, 1.5f * Projectile.scale, color * 0.75f);
            GeneralParticleHandler.SpawnParticle(spark);

            for (int i = 1; i < 4; i++)
            {
                SparkParticle miniSpark = new SparkParticle(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(.25f, .5f) * i, false, 40, Projectile.scale, color * 0.75f);
                GeneralParticleHandler.SpawnParticle(miniSpark);
            }
        }
        public override void CutTiles()
        {
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 100, Projectile.width * Projectile.scale, DelegateMethods.CutTiles);

        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float useless = 0f;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 100, 22f * Projectile.scale, ref useless))
                return true;

            return false;
        }

    }
}
