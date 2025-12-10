using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Clamity.Content.Items.Weapons.Rogue
{
    public class AuricKunai : RogueWeapon
    {
        private int counter = 0;

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 48;
            Item.rare = ModContent.RarityType<Violet>();
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;

            Item.useTime = 1;
            Item.useAnimation = 10;
            Item.reuseDelay = 1;
            Item.useLimitPerAnimation = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.UseSound = SoundID.Item109;
            Item.autoReuse = true;

            Item.damage = 140;
            Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Item.knockBack = 5f;

            Item.shoot = ModContent.ProjectileType<AuricKunaiProjectile>();
            Item.shootSpeed = 38f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int stealth = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (player.Calamity().StealthStrikeAvailable() && player.ownedProjectileCounts[ModContent.ProjectileType<AuricKunaiLightning>()] < 10 && counter == 0 && stealth.WithinBounds(Main.maxProjectiles))
            {
                Main.projectile[stealth].Calamity().stealthStrike = true;
                SoundEngine.PlaySound(SoundID.Item73, player.Center);
                for (float i = 0; i < 5; i++)
                {
                    float angle = MathHelper.TwoPi / 5f * i;
                    Projectile.NewProjectile(source, player.Center, angle.ToRotationVector2() * 8f, ModContent.ProjectileType<AuricKunaiLightning>(), damage, knockback, player.whoAmI, angle, 0f);
                }
            }

            counter++;
            if (counter >= Item.useAnimation / Item.useTime)
                counter = 0;
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<CosmicKunai>()
                .AddIngredient<ExecutionersBlade>()
                .AddIngredient<LunarKunai>(100)
                .AddIngredient<AuricBar>(5)
                .AddTile<CosmicAnvil>()
                .Register();
        }
    }
    public class AuricKunaiProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => ModContent.GetInstance<AuricKunai>().Texture;
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
        }

        public override void AI()
        {
            Projectile.rotation = (float)Math.Atan2((double)Projectile.velocity.Y, (double)Projectile.velocity.X) + 1.57f;
            Projectile.alpha += 17;
            if (Projectile.alpha >= 255)
            {
                Projectile.Kill();
            }
            Lighting.AddLight(Projectile.Center, (255 - Projectile.alpha) * 0.5f / 255f, (255 - Projectile.alpha) * 0f / 255f, (255 - Projectile.alpha) * 0.5f / 255f);
            CalamityUtils.HomeInOnNPC(Projectile, true, 300f, 12f, 20f);
        }
    }
    public class AuricKunaiLightning : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        private readonly HashSet<int> hitTargets = new HashSet<int>();
        private readonly HashSet<int> foundTargets = new HashSet<int>();

        private UnifiedRandom rng;

        public UnifiedRandom Rng
        {
            get
            {
                if (rng == null)
                {
                    rng = new UnifiedRandom(RandomSeed / (1 + Projectile.identity));
                }
                return rng;
            }
        }

        public int RandomSeed
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public bool HasSpawnEffects
        {
            get => Projectile.ai[1] == 1f;
            set => Projectile.ai[1] = value ? 1f : 0f;
        }

        public int WobbleTimer
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CanDistortWater[Projectile.type] = false;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 100;
            Projectile.ignoreWater = true;
            Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (hitTargets.Contains(target.whoAmI))
            {
                return false;
            }
            return base.CanHitNPC(target);
        }

        public override void AI()
        {
            bool killed = Projectile.HandleChaining(hitTargets, foundTargets, 8);
            if (killed)
            {
                return;
            }

            int index = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Vortex, 0f, 0f, 0, default(Color), 1.15f);
            Dust dust = Main.dust[index];
            dust.position.X = Projectile.Center.X;
            dust.position.Y = Projectile.Center.Y;
            dust.velocity *= 0f;
            dust.noGravity = true;

            if (HasSpawnEffects)
            {
                HasSpawnEffects = false;

                SoundEngine.PlaySound(SoundID.Item94, Projectile.Center);

                for (int k = 0; k < 20; k++)
                {
                    dust = Dust.NewDustDirect(Projectile.Center - new Vector2(4), 8, 8, DustID.Vortex, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 0, default, 1.25f);
                    dust.noGravity = true;
                }
            }

            if (Projectile.timeLeft < 580)
            {
                if (Projectile.timeLeft >= 600)
                {
                    for (int k = 0; k < 6; k++)
                    {
                        index = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Vortex, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, 125, default(Color), 1.15f);
                        Main.dust[index].noGravity = true;
                    }
                }

                WobbleTimer++;
                if (WobbleTimer > 2)
                {
                    Projectile.velocity.Y += Rng.NextFloat(-0.75f, 0.75f);
                    Projectile.velocity.X += Rng.NextFloat(-0.75f, 0.75f);
                    WobbleTimer = 0;
                }

                float x = Projectile.Center.X;
                float y = Projectile.Center.Y;
                float dist = 750;
                bool found = false;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy() && !hitTargets.Contains(npc.whoAmI) && Projectile.DistanceSQ(npc.Center) < dist * dist && Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1))
                    {
                        float foundX = npc.Center.X;
                        float foundY = npc.Center.Y;
                        float abs = Math.Abs(Projectile.Center.X - foundX) + Math.Abs(Projectile.Center.Y - foundY);
                        if (abs < dist)
                        {
                            dist = abs;
                            x = foundX;
                            y = foundY;
                            found = true;
                        }
                    }
                }

                if (found)
                {
                    float mag = 2.5f;
                    Vector2 center = Projectile.Center;
                    float toX = x - center.X;
                    float toY = y - center.Y;
                    float len = (float)Math.Sqrt((double)(toX * toX + toY * toY));
                    len = mag / len;
                    toX *= len;
                    toY *= len;

                    Projectile.velocity.X = (Projectile.velocity.X * 20f + toX) / 21f;
                    Projectile.velocity.Y = (Projectile.velocity.Y * 20f + toY) / 21f;
                }
                else
                {
                    Projectile.velocity = Vector2.Zero;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int k = 0; k < 10; k++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Vortex, Main.rand.Next((int)-5f, (int)5f), Main.rand.Next((int)-5f, (int)5f), 75, default(Color), 0.75f);
                Main.dust[dust].noGravity = true;
            }
        }
    }
}
