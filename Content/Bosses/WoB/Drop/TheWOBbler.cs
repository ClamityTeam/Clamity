using CalamityMod.Items;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.WoB.Drop
{
    public class TheWOBbler : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Tools";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 40;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<BurnishedAuric>();

            Item.useTime = 1;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;

            Item.damage = 550;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 20;

            //Item.shoot = ModContent.ProjectileType<TheWOBblerProjectile>();
            Item.shootSpeed = 20;

            Item.axe = 300 / 5;
            Item.hammer = 250;
            Item.tileBoost = 5;
        }
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.axe = 0;
                Item.hammer = 0;
                Item.useTime = 8;
                Item.shoot = ModContent.ProjectileType<TheWOBblerProjectile>();
                return true;
            }
            else
            {
                Item.axe = 300 / 5;
                Item.hammer = 250;
                Item.useTime = 1;
                Item.shoot = ProjectileID.None;
                return true;
            }
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    Projectile.NewProjectile(source, position, velocity + Main.rand.NextVector2Circular(2, 2) + new Vector2(0, 1), type, damage / 3, knockback, player.whoAmI);
                }
            }
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }
    }
    public class TheWOBblerProjectile : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.SpikyBall);
            Projectile.width = Projectile.height = 22;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 300;
            Projectile.usesLocalNPCImmunity = false;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 3;
            Projectile.DamageType = DamageClass.Melee;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity.X = 0;
            Projectile.rotation = 0;
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frozen, 5 * 60);
        }
    }
}
