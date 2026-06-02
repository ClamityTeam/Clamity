using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Weapons.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Bosses.Pyrogen.Drop.Weapons
{
    public class MoltenPiercer : RogueWeapon
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            if (!ModLoader.TryGetMod("Redemption", out var redemption))
                return;
            redemption.Call("addElementItem", 2, Type);
        }
        public override void SetDefaults()
        {
            Item.width = Item.height = 16;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;

            Item.useTime = Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Swing;
            //Item.UseSound = SoundID.Item11;
            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.shoot = ModContent.ProjectileType<MoltenPiercerProjectile>();
            Item.shootSpeed = 12f;

            Item.damage = 144;
            Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Item.knockBack = 5f;
        }
        public override float StealthVelocityMultiplier => 1.5f;
        public override float StealthDamageMultiplier => 2f;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int index2 = Projectile.NewProjectile(source, position, velocity + Main.rand.NextVector2Square(-1, 1), type, damage / 2, knockback, player.whoAmI);
            if (player.Calamity().StealthStrikeAvailable())
                Main.projectile[index2].Calamity().stealthStrike = true;

            return false;
        }
    }
    public class MoltenPiercerProjectile : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override void SetStaticDefaults()
        {
            if (!ModLoader.TryGetMod("Redemption", out var redemption))
                return;
            redemption.Call("addElementProj", 2, Type);
        }
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 40;
            Projectile.aiStyle = -1;
            Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Projectile.penetrate = 5;
            Projectile.timeLeft = 600;
            Projectile.friendly = true;
            AIType = -1;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity.Y += 0.1f;
            if (Projectile.Calamity().stealthStrike)
            {
                if (Projectile.ai[0] > 0)
                    Projectile.ai[0]--;
                else
                {
                    Projectile.ai[0] = 3;
                    int index = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2CircularEdge(20, 20), ModContent.ProjectileType<ObsidigunBulletShard>(), Projectile.damage / 2, Projectile.knockBack / 5, Projectile.owner);
                    Main.projectile[index].DamageType = ModContent.GetInstance<RogueDamageClass>();
                }
            }
            //if (Projectile.timeLeft < 540) 
            //    Projectile.rotation
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //CursedDaggerProj
            target.AddBuff(BuffID.OnFire, 120);
        }
    }
}
