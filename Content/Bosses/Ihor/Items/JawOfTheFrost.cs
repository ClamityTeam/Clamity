using CalamityMod.Items;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Ihor.Items
{
    public class JawOfTheFrost : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetStaticDefaults()
        {
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

            ItemID.Sets.BonusAttackSpeedMultiplier[Type] = 0.25f;
        }

        public override void SetDefaults()
        {
            Item.width = 66;
            Item.height = 64;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;

            Item.useTime = Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            Item.damage = 120;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 6;

            Item.shootsEveryUse = true;
            Item.shoot = ModContent.ProjectileType<JawOfTheFrostProjectileBig>();
            Item.shootSpeed = 15f;
        }
        int Timer = 0;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.MountedCenter, new Vector2(player.direction, 0f), ModContent.ProjectileType<JawOfTheFrostMeleeEffect>(), damage, knockback, player.whoAmI, (float)player.direction * player.gravDir, player.itemAnimationMax, player.GetAdjustedItemScale(Item));
            return false;
        }
        public override void HoldItem(Player player) //fancy momentum swing, this should be generalized and applied to other swords imo
        {
            if (player.itemAnimation == 0)
            {
                Timer = 0;
                return;
            }

            if (player.itemAnimation == player.itemAnimationMax)
            {
                Timer = player.itemAnimationMax;
            }
            if (player.itemAnimation > 0)
            {
                Timer--;
            }

            if (Timer == player.itemAnimationMax / 2)
            {
                SoundEngine.PlaySound(SoundID.Item1, player.Center);
                SoundEngine.PlaySound(SoundID.Item39, player.Center);
                if (player.whoAmI == Main.myPlayer)
                {
                    Vector2 vel = (Item.shootSpeed + Main.rand.Next(-1, 1)) * Vector2.Normalize(Main.MouseWorld - player.itemLocation);
                    int p = Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.itemLocation, vel, Item.shoot, (int)(player.GetTotalDamage(DamageClass.Melee).Additive * player.GetTotalDamage(DamageClass.Melee).Multiplicative * Item.damage), Item.knockBack, player.whoAmI);
                }
            }
            if (Timer > 2 * player.itemAnimationMax / 3)
            {
                player.itemAnimation = player.itemAnimationMax;
                Item.noMelee = true;
            }
            else
            {
                Item.noMelee = false;
                float prog = (float)Timer / (2 * player.itemAnimationMax / 3);
                player.itemAnimation = (int)(player.itemAnimationMax * Math.Pow(MomentumProgress(prog), 2));
            }

            /*if (Timer > 2 * player.itemAnimationMax / 3)
            {
                player.itemAnimation = player.itemAnimationMax;
                Item.noMelee = true;
            }
            else
            {
                Item.noMelee = false;
                float prog = (float)Timer / (2 * player.itemAnimationMax / 3);
                player.itemAnimation = (int)(player.itemAnimationMax * Math.Pow(MomentumProgress(prog), 2));
            }*/
        }
        //this is ripped from my own game project
        ///<summary>
        ///Returns distance progress by a sine formula based on linear progress = (% between 1-0). f(1) = 1, f(0) = 0.
        ///</summary>
        public static float MomentumProgress(float x)
        {
            return (x * x * 3) - (x * x * x * 2);
        }
    }
    public class JawOfTheFrostProjectileBig : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.DeerclopsRangedProjectile);
            Projectile.width = Projectile.height = 50;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 30;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 600;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 7; i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, new Vector2(0, -5) + Main.rand.NextVector2Circular(1, 1), ModContent.ProjectileType<JawOfTheFrostProjectile>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
            }
            for (int i = 0; i < 20; i++)
            {
                //Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2, Projectile.height / 2), Main.rand.NextBool() ? DustID.SnowBlock : DustID.Ice, Projectile.velocity.RotatedBy(1.5f) / 3f, Scale: Main.rand.NextFloat(2f));
                Dust dust2 = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2, Projectile.height / 2), Main.rand.NextBool() ? DustID.SnowBlock : DustID.Ice, Main.rand.NextVector2Circular(3f, 3f), Scale: Main.rand.NextFloat(2f));

            }
        }
    }
    public class JawOfTheFrostProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.DeerclopsRangedProjectile);
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 20;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 600;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return Projectile.velocity.Y > 0;
        }
    }
}
