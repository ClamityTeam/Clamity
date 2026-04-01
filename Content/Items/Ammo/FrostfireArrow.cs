using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Clamity.Content.Biomes.FrozenHell.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Ammo
{
    public class FrostfireArrow : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Ammo.Arrow";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 99;
        }
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.WoodenArrow);
            Item.rare = ModContent.RarityType<BurnishedAuric>();
            Item.value = Terraria.Item.sellPrice(copper: 30);

            Item.shoot = ModContent.ProjectileType<FrostfireArrowProj>();
            Item.shootSpeed = 10;

            Item.knockBack = 4;
            Item.damage = 20;
        }
        public override void AddRecipes()
        {
            CreateRecipe(999)
                //.AddIngredient(ItemID.HellfireArrow, 666)
                //.AddIngredient(ItemID.FrostburnArrow, 333)
                .AddIngredient(ItemID.HellfireArrow, 999)
                .AddIngredient<EnchantedMetal>()
                .AddTile<CosmicAnvil>()
                .Register();
        }
    }
    public class FrostfireArrowProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged.Ammo";
        public override string Texture => ModContent.GetInstance<FrostfireArrow>().Texture;
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            Projectile.extraUpdates = 1;
        }
        /*public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.NewProjectile(Projectile.GetSource_OnHit(target), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FrostfireArrowProj2>(), Projectile.damage / 4, 0, Projectile.owner);
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            Projectile.NewProjectile(Projectile.GetSource_OnHit(target), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FrostfireArrowProj2>(), Projectile.damage / 4, 0, Projectile.owner);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FrostfireArrowProj2>(), Projectile.damage / 4, 0, Projectile.owner);
            Projectile.Kill();
            return false;
        }*/
        public override void OnKill(int timeLeft)
        {
            Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FrostfireArrowProj2>(), Projectile.damage / 4, 0, Projectile.owner);
        }

        public override void PostAI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation), DustID.Flare_Blue, Vector2.UnitX.RotatedBy(Projectile.rotation + MathHelper.PiOver2));
            dust.noGravity = true;
        }
    }
    public class FrostfireArrowProj2 : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged.Ammo";
        //public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        //public override string Texture => "Content/Images/Projectile_961";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 7;
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 140;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.idStaticNPCHitCooldown = 10;
            Projectile.usesIDStaticNPCImmunity = true;
            //Projectile.scale = 0;
        }
        /*public override void PostDraw(Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("Content/Images/Projectile_961").Value;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 5 / 2);
            Main.spriteBatch.Draw(texture.)
        }*/
        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 3)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Type])
                    Projectile.Kill();
            }
            Projectile.scale += 0.05f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //SoundEngine.PlaySound(SoundID);
            target.AddBuff(BuffID.Frostburn, 120);
            Projectile.Clamity().extraAI[0]++;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Frostburn, 120);
            target.AddBuff(BuffID.Frozen, 30);
            Projectile.Clamity().extraAI[0]++;
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.Clamity().extraAI[0] > 5)
                return false;
            return base.CanHitNPC(target);
        }
    }
}
