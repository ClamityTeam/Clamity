using CalamityMod;
using CalamityMod.Events;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;


namespace Clamity.Content.Bosses.Pyrogen.Projectiles
{
    public class FireBarrage : BrimstoneBarrage, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            if (!ModLoader.TryGetMod("Redemption", out var redemption))
                return;
            redemption.Call("addElementProj", 2, Type);
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {

        }
    }
    public class FireBarrageHoming : FireBarrage, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "Clamity/Content/Bosses/Pyrogen/Projectiles/FireBarrage";
        public int TargetIndex = -1;
        public override void AI()
        {
            base.AI();

            if (TargetIndex >= 0)
            {
                if (!Main.npc[TargetIndex].active || !Main.npc[TargetIndex].CanBeChasedBy())
                {
                    TargetIndex = -1;
                }
                else
                {
                    Vector2 value = Projectile.SafeDirectionTo(Main.npc[TargetIndex].Center)/* * (Projectile.velocity.Length() + 3.5f)*/;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, value, 0.01f);
                }
            }

            if (TargetIndex == -1)
            {
                NPC nPC = Projectile.Center.ClosestNPCAt(1600f);
                if (nPC != null)
                {
                    TargetIndex = nPC.whoAmI;
                }
            }
        }
    }
    public class Fireblast : SCalBrimstoneFireblast, ILocalizedModType, IModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            if (!ModLoader.TryGetMod("Redemption", out var redemption))
                return;
            redemption.Call("addElementProj", 2, Type);
        }
        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= 5)
                Projectile.frame = 0;

            bool revenge = CalamityWorld.revenge || BossRushEvent.BossRushActive;

            Lighting.AddLight(Projectile.Center, 0.9f * Projectile.Opacity, 0f, 0f);

            if (!withinRange)
            {
                if (Projectile.ai[2] == 1f)
                    Projectile.Opacity = MathHelper.Clamp(Projectile.timeLeft / 60f, 0f, 1f);
                else
                    Projectile.Opacity = MathHelper.Clamp(1f - ((Projectile.timeLeft - 130) / 20f), 0f, 1f);
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
            }

            int target = (int)Projectile.ai[0];

            if (!withinRange)
            {
                float inertia = revenge ? 80f : 100f;
                float homeSpeed = revenge ? 13f : 9f;
                float minDist = 40f;
                if (target >= 0 && Main.player[target].active && !Main.player[target].dead)
                {
                    if (Projectile.Distance(Main.player[target].Center) > minDist)
                    {
                        Vector2 moveDirection = Projectile.SafeDirectionTo(Main.player[target].Center, Vector2.UnitY);
                        Projectile.velocity = (Projectile.velocity * (inertia - 1f) + moveDirection * homeSpeed) / inertia;
                    }
                }
                else
                {
                    if (Projectile.ai[0] != -1f)
                    {
                        Projectile.ai[0] = -1f;
                        Projectile.netUpdate = true;
                    }
                }
            }

            float targetDist;
            if (target != -1 && !Main.player[target].dead && Main.player[target].active && Main.player[target] != null)
                targetDist = Vector2.Distance(Main.player[target].Center, Projectile.Center);
            else
                targetDist = 1000;

            if (Projectile.ai[1] == 2f && !withinRange && Main.rand.NextBool())
            {
                SparkParticle orb = new SparkParticle(Projectile.Center - Projectile.velocity + Main.rand.NextVector2Circular(20, 20), -Projectile.velocity * Main.rand.NextFloat(0.1f, 1f), false, 14, Main.rand.NextFloat(0.35f, 0.6f), (Main.rand.NextBool() ? Color.Lerp(Color.Red, Color.Magenta, 0.5f) : Color.Red) * Projectile.Opacity);
                GeneralParticleHandler.SpawnParticle(orb);
            }
            if ((Projectile.timeLeft == 1 && !withinRange) || (targetDist < 224 && Projectile.Opacity == 1f)) // When within 14 blocks of player or when it runs out of time
            {
                if (!setLifetime)
                {
                    Projectile.timeLeft = 60;
                    setLifetime = true;
                }
                withinRange = true;
            }
            if (withinRange && Projectile.ai[2] == 0f)
            {
                Projectile.velocity *= 0.9f;
                for (int i = 0; i < 2; i++)
                {
                    Dust failShotDust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(3) ? 60 : 114);
                    failShotDust.noGravity = true;
                    failShotDust.velocity = new Vector2(4, 4).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1.3f);
                    failShotDust.scale = Main.rand.NextFloat(0.7f, 1.8f);
                }
                if (Projectile.timeLeft <= 40)
                {
                    if (Projectile.Opacity > 0)
                        Projectile.Opacity -= 0.05f;
                }
                if (Projectile.timeLeft == 30)
                {
                    Projectile.Opacity = 0;
                    Projectile.velocity *= 0;
                    for (int i = 0; i < 2; i++)
                    {
                        Particle bloom = new BloomParticle(Projectile.Center, Vector2.Zero, new Color(248, 147, 79), 0.1f, 0.7f, 30, false);
                        GeneralParticleHandler.SpawnParticle(bloom);
                        if (Projectile.ai[2] == 1f)
                            bloom.Lifetime = 0;
                    }
                }
                if (Projectile.timeLeft == 15)
                {
                    Particle bloom = new BloomParticle(Projectile.Center, Vector2.Zero, Color.Red, 0.1f, 0.65f, 15, false);
                    GeneralParticleHandler.SpawnParticle(bloom);
                }
                if (Projectile.timeLeft == 8)
                {
                    Particle bloom = new BloomParticle(Projectile.Center, Vector2.Zero, Color.White, 0.1f, 0.5f, 8, false);
                    GeneralParticleHandler.SpawnParticle(bloom);
                }
            }
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {

        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(in ImpactSound, base.Projectile.Center);
            bool bossRushActive = BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || bossRushActive;
            bool rev = CalamityWorld.revenge || bossRushActive;
            bool expert = Main.expertMode || bossRushActive;
            if (base.Projectile.ai[1] == 0f && base.Projectile.owner == Main.myPlayer)
            {
                int num = (bossRushActive ? 20 : (death ? 12 : (rev ? 10 : (expert ? 8 : 6))));
                float num2 = MathF.PI * 2f / (float)num;
                int type = ModContent.ProjectileType<FireBarrage>();
                float num3 = 7f;
                Vector2 spinningpoint = new Vector2(0f, 0f - num3);
                for (int i = 0; i < num; i++)
                {
                    Vector2 velocity = spinningpoint.RotatedBy(num2 * (float)i);
                    Projectile.NewProjectile(base.Projectile.GetSource_FromThis(), base.Projectile.Center, velocity, type, (int)Math.Round((double)base.Projectile.damage * 0.75), 0f, base.Projectile.owner, 0f, 1f);
                }
            }

            int type2 = 235;

            Dust.NewDust(base.Projectile.position, base.Projectile.width, base.Projectile.height, type2, 0f, 0f, 50);
            for (int j = 0; j < 10; j++)
            {
                int num4 = Dust.NewDust(base.Projectile.position, base.Projectile.width, base.Projectile.height, type2, 0f, 0f, 0, default(Color), 1.5f);
                Main.dust[num4].noGravity = true;
                Main.dust[num4].velocity *= 3f;
                num4 = Dust.NewDust(base.Projectile.position, base.Projectile.width, base.Projectile.height, type2, 0f, 0f, 50);
                Main.dust[num4].velocity *= 2f;
                Main.dust[num4].noGravity = true;
            }
        }
    }
}
