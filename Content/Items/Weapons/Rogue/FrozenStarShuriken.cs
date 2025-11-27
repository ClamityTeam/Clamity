using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Clamity.Content.Biomes.FrozenHell.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Rogue
{
    public class FrozenStarShuriken : RogueWeapon
    {
        public override float StealthDamageMultiplier => 0.5f;
        public override void SetDefaults()
        {
            Item.damage = 900;
            Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.width = 1;
            Item.height = 1;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<Violet>();
            Item.UseSound = SoundID.Item1;
            Item.shootSpeed = 13f;
            Item.shoot = ModContent.ProjectileType<FrozenStarShurikenProjectile>();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int index = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (player.Calamity().StealthStrikeAvailable())
                Main.projectile[index].Calamity().stealthStrike = true;
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<BlazingStar>()
                .AddIngredient<StarfishFromTheDepth>()
                .AddIngredient(ItemID.Trimarang)
                .AddIngredient<EnchantedMetal>(8)
                .AddTile<CosmicAnvil>()
                .Register();
        }
    }
    public class FrozenStarShurikenProjectile : ModProjectile, ILocalizedModType, IModType
    {
        public override string Texture => ModContent.GetInstance<FrozenStarShuriken>().Texture;
        public new string LocalizationCategory => "Projectiles.Rogue";

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = BlazingStarProj.Lifetime;
            Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            Projectile.rotation += 0.4f * (float)Projectile.direction;
            if (Projectile.timeLeft < BlazingStarProj.Lifetime - BlazingStarProj.ReboundTime)
                Projectile.ai[0] = 1f;
            if (Projectile.ai[0] == 0.0)
                return;
            Projectile.tileCollide = false;
            float num1 = 32.5f;
            float num2 = 3f;
            Terraria.Player player = Main.player[Projectile.owner];
            float num3 = Projectile.Distance(player.Center);
            Vector2 vector2 = (player.Center - Projectile.Center) / num3 * num1;
            if (num3 > 3000)
                Projectile.Kill();
            if (Projectile.velocity.X < vector2.X)
            {
                Projectile.velocity.X += num2;
                if (Projectile.velocity.X < 0.0 && vector2.X > 0.0)
                    Projectile.velocity.X += num2;
            }
            else if (Projectile.velocity.X > vector2.X)
            {
                Projectile.velocity.X -= num2;
                if (Projectile.velocity.X > 0.0 && vector2.X < 0.0)
                    Projectile.velocity.X -= num2;
            }
            if (Projectile.velocity.Y < vector2.Y)
            {
                Projectile.velocity.Y += num2;
                if (Projectile.velocity.Y < 0.0 && vector2.Y > 0.0)
                    Projectile.velocity.Y += num2;
            }
            else if (Projectile.velocity.Y > vector2.Y)
            {
                Projectile.velocity.Y -= num2;
                if (Projectile.velocity.Y > 0.0 && vector2.Y < 0.0)
                    Projectile.velocity.Y -= num2;
            }
            if (Main.myPlayer != Projectile.owner)
                return;
            Rectangle hitbox = Projectile.Hitbox;
            if (!hitbox.Intersects(player.Hitbox))
                return;
            Projectile.Kill();
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.ai[0] = 1f;
            Projectile.tileCollide = false;
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < Main.rand.Next(3 + (Projectile.Calamity().stealthStrike ? 1 : 0)); i++)
            {
                int index = Projectile.NewProjectile(Projectile.GetSource_OnHit(target), Projectile.Center, new Vector2(0, 15).RotatedByRandom(MathHelper.TwoPi), ProjectileID.LostSoulFriendly, Projectile.damage / 3, 0, Projectile.owner);
                Main.projectile[index].DamageType = ModContent.GetInstance<RogueDamageClass>();
                Main.projectile[index].usesLocalNPCImmunity = false;
                Main.projectile[index].usesIDStaticNPCImmunity = true;
                Main.projectile[index].idStaticNPCHitCooldown = 10;
            }
        }
    }
}
