using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Rogue;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Melee.Shortswords
{
    public class Disease : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetStaticDefaults()
        {
            if (!ModLoader.TryGetMod("Redemption", out var redemption))
                return;
            redemption.Call("addElementItem", 11, Type);
        }
        public override void SetDefaults()
        {
            Item.width = 30; Item.height = 38;
            Item.rare = ItemRarityID.Pink;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;

            Item.useAnimation = Item.useTime = 12;
            Item.useStyle = ItemUseStyleID.Rapier;
            Item.UseSound = new SoundStyle?(SoundID.Item1);
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;

            Item.damage = 194;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 5.5f;

            Item.shoot = ModContent.ProjectileType<DiseaseProjectile>();
            Item.shootSpeed = 2.4f;
        }
    }
    public class DiseaseProjectile : BaseShortswordProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => ModContent.GetInstance<Disease>().Texture;
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            if (!ModLoader.TryGetMod("Redemption", out var redemption))
                return;
            redemption.Call("addElementProj", 11, Type);
        }
        public override void SetDefaults()
        {
            Projectile.width = 30; Projectile.height = 38;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.scale = 1f;
            Projectile.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();
            Projectile.ownerHitCheck = true;
            Projectile.timeLeft = 360;
            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            /*if (!Main.player[Projectile.owner].HasCooldown(ShortstrikeCooldown.ID))
            {
                Main.player[Projectile.owner].AddCooldown(ShortstrikeCooldown.ID, 30);
                //for (int i = 0; i < 3; i++)
                //    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.UnitY.RotatedByRandom(MathHelper.PiOver4 / 2) / 10, ModContent.ProjectileType<CaliburnSlash>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner);
                //PlaguenadeProj;
                for (int i = 0; i < 5; i++)
                {
                    float speedX = Main.rand.Next(-35, 36) * 0.02f;
                    float speedY = Main.rand.Next(-35, 36) * 0.02f;
                    int num = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X, Projectile.position.Y, speedX, speedY, ModContent.ProjectileType<PlaguenadeBee>(), Main.player[Projectile.owner].beeDamage(base.Projectile.damage), Main.player[Projectile.owner].beeKB(0f), Projectile.owner);
                    Main.projectile[num].DamageType = DamageClass.Melee;
                }
            }*/
            float speedX = Main.rand.Next(-35, 36) * 0.02f;
            float speedY = Main.rand.Next(-35, 36) * 0.02f;
            int num = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X, Projectile.position.Y, speedX, speedY, ModContent.ProjectileType<PlaguenadeBee>(), Main.player[Projectile.owner].beeDamage(base.Projectile.damage), Main.player[Projectile.owner].beeKB(0f), Projectile.owner);
            Main.projectile[num].DamageType = DamageClass.Melee;

        }
    }
}
