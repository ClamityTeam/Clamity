using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Clamity.Content.Bosses.Pyrogen.NPCs;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod;

namespace Clamity.Content.Bosses.Pyrogen.Projectiles
{
    public class MoltenChain : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2000;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.scale = 1;
            Projectile.tileCollide = false;
            Projectile.light = 0.5f;
            Projectile.penetrate = -1;
            Projectile.Calamity().DealsDefenseDamage = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.ai[0] < 0)
            {
                Projectile.Kill();
                return false;
            }

            NPC owner = Main.npc[(int)Projectile.ai[0]];

            if (owner == null || !owner.active || owner.type != ModContent.NPCType<PyrogenBoss>())
            {
                Projectile.Kill();
                return false;
            }

            Asset<Texture2D> t = TextureAssets.Projectile[Type];
            Main.instance.LoadProjectile(ProjectileID.CultistBossIceMist);
            Asset<Texture2D> fire = TextureAssets.Projectile[ProjectileID.CultistBossIceMist];

            ref float timer = ref Projectile.ai[2];
            if (true)
            {
                for (int i = 0; i < Projectile.Distance(owner.Center); i += (int)(48 * Projectile.scale))
                {
                    Vector2 pos = Projectile.Center + new Vector2(-i, 0).RotatedBy(Projectile.AngleFrom(owner.Center));
                    for (int j = 0; j < 12; j++)
                    {
                        Vector2 afterimageOffset = (MathHelper.TwoPi * j / 12f).ToRotationVector2() * 1f;
                        Color glowColor = Color.DarkRed with { A = 0 } * 0.7f;


                        Main.EntitySpriteDraw(t.Value, pos + afterimageOffset - Main.screenPosition, null, glowColor * Projectile.Opacity, Projectile.rotation, t.Size() / 2, Projectile.scale, SpriteEffects.None);
                    }
                    Main.EntitySpriteDraw(t.Value, pos - Main.screenPosition, null, Lighting.GetColor(pos.ToTileCoordinates()) * Projectile.Opacity, Projectile.rotation, t.Size() / 2, Projectile.scale, SpriteEffects.None);

                }
            }
            if (timer > 300)
            {
                for (int i = 0; i < Projectile.Distance(owner.Center); i += (int)(17 * Projectile.scale))
                {
                    Vector2 pos = Projectile.Center + new Vector2(-i, 0).RotatedBy(Projectile.AngleFrom(owner.Center));
                    for (int j = 0; j < 12; j++)
                    {
                        Vector2 afterimageOffset = (MathHelper.TwoPi * j / 12f).ToRotationVector2() * 1f;
                        Color glowColor = Color.Red with { A = 0 } * 0.7f;


                        Main.EntitySpriteDraw(fire.Value, pos + afterimageOffset - Main.screenPosition, null, glowColor * 0.5f, Projectile.rotation + i, fire.Size() / 2, Projectile.scale * 0.6f, SpriteEffects.None);
                    }
                    Main.EntitySpriteDraw(fire.Value, pos - Main.screenPosition, null, Lighting.GetColor(pos.ToTileCoordinates()) * 0.5f, Projectile.rotation + i, fire.Size() / 2, (Projectile.scale * 0.6f), SpriteEffects.None);

                }
            }
            return false;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 120);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.ai[0] < 0)
            {
                Projectile.Kill();
                return false;
            }
            NPC owner = Main.npc[(int)Projectile.ai[0]];
            if (owner == null || !owner.active || owner.type != ModContent.NPCType<PyrogenBoss>())
            {

                Projectile.Kill();
                return false;

            }
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, owner.Center) && Projectile.ai[2] > 300;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }

        public const int ActiveTime = 600;

        public override void AI()
        {
            Projectile.hide = true;

            if (Projectile.ai[0] < 0)
            {

                Projectile.Kill();
                return;
            }
            NPC owner = Main.npc[(int)Projectile.ai[0]];
            ref float timer = ref Projectile.ai[2];
            if (owner == null || !owner.active || owner.type != ModContent.NPCType<PyrogenBoss>())
            {

                Projectile.Kill();
                return;

            }
            else
            {
                Projectile.timeLeft = 2;
            }

            timer++;

            Vector2 direction = Projectile.ai[1].ToRotationVector2();

            // Endpoint sits directly on the inner arena wall.
            Projectile.Center = owner.Center + direction * PyrogenBoss.ArenaRadius;
            Projectile.velocity = Vector2.Zero;

            // Chain line faces from boss to wall.
            Projectile.rotation = direction.ToRotation();

            if (timer >= 200)
            {
                Projectile.Opacity = 1f;
                timer++;
            }
            else
            {
                Projectile.Opacity = 0.2f;
            }
            if (timer >= 200 && timer <= 300)
            {

                for (int i = 0; i < 5; i++)
                    Dust.NewDustDirect(owner.Center - Projectile.Size / 2 + new Vector2(0, -((timer - 200) * Projectile.Distance(owner.Center) / 100)).RotatedBy(Projectile.rotation + MathHelper.PiOver2), Projectile.width, Projectile.height, DustID.Torch, Scale: 2).noGravity = true;

            }
            if (timer == 300)
            {
                for (int i = 0; i < Projectile.Distance(owner.Center); i += 3)
                {
                    Dust.NewDustDirect(owner.Center - Projectile.Size / 2 + new Vector2(0, -i).RotatedBy(Projectile.rotation + MathHelper.PiOver2), Projectile.width, Projectile.height, DustID.Torch, Scale: 2).noGravity = true;
                }
            }
            if (timer >= 100 + ActiveTime)
            {
                timer = 100;
                for (int i = 0; i < Projectile.Distance(owner.Center); i += 3)
                {
                    Dust.NewDustDirect(owner.Center - Projectile.Size + new Vector2(0, -i).RotatedBy(Projectile.rotation + MathHelper.PiOver2), Projectile.width * 2, Projectile.height * 2, DustID.Torch, Scale: 2).noGravity = true;
                }
                SoundEngine.PlaySound(CalamityMod.NPCs.Cryogen.CryogenShield.BreakSound, owner.Center);
            }

            for (int i = 0; i < 5; i++)
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Scale: 2).noGravity = true;
            base.AI();
        }
    }
}
