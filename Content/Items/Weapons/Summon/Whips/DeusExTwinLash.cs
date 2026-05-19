using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Materials;
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
    public class DeusExTwinLash : BaseWhipItem
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
            Item.DefaultToWhip(ModContent.ProjectileType<DeusExTwinProj>(), 150, 4, 12);
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
            Item.useTime = Item.useAnimation = (int)(UseTime / MathHelper.Lerp(1, 2, combo / 4f));
            //Item.useTime = Item.useAnimation = UseTime / combo;
            if (combo > 4) combo = 1;
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (flipped)
                type = ModContent.ProjectileType<DeusExProj>();
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            flipped = !flipped;
            combo++;
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<BalancedDuality>())
                .AddIngredient(ModContent.ItemType<AstralBar>(), 8)
                //.AddIngredient(ModContent.ItemType<StarblightSoot>(), 25)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class DeusExTwinProj : BaseWhipProjectile
    {
        public override string ExtraPath => "DeusExTwinLash";
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
            trailLineColorOverride = Color.Coral;
            segmentRotation = -MathF.PI / 2;

            flipped = true;

            tagDebuff = ModContent.BuffType<detlDebuff>();
            dustAmount = 4;
            swingDust = DustID.OrangeTorch;
        }

        public override void WhipOnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 240);

            DirectionalPulseRing pulse = new DirectionalPulseRing(target.Center, Vector2.Zero, Main.rand.NextBool() ? Color.DarkTurquoise : Color.Coral, new Vector2(1, 1), 0, 0.5f, 0f, 20);
            GeneralParticleHandler.SpawnParticle(pulse);
        }
        public override bool DrawTrailAtTip => true;
    }

    public class DeusExProj : BaseWhipProjectile
    {
        public override string ExtraPath => "DeusExTwinLash";
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
            trailLineColorOverride = Color.DarkTurquoise;
            segmentRotation = -MathF.PI / 2;

            tagDebuff = ModContent.BuffType<detlDebuff>();
            dustAmount = 4;
            swingDust = DustID.BlueFlare;
        }

        public override void WhipOnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 480);

            DirectionalPulseRing pulse = new DirectionalPulseRing(target.Center, Vector2.Zero, Main.rand.NextBool() ? Color.DarkTurquoise : Color.Coral, new Vector2(1, 1), 0, 0.5f, 0f, 20);
            GeneralParticleHandler.SpawnParticle(pulse);
        }
        public override bool DrawTrailAtTip => true;
    }

    public class detlDebuff : ModBuff, ILocalizedModType
    {
        public new string LocalizationCategory => "Buffs.Whips";
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
            npc.GetGlobalNPC<GlobalTagDebuffs>().detlIndex = buffIndex;
        }
    }
}