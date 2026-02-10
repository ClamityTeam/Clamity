using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.NPCs.Providence;
using CalamityMod.Projectiles.BaseProjectiles;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Melee.Shortswords
{
    public class WulfrumLeechDagger : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.width = Item.height = 34;
            Item.rare = ItemRarityID.Blue;
            Item.value = 0; //this item having value is extremely exploitable, so set it to have no value
            Item.maxStack = 9999;

            Item.useAnimation = Item.useTime = 70;
            Item.useStyle = ItemUseStyleID.Rapier;
            Item.UseSound = new SoundStyle?(SoundID.Item1);
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.consumable = true;

            Item.damage = 1;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.knockBack = 5.75f;
            Item.crit = 0;

            Item.shoot = ModContent.ProjectileType<WulfrumLeechDaggerProjectile>();
            Item.shootSpeed = 2.4f;
        }
        public override void ModifyWeaponCrit(Player player, ref float crit)
        {
            crit = 1f;
        }
        public override void UpdateInventory(Player player)
        {
            Item.crit = 0;
            if (player.Clamity().wulfrumShortstrike)
                Item.consumable = false;
            else
                Item.consumable = true;
        }
        public override void AddRecipes()
        {
            CreateRecipe(50)
                .AddIngredient<WulfrumMetalScrap>()
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    public class WulfrumLeechDaggerProjectile : BaseShortswordProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => ModContent.GetInstance<WulfrumLeechDagger>().Texture;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 34;
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
            Player player = Main.player[Projectile.owner];
            /*if (!player.HasCooldown(ShortstrikeCooldown.ID))
            {
                player.AddCooldown(ShortstrikeCooldown.ID, 180);
                player.AddBuff(ModContent.BuffType<WulfrumShortstrike>(), 60);
            }*/
            if (target.type != NPCID.TargetDummy && target.type != ModContent.NPCType<Providence>())
                target.life -= target.lifeMax / 200;
            if (!player.Clamity().wulfrumShortstrike)
            {
                Projectile.NewProjectile(Projectile.GetSource_OnHit(target), Projectile.Center, Projectile.velocity / 2f, ModContent.ProjectileType<WulfrumLeechDaggerShard>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                Projectile.active = false;
            }
        }
    }
    public class WulfrumLeechDaggerShard : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.SpikyBall);
            Projectile.width = 12;
            Projectile.height = 14;
            Projectile.timeLeft = 300;
        }
    }
}
