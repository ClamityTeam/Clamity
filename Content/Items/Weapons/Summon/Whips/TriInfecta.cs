using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Clamity.Commons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Summon.Whips
{
    public class TriInfecta : BaseWhipItem
    {
        private const int SmallUseTime = 40;
        private const int MediumUseTime = 30;
        private const int LargeUseTime = 20;

        private int combo = 1;

        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<TriInfectaSmallProj>(), 40, 4, 8, SmallUseTime);
            Item.autoReuse = true;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Orange;
        }

        public override bool MeleePrefix()
        {
            return true;
        }

        public override void UpdateInventory(Player player)
        {
            switch (combo)
            {
                case 1:
                    Item.useTime = Item.useAnimation = SmallUseTime;
                    Item.shoot = ModContent.ProjectileType<TriInfectaSmallProj>();
                    break;
                case 2:
                    Item.useTime = Item.useAnimation = MediumUseTime;
                    Item.shoot = ModContent.ProjectileType<TriInfectaMedProj>();
                    break;
                case 3:
                case 4:
                    Item.useTime = Item.useAnimation = LargeUseTime;
                    Item.shoot = ModContent.ProjectileType<TriInfectaLargeProj>();
                    break;
            }
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            combo++;
            if (combo > 4)
                combo = 1;

            return false;
        }

        /*public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CrimtaneBar, 5)
                .AddIngredient(ModContent.ItemType<BloodSample>(), 7)
                .AddIngredient(ItemID.Vertebrae, 6)
                .AddTile(TileID.DemonAltar)
                .Register();
        }*/
    }

    public class TriInfectaSmallProj : BaseWhipProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2; // cal trail mode
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetWhipStats()
        {
            Projectile.localNPCHitCooldown = -1;
            Projectile.WhipSettings.Segments = 10;
            Projectile.WhipSettings.RangeMultiplier = 0.4f;

            segmentRotation = -MathF.PI / 2;
            fishingLineColor = Color.Purple;
            dustAmount = 2;
            swingDust = DustID.Blood;
        }

        public override void WhipTipParticles(Vector2 tipCoord, Color lightingCol, int dustID, int dustNum)
        {
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPosition = tipCoord + Main.rand.NextVector2Circular(Projectile.width / 2f, Projectile.height / 2f);
                HeavySmokeParticle smoke = new HeavySmokeParticle(
                    trailPosition,
                    Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    Color.Lerp(Color.Red, Color.Crimson, Main.rand.NextFloat()),
                    Main.rand.Next(100, 200),
                    Main.rand.NextFloat(0.1f, 0.2f),
                    0.4f
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }
    }

    public class TriInfectaMedProj : BaseWhipProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetWhipStats()
        {
            whipSegment2 = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Segment2");
            Projectile.localNPCHitCooldown = -1;
            Projectile.WhipSettings.Segments = 15;
            Projectile.WhipSettings.RangeMultiplier = 0.5f;

            segmentRotation = -MathF.PI / 2;
            fishingLineColor = Color.DarkGreen;
            dustAmount = 4;
            swingDust = DustID.Blood;
        }

        public override void WhipTipParticles(Vector2 tipCoord, Color lightingCol, int dustID, int dustNum)
        {
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPosition = tipCoord + Main.rand.NextVector2Circular(Projectile.width / 2f, Projectile.height / 2f);
                HeavySmokeParticle smoke = new HeavySmokeParticle(
                    trailPosition,
                    Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    Color.Lerp(Color.Red, Color.Orange, Main.rand.NextFloat()),
                    Main.rand.Next(100, 200),
                    Main.rand.NextFloat(0.1f, 0.2f),
                    0.6f
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }
    }

    public class TriInfectaLargeProj : BaseWhipProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetWhipStats()
        {
            whipSegment2 = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Segment2");
            Projectile.localNPCHitCooldown = -1;
            Projectile.WhipSettings.Segments = 15;
            Projectile.WhipSettings.RangeMultiplier = 0.6f;

            segmentRotation = -MathF.PI / 2;
            fishingLineColor = Color.Red;
            tagDebuff = ModContent.BuffType<TriInfectaDebuff>();
            dustAmount = 6;
            swingDust = DustID.Blood;
        }

        public override void WhipOnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<BurningBlood>(), 300);

            if (Main.rand.NextFloat() <= 0.35f)
                for (int i = 0; i < 5; i++)
                {
                    HeavySmokeParticle smoke = new HeavySmokeParticle(
                        target.Center + Main.rand.NextVector2Circular(15, 15),
                        Main.rand.NextVector2Circular(1f, 1f),
                        Color.Crimson,
                        Main.rand.Next(150, 255), // opacity
                        Main.rand.NextFloat(0.2f, 0.3f), // size
                        0.6f
                    );
                    GeneralParticleHandler.SpawnParticle(smoke);
                }
        }

        public override void WhipTipParticles(Vector2 tipCoord, Color lightingCol, int dustID, int dustNum)
        {
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPosition = tipCoord + Main.rand.NextVector2Circular(Projectile.width / 2f, Projectile.height / 2f);
                HeavySmokeParticle smoke = new HeavySmokeParticle(
                    trailPosition,
                    Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    Color.Lerp(Color.Orange, Color.Red, Main.rand.NextFloat()),
                    Main.rand.Next(100, 200), // opacity
                    Main.rand.NextFloat(0.1f, 0.2f), // size
                    0.8f // length
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }
    }

    public class TriInfectaDebuff : ModBuff
    {
        public override string Texture => Clamity.buffPath;

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            BuffID.Sets.IsATagBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<GlobalTagDebuffs>().activeTag = Type;
            npc.GetGlobalNPC<GlobalTagDebuffs>().triIndex = buffIndex;
        }
    }
}