using CalamityMod.Items;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Magic
{
    public class MoonstoneKnives : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 20;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;

            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item39;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;

            Item.knockBack = 2f;
            Item.damage = 8;
            Item.DamageType = DamageClass.Magic;

            Item.shoot = ModContent.ProjectileType<MoonstoneKnifeProjectile>();
            Item.shootSpeed = 13f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float speed = Item.shootSpeed;
            Vector2 playerPos = player.RotatedRelativePoint(player.MountedCenter, true);
            float xDist = Main.mouseX + Main.screenPosition.X - playerPos.X;
            float yDist = Main.mouseY + Main.screenPosition.Y - playerPos.Y;
            if (player.gravDir == -1f)
            {
                yDist = Main.screenPosition.Y + Main.screenHeight - Main.mouseY - playerPos.Y;
            }
            Vector2 vector = new Vector2(xDist, yDist);
            float speedMult = vector.Length();
            if ((float.IsNaN(xDist) && float.IsNaN(yDist)) || (xDist == 0f && yDist == 0f))
            {
                xDist = player.direction;
                yDist = 0f;
                speedMult = speed;
            }
            else
            {
                speedMult = speed / speedMult;
            }
            xDist *= speedMult;
            yDist *= speedMult;
            int knifeAmt = 3;
            if (Main.rand.NextBool())
            {
                knifeAmt++;
            }
            if (Main.rand.NextBool(4))
            {
                knifeAmt++;
            }
            if (Main.rand.NextBool(8))
            {
                knifeAmt++;
            }
            if (Main.rand.NextBool(16))
            {
                knifeAmt++;
            }
            for (int i = 0; i < knifeAmt; i++)
            {
                float xVec = xDist;
                float yVec = yDist;
                float spreadMult = 0.05f * i;
                xVec += Main.rand.NextFloat(-25f, 25f) * spreadMult;
                yVec += Main.rand.NextFloat(-25f, 25f) * spreadMult;
                Vector2 directionToShoot = new Vector2(xVec, yVec);
                speedMult = directionToShoot.Length();
                speedMult = speed / speedMult;
                xVec *= speedMult;
                yVec *= speedMult;
                directionToShoot = new Vector2(xVec, yVec);
                Projectile.NewProjectile(source, playerPos, directionToShoot, type, damage, knockback, player.whoAmI, 0f, 0f);
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.ThrowingKnife, 200).
                AddIngredient(ItemID.ManaCrystal, 2).
                AddIngredient(ItemID.LesserManaPotion, 3).
                AddTile(TileID.Anvils).
                Register();
        }

    }
    public class MoonstoneKnifeProjectile : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = ProjAIStyleID.ThrownProjectile;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] >= 30f)
            {
                Projectile.alpha += 10;
                if (Projectile.damage > 1)
                    Projectile.damage = (int)(Projectile.damage * 0.9);
                Projectile.knockBack = Projectile.knockBack * 0.9f;
                if (Projectile.alpha >= 255)
                    Projectile.Kill();
            }
            if (Projectile.ai[0] < 30f)
                Projectile.rotation = MathF.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver2;
        }

        public override void OnKill(int timeLeft)
        {
            for (int dustIndex = 0; dustIndex < 3; ++dustIndex)
            {
                int redDust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.DungeonSpirit, 0f, 0f, 100, new Color(), 0.8f);
                Dust dust = Main.dust[redDust];
                dust.noGravity = true;
                dust.velocity *= 1.2f;
                dust.velocity -= Projectile.oldVelocity * 0.3f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.myPlayer != Projectile.owner)
                return;

            if (Main.rand.NextBool(3))
            {
                Main.player[Projectile.owner].statMana += 3;
                Main.player[Projectile.owner].ManaEffect(3);
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (Main.myPlayer != Projectile.owner)
                return;

            if (Main.rand.NextBool(3))
            {
                Main.player[Projectile.owner].statMana += 3;
                Main.player[Projectile.owner].ManaEffect(3);
            }
        }
    }
}
