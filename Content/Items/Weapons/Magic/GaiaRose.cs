using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Magic
{
    public class GaiaRose : ManaRose
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.shoot = ModContent.ProjectileType<GaiaRoseBolt>();
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.JungleRose)
                .AddIngredient(ItemID.ManaCrystal)
                .AddIngredient(ItemID.Moonglow, 5)
                .AddTile(TileID.Anvils)
                .Register();
            Recipe.Create(ModContent.ItemType<GleamingMagnolia>())
                .AddIngredient(Type)
                .AddIngredient(ItemID.HallowedBar, 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class GaiaRoseBolt : ManaBolt
    {
        public override void AI()
        {
            Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.01f * Projectile.direction;
            Projectile.velocity *= 0.985f;
            int randomDust = Utils.SelectRandom(Main.rand, new int[]
            {
                15,
                107
            });
            Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, randomDust, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.MagicMirror, Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f);
                //Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.TerraBlade, Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f);
            }
            if (Projectile.owner == Main.myPlayer)
            {
                float rand = Main.rand.NextFloat(0, MathHelper.TwoPi);
                for (int i = 0; i < 6; i++)
                {
                    Vector2 velocity = ((MathHelper.TwoPi * i / 6f) - rand).ToRotationVector2() * 10f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<GaiaRoseBoltSmall>(), (int)(Projectile.damage * 0.5), Projectile.knockBack, Projectile.owner);
                }
            }
            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
        }
    }
    public class GaiaRoseBoltSmall : ManaBoltSmall2
    {
        public override string Texture => (GetType().Namespace + "." + Name).Replace('.', '/');
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.timeLeft *= 2;
            Projectile.penetrate = 4;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 30;
        }
        public override void AI()
        {
            base.AI();
            Projectile.velocity.X *= 0.95f;
            Projectile.velocity.Y *= 0.95f;
        }
    }
}
