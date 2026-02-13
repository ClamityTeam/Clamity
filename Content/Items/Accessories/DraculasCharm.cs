using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Accessories
{
    public class DraculasCharm : ModItem, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 46;
            Item.accessory = true;
            Item.value = 20000 * 5;
            Item.rare = ModContent.RarityType<Turquoise>();
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage<ThrowingDamageClass>() += 0.15f;
            var modPlayer = player.Calamity();

            modPlayer.rogueStealthMax += 0.1f;
            player.Clamity().vampireEX = true;

            modPlayer.rottenDogTooth = true;
            modPlayer.vampiricTalisman = true;
            modPlayer.raiderTalisman = true;
            modPlayer.etherealExtorter = true;

            //get fixed boi funny
            if (Main.zenithWorld)
                player.lifeRegen -= 10; //Never ending thirst of calamity devs
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<EtherealExtorter>()
                .AddIngredient<VampiricTalisman>()
                .AddIngredient<BloodstoneCore>(5)
                .AddIngredient<AshesofCalamity>(4)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    public class DraculasCharmProj : ModProjectile, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Moonflare Bat");
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 52;
            Projectile.height = 48;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Projectile.timeLeft = 600;
        }
        public override void AI()
        {
            if (Projectile.ai[0] == -1)
            {
                NPC nPC = Projectile.Center.ClosestNPCAt(1600f);
                if (nPC != null)
                {
                    Projectile.ai[0] = nPC.whoAmI;
                }
            }
            int npc = (int)Projectile.ai[0];
            NPC target = Main.npc[(int)Projectile.ai[0]];
            if (npc < 0 || npc >= 200 || !Main.npc[npc].active)
                Projectile.Kill();
            Projectile.spriteDirection = target.Center.X > Projectile.Center.X ? 1 : -1;

            if (++Projectile.frameCounter >= 3)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= 4)
                    Projectile.frame = 0;
            }

            Projectile.rotation = Projectile.velocity.X * 0.05f;

            if (Projectile.timeLeft > 550)
                Projectile.velocity *= 0.98f;
            else
            {
                Projectile.friendly = true;
                Projectile.Move(target.Center, Projectile.timeLeft > 50 ? 30 : 50, 20);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            int random = Main.rand.Next(1, 3);
            player.statLife += random;
            player.HealEffect(random);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D texture2 = ModContent.Request<Texture2D>(Texture + "_Trail").Value;
            var effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            int height = texture.Height / 4;
            int y = height * Projectile.frame;
            Rectangle rect = new(0, y, texture.Width, height);
            Vector2 drawOrigin = new(texture.Width / 2, Projectile.height / 2);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Color.White * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture2, drawPos, new Rectangle?(rect), color, Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle?(rect), Color.White, Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);

            return false;
        }
    }
}
