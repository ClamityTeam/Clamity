using CalamityMod.Particles;
using Clamity.Commons;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Summon.Whips
{
    public class BalancedDuality : BaseWhipItem
    {
        public const int UseTime = 30;
        public int combo = 1;
        public bool flipped = false;
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<BalancedYangProj>(), 45, 2, 10, UseTime);
            Item.autoReuse = true;
            Item.value = Item.sellPrice(gold: 12);
            Item.rare = ItemRarityID.LightRed;
        }

        public override bool MeleePrefix()
        {
            return true;
        }

        public override void UpdateInventory(Player player)
        {
            if (combo == 1)
            {
                Item.useTime = Item.useAnimation = UseTime;
            }
            if (combo == 3)
            {
                Item.useTime = Item.useAnimation = UseTime / 2;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.LightShard, 1)
                .AddIngredient(ItemID.DarkShard, 1)
                .AddIngredient(ItemID.SoulofLight, 4)
                .AddIngredient(ItemID.SoulofNight, 4)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (flipped)
                type = ModContent.ProjectileType<BalancedYinProj>();
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            flipped = flipped ? false : true;
            combo++;
            if (combo > 5)
                combo = 1;

            return false;
        }
    }

    public class BalancedYangProj : BaseWhipProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetWhipStats()
        {
            Projectile.localNPCHitCooldown = -1;
            Projectile.WhipSettings.Segments = 15;
            Projectile.WhipSettings.RangeMultiplier = .6f;
            fishingLineColor = Color.Black;
            segmentRotation = -MathF.PI / 2;

            flipped = true;

            dustAmount = 4;
            swingDust = DustID.GolfPaticle;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Confused, 240);
            WhipOnHit(target);

            for (int i = 0; i < 10; i++)
            {
                LineParticle line = new LineParticle(target.Center, (target.Center - Main.player[Projectile.owner].Center).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2 * Projectile.spriteDirection + Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.Next(5, 10), false, Main.rand.Next(23, 35), Main.rand.NextFloat(1f, 1.8f), Color.White);
                GeneralParticleHandler.SpawnParticle(line);
            }
        }
    }

    public class BalancedYinProj : BaseWhipProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetWhipStats()
        {
            Projectile.localNPCHitCooldown = -1;
            Projectile.WhipSettings.Segments = 15;
            Projectile.WhipSettings.RangeMultiplier = .6f;
            fishingLineColor = Color.White;
            segmentRotation = -MathF.PI / 2;

            dustAmount = 4;
            swingDust = DustID.SpookyWood;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Confused, 240);
            WhipOnHit(target);

            for (int i = 0; i < 10; i++)
            {
                LineParticle line = new LineParticle(target.Center, (target.Center - Main.player[Projectile.owner].Center).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2 * Projectile.spriteDirection + Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.Next(5, 10), false, Main.rand.Next(23, 35), Main.rand.NextFloat(1f, 1.8f), Color.White);
                GeneralParticleHandler.SpawnParticle(line);
            }

            if (Main.rand.NextBool(4))
            {
            }
        }
    }

    public class BalancedDualityDebuff : ModBuff
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
            npc.GetGlobalNPC<GlobalTagDebuffs>().baldualIndex = buffIndex;
        }
    }
}