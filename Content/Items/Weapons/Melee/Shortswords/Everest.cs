using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Clamity.Content.Biomes.FrozenHell.Items;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Melee.Shortswords
{
    public class Everest : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.width = Item.height = 64;
            Item.rare = ModContent.RarityType<BurnishedAuric>();
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;

            Item.useAnimation = Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Rapier;
            Item.UseSound = new SoundStyle?(SoundID.Item1);
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;

            Item.damage = 750;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 8.5f;

            Item.shoot = ModContent.ProjectileType<EverestProjectile>();
            Item.shootSpeed = 2.4f;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                //.AddIngredient<Tomutus>()
                .AddIngredient<HellfireFlamberge>()
                .AddIngredient<EnchantedMetal>(8)
                .AddTile<CosmicAnvil>()
                .Register();
        }
    }
    public class EverestProjectile : BaseShortswordProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => ModContent.GetInstance<Everest>().Texture;
        public override void SetDefaults()
        {
            //AquaticDischargeProj
            /*Projectile.width = Projectile.height = 64;
            Projectile.timeLeft = 1000;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
            Projectile.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();*/

            Projectile.width = Projectile.height = 60;
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
        public override void OnSpawn(IEntitySource source)
        {
            for (int i = 0; i < 3; i++)
            {
                float speedX = Projectile.velocity.X * 10 + (float)Main.rand.Next(-40, 41) * 0.05f;
                float speedY = Projectile.velocity.Y * 10 + (float)Main.rand.Next(-40, 41) * 0.05f;
                float num = 0.5f;
                int type = 0;
                switch (i)
                {
                    case 0:
                    case 1:
                        type = ModContent.ProjectileType<VolcanicFireball>();
                        break;
                    case 2:
                        type = ModContent.ProjectileType<VolcanicFireballLarge>();
                        num = 0.75f;
                        break;
                }

                Projectile.NewProjectile(source, Projectile.Center.X, Projectile.Center.Y, speedX, speedY, type, (int)((float)Projectile.damage * num), Projectile.knockBack, Projectile.owner);
            }
        }
    }
}
