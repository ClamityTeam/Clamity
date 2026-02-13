using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Clamity.Content.Biomes.FrozenHell.Items;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Melee
{
    public class Breakthrough : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 88; Item.height = 86;
            Item.rare = ModContent.RarityType<BurnishedAuric>();
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;

            Item.useAnimation = Item.useTime = 16;
            Item.useStyle = ItemUseStyleID.Rapier;
            Item.UseSound = new SoundStyle?(SoundID.Item1);
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;

            Item.damage = 400;
            Item.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();
            Item.knockBack = 4f;

            Item.shoot = ModContent.ProjectileType<BreakthroughProj>();
            Item.shootSpeed = 12f;
        }
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override void UpdateInventory(Player player)
        {
            Item.channel = player.altFunctionUse == 2;
        }
        public override bool? UseItem(Player player)
        {
            Item.channel = player.altFunctionUse == 2;
            return true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0, player.altFunctionUse == 2 ? 1 : 0);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<EnchantedMetal>(20)
                .AddIngredient(ItemID.BorealWood)
                .AddIngredient(ItemID.SpectrePaintScraper)
                .AddTile<CosmicAnvil>()
                .Register();
        }
    }
    public class BreakthroughProj : BaseSpearProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 40;
            Projectile.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();
            Projectile.timeLeft = 32;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
        }
        public override void AI()
        {
            if (Projectile.ai[1] == 0)
            {
                base.AI();
                return;
            }
            //Main.NewText("Trigger");
            Player player = Main.player[base.Projectile.owner];
            player.ChangeDir(base.Projectile.direction);
            player.heldProj = base.Projectile.whoAmI;
            if (player.channel && !player.dead)
            {
                Projectile.timeLeft = 10;
                Projectile.friendly = false;
            }
            else
            {
                //Main.NewText("Trigger");
                Projectile.Kill();
            }
            player.itemTime = player.itemAnimation;
            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter);
            Projectile.position += Projectile.velocity * 10;
            Projectile.velocity = CalamityUtils.RotateTowards(Projectile.velocity, Projectile.AngleTo(Main.MouseWorld), 0.1f);
            if (++Projectile.ai[2] > 5)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, ModContent.ProjectileType<BreakthroughProjBreath>(), Projectile.damage, Projectile.knockBack, Main.myPlayer);
                Projectile.ai[2] = 0;
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.HasBuff(BuffID.Frozen))
            {
                modifiers.FinalDamage *= 1.1f;
            }
        }
    }
    public class BreakthroughProjBreath : ModProjectile, ILocalizedModType, IModType
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public new string LocalizationCategory => "Projectiles.Melee";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 40;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 300;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        public override void AI()
        {
            if (Main.rand.NextBool(3))
            {
                MediumMistParticle mist = new MediumMistParticle(Projectile.Center, Vector2.Zero, Color.Aquamarine, Color.White, 0.5f, 90, Main.rand.NextFloat(-0.1f, 0.1f));
                GeneralParticleHandler.SpawnParticle(mist);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frozen, 300);
        }
    }
}
