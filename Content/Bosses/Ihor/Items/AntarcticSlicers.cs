using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Clamity.Content.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Ihor.Items
{
    public class AntarcticSlicers : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public bool AltProjectile = true;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 43;
            Item.height = 34;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.damage = 35;
            Item.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();
            Item.useTime = 4;
            Item.useAnimation = 4;
            Item.shoot = ModContent.ProjectileType<AntarcticSlicersBolt>();
            Item.shootSpeed = 3.3f;
            Item.knockBack = 6f;
            Item.UseSound = null;
            Item.channel = true;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
        }
        public override bool AltFunctionUse(Player player) => true;
        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useTime = 1;
                Item.useAnimation = 1;
                //Item.useLimitPerAnimation = player.Clamity().antarcticSlicersBolts;
                //Item.reuseDelay = 10;
            }
            else
            {
                Item.useTime = 4;
                Item.useAnimation = 4;
                //Item.useLimitPerAnimation = 1;
                //Item.reuseDelay = 0;
            }
            return base.UseItem(player);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2) // Bolts
            {
                Item.useStyle = ItemUseStyleID.Swing;
                if (player.Clamity().antarcticSlicersBolts > 0)
                {
                    SoundEngine.PlaySound(SoundID.DD2_GhastlyGlaivePierce with { Pitch = 1.5f }, player.Center);
                    Vector2 vel = velocity.RotatedByRandom(0.4f);
                    Projectile.NewProjectile(source, position + vel * 20, vel * 0.1f, ModContent.ProjectileType<AntarcticSlicersSlash>(), (int)(damage * 1.2), knockback * 1.2f, player.whoAmI, 1);
                    player.Clamity().antarcticSlicersBolts--;
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.Item7 with { Pitch = 0.2f }, player.Center);
                }
            }
            else // Shortswords
            {
                Item.useStyle = ItemUseStyleID.Shoot;
                if (AltProjectile)
                {
                    SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.4f }, player.Center);
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<AntarcticSlicersBladeAlt>(), damage, knockback, player.whoAmI);
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.9f }, player.Center);
                    Projectile.NewProjectile(source, position, velocity * 0.75f, ModContent.ProjectileType<AntarcticSlicersBlade>(), damage, knockback, player.whoAmI);
                }
                AltProjectile = !AltProjectile;
            }
            return false;
        }
        public override bool MeleePrefix() => true;
    }
    public class AntarcticSlicersBolt : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public ref int Bolts => ref Main.player[Projectile.owner].Clamity().antarcticSlicersBolts;
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.penetrate = 2;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float playerDist = Vector2.Distance(Owner.Center, Projectile.Center);

            // Shot Mode
            if (Projectile.ai[0] == 1)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                Projectile.extraUpdates = 6;
                if (Main.rand.NextBool(2))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5), Main.rand.NextBool() ? 288 : 207);
                    dust.scale = Main.rand.NextFloat(0.2f, 0.45f);
                    dust.noGravity = true;
                    dust.velocity = -Projectile.velocity * 0.5f;
                }
                if (Projectile.timeLeft % 2 == 0 && playerDist < 1400f)
                {
                    SparkParticle spark = new SparkParticle(Projectile.Center - Projectile.velocity * 3f, -Projectile.velocity * 0.05f, false, 10, 1f, Color.White * 0.135f);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
            else // "Quiver" mode
            {
                // Refresh lifetime
                Projectile.timeLeft = 4;

                // If the player uses a bolt, destroy the most recent bolt
                if (Bolts < Projectile.ai[1])
                    Projectile.Kill();

                // Setting the bolt's position on the player's back
                Projectile.rotation = (21.8f - (Projectile.ai[1] * 0.1f)) * -Owner.direction - MathHelper.PiOver4;
                //Projectile.rotation = Owner.direction == 1 ? (21.8f - (Projectile.ai[1] * 0.1f)) : MathHelper.Pi - (21.8f - (Projectile.ai[1] * 0.1f));
                Vector2 BoltPos = Owner.MountedCenter + new Vector2((10 + Projectile.ai[1] * 2.5f) * -Owner.direction, 3f - Projectile.ai[1]);

                Projectile.Center = BoltPos;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i <= 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? 288 : 207, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.3f, 1.9f));
                dust.noGravity = false;
                dust.scale = Main.rand.NextFloat(0.6f, 0.9f);
                Dust dust2 = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? 288 : 207, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(35f)) * Main.rand.NextFloat(0.05f, 0.9f));
                dust2.noGravity = false;
                dust2.scale = Main.rand.NextFloat(0.6f, 0.9f);
            }
        }
        public override void OnKill(int timeLeft)
        {
            if (Projectile.ai[0] == 1)
            {
                SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
                for (int i = 0; i <= 5; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? 216 : 207, -Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.2f, 1f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(1.1f, 1.8f);
                    Dust dust2 = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? 216 : 207, -Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(35f)) * Main.rand.NextFloat(0.05f, 0.4f));
                    dust2.noGravity = true;
                    dust2.scale = Main.rand.NextFloat(1.1f, 1.8f);
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.ai[0] == 1)
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            CalamityUtils.DrawAfterimagesCentered(Projectile, 3, lightColor, 1);
            return false;
        }
        public override bool? CanDamage() => Projectile.ai[0] == 1 ? true : false;
    }
    public class AntarcticSlicersSlash : BaseSlash
    {
        public override Color FirstColor => Color.LightCyan;
        public override Color SecondColor => Color.Black;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();
        }
    }
    public class AntarcticSlicersBlade : BaseShortswordProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<AntarcticSlicers>();
        //public override string Texture => ModContent.GetInstance<AntarcticSlicersBolt>().Texture;
        public new string LocalizationCategory => "Projectiles.Melee";
        public Player player => Main.player[Projectile.owner];
        public ref int Bolts => ref Main.player[Projectile.owner].Clamity().antarcticSlicersBolts;
        public override float FadeInDuration => 3f;
        public override float FadeOutDuration => 0f;
        public override float TotalDuration => 4f;
        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(15);
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.scale = 1f;
            Projectile.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();
            Projectile.ownerHitCheck = true;
            Projectile.timeLeft = 360;
            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void SetVisualOffsets()
        {
            const int HalfSpriteWidth = 32 / 2;
            const int HalfSpriteHeight = 32 / 2;

            int HalfProjWidth = Projectile.width / 2;
            int HalfProjHeight = Projectile.height / 2;

            DrawOriginOffsetX = 0;
            DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
            DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);
        }

        public override void ExtraBehavior()
        {
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12, 12), DustID.SnowBlock);
                dust.scale = Main.rand.NextFloat(0.15f, 0.6f);
                dust.noGravity = true;
                dust.velocity = -Projectile.velocity * 0.5f;
            }

            float armPointingDirection = ((Owner.Calamity().mouseWorld - Owner.MountedCenter).ToRotation());
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armPointingDirection - MathHelper.PiOver2);
            Owner.heldProj = Projectile.whoAmI;

            Projectile.Center = Projectile.Center + new Vector2(0, 1.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Add to bolt counter, spawn 2 quiver bolts, and on hit visual
            if (Bolts < 15)
            {
                Bolts++;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.Center, Vector2.Zero, ModContent.ProjectileType<AntarcticSlicersBolt>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0, Bolts);
                /*if (Bolts < 15)
                {
                    Bolts++;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.Center, Vector2.Zero, ModContent.ProjectileType<AntarcticSlicersBolt>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0, Bolts);
                }*/
            }
            else if (Main.rand.NextBool(2))
            {
                Vector2 vel = (Projectile.Center - player.Center).SafeNormalize(Vector2.Zero).RotatedByRandom(0.4f);
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), player.Center + vel * 20, vel * 0.1f, ModContent.ProjectileType<AntarcticSlicersSlash>(), (int)(Projectile.damage * 1.1), Projectile.knockBack * 1.2f, player.whoAmI, 1);
            }
            float numberOfDusts = 5f;
            float rotFactor = 360f / numberOfDusts;
            for (int i = 0; i < numberOfDusts; i++)
            {
                float rot = MathHelper.ToRadians(i * rotFactor);
                Vector2 offset = new Vector2(Main.rand.NextFloat(0.5f, 2.5f), 0).RotatedBy(rot * Main.rand.NextFloat(1.1f, 9.1f));
                Vector2 velOffset = new Vector2(Main.rand.NextFloat(0.5f, 2.5f), 0).RotatedBy(rot * Main.rand.NextFloat(1.1f, 9.1f));
                Dust dust = Dust.NewDustPerfect(target.Center + offset, Main.rand.NextBool() ? DustID.SnowBlock : DustID.Ice, new Vector2(velOffset.X, velOffset.Y));
                dust.noGravity = false;
                dust.velocity = velOffset;
                dust.scale = Main.rand.NextFloat(1.5f, 1.2f);
            }
        }

    }
    public class AntarcticSlicersBladeAlt : BaseShortswordProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<AntarcticSlicers>();
        //public override string Texture => ModContent.GetInstance<AntarcticSlicersBolt>().Texture;
        public new string LocalizationCategory => "Projectiles.Melee";
        public Player player => Main.player[Projectile.owner];
        public ref int Bolts => ref Main.player[Projectile.owner].Clamity().antarcticSlicersBolts;
        public override float FadeInDuration => 3f;
        public override float FadeOutDuration => 0f;
        public override float TotalDuration => 4f;
        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(15);
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.scale = 1f;
            Projectile.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();
            Projectile.ownerHitCheck = true;
            Projectile.timeLeft = 360;
            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void SetVisualOffsets()
        {
            const int HalfSpriteWidth = 32 / 2;
            const int HalfSpriteHeight = 32 / 2;

            int HalfProjWidth = Projectile.width / 2;
            int HalfProjHeight = Projectile.height / 2;

            DrawOriginOffsetX = 0;
            DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
            DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);
        }

        public override void ExtraBehavior()
        {
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12, 12), DustID.SnowBlock);
                dust.scale = Main.rand.NextFloat(0.15f, 0.6f);
                dust.noGravity = true;
                dust.velocity = -Projectile.velocity * 0.5f;
            }

            float armPointingDirection = (Owner.Calamity().mouseWorld - Owner.MountedCenter).ToRotation();
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armPointingDirection - MathHelper.PiOver2);
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, 0);
            Owner.heldProj = Projectile.whoAmI;

            Projectile.Center = Projectile.Center + new Vector2(0, -6f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Add to bolt counter, spawn 2 quiver bolts, and on hit visual
            if (Bolts < 15)
            {
                Bolts++;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.Center, Vector2.Zero, ModContent.ProjectileType<AntarcticSlicersBolt>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0, Bolts);
                if (Bolts < 15)
                {
                    Bolts++;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.Center, Vector2.Zero, ModContent.ProjectileType<AntarcticSlicersBolt>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0, Bolts);
                }
            }
            else if (Main.rand.NextBool(2))
            {
                Vector2 vel = (Projectile.Center - player.Center).SafeNormalize(Vector2.Zero).RotatedByRandom(0.4f);
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), player.Center + vel * 20, vel * 0.1f, ModContent.ProjectileType<AntarcticSlicersSlash>(), (int)(Projectile.damage * 1.1), Projectile.knockBack * 1.2f, player.whoAmI, 1);
            }
            float numberOfDusts = 5f;
            float rotFactor = 360f / numberOfDusts;
            for (int i = 0; i < numberOfDusts; i++)
            {
                float rot = MathHelper.ToRadians(i * rotFactor);
                Vector2 offset = new Vector2(Main.rand.NextFloat(0.5f, 2.5f), 0).RotatedBy(rot * Main.rand.NextFloat(1.1f, 9.1f));
                Vector2 velOffset = new Vector2(Main.rand.NextFloat(0.5f, 2.5f), 0).RotatedBy(rot * Main.rand.NextFloat(1.1f, 9.1f));
                Dust dust = Dust.NewDustPerfect(target.Center + offset, Main.rand.NextBool() ? DustID.SnowBlock : DustID.Ice, new Vector2(velOffset.X, velOffset.Y));
                dust.noGravity = false;
                dust.velocity = velOffset;
                dust.scale = Main.rand.NextFloat(1.5f, 1.2f);
            }
        }
    }
}
