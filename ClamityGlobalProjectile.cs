using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Accessories;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Projectiles.Typeless;
using Clamity.Content.Bosses.ForbiddenLantern.Items;
using Clamity.Content.Items.Accessories.GemCrawlerDrop;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static System.Net.Mime.MediaTypeNames;

namespace Clamity
{
    public class ClamityGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public float[] extraAI = new float[5];
        public bool IsSentryRelated = false;

        public bool subShot = false;
        public int subShotTimer = 0;
        public bool shatteredBullet = false;
        public bool aquarelBullet = false;
        public bool abysscentBullet = false;
        public bool lacescentBullet = false;
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[projectile.owner];

            UpdateAflameAccesory(projectile, target, hit, damageDone);


            if (!subShot)
            {
                if (shatteredBullet)
                {
                    target.AddBuff(BuffID.Wet, 120);

                    for (int i = 0; i < 3; i++)
                    {
                        /*Projectile cobyShot = Projectile.NewProjectileDirect(projectile.GetSource_FromAI(), projectile.Center, projectile.velocity.RotatedByRandom(MathHelper.Pi), projectile.type, projectile.damage / 4, projectile.knockBack, projectile.owner);
                        ClamityGlobalProjectile cgp = cobyShot.Clamity();
                        cgp.shatteredBullet = true;
                        cgp.subShot = true;*/


                        Projectile shardShot = Projectile.NewProjectileDirect(projectile.GetSource_FromAI(), projectile.Center, projectile.velocity.RotatedByRandom(MathHelper.Pi), ModContent.ProjectileType<ShatteredPistolShard>(), projectile.damage / 4, projectile.knockBack, projectile.owner);

                    }
                }
                if (aquarelBullet)
                {
                    target.AddBuff(BuffID.Wet, 120);
                    target.AddBuff(BuffID.Frostburn2, 120);


                    for (int i = 0; i < 6; i++)
                    {
                        /*Projectile cobyShot = Projectile.NewProjectileDirect(projectile.GetSource_FromAI(), projectile.Center, projectile.velocity.RotatedByRandom(MathHelper.Pi), projectile.type, projectile.damage / 4, projectile.knockBack, projectile.owner);
                        ClamityGlobalProjectile cgp = cobyShot.Clamity();
                        cgp.shatteredBullet = true;
                        cgp.subShot = true;*/


                        Projectile shardShot = Projectile.NewProjectileDirect(projectile.GetSource_FromAI(), projectile.Center, projectile.velocity.RotatedByRandom(MathHelper.Pi), ModContent.ProjectileType<ShatteredPistolShard>(), projectile.damage / 4, projectile.knockBack, projectile.owner, 1);

                    }
                }
            }
        }
        private void UpdateAflameAccesory(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[projectile.owner];
            ClamityPlayer modPlayer = player.Clamity();
            /*if (modPlayer.aflameAccList.Contains(ModContent.ItemType<LuxorsGift>()))
            {

            }*/
            List<int> list = modPlayer.aflameAccList;
            AddVulHexDebuff(list, projectile, target, ItemID.VolatileGelatin, ProjectileID.VolatileGelatinBall);
            AddVulHexDebuff(list, projectile, target, ItemID.BoneGlove, ProjectileID.BoneGloveProj);
            AddVulHexDebuff(list, projectile, target, ItemID.BoneHelm, 964);
            AddVulHexDebuff(list, projectile, target, ItemID.SporeSac, ProjectileID.SporeTrap, ProjectileID.SporeTrap2, ProjectileID.SporeGas, ProjectileID.SporeGas2, ProjectileID.SporeGas3);

            AddVulHexDebuff(list, projectile, target, ModContent.ItemType<LuxorsGift>(), ModContent.ProjectileType<LuxorsGiftMelee>(), ModContent.ProjectileType<LuxorsGiftRanged>(), ModContent.ProjectileType<LuxorsGiftMagic>(), ModContent.ProjectileType<LuxorsGiftRogue>(), ModContent.ProjectileType<LuxorsGiftSummon>());
            AddVulHexDebuff(list, projectile, target, ModContent.ItemType<FungalClump>(), ModContent.ProjectileType<FungalClumpMinion>());
            AddVulHexDebuff(list, projectile, target, new int[] { ModContent.ItemType<HeartoftheElements>() }, ModContent.ProjectileType<SandBolt>(), 32);
            AddVulHexDebuff(list, projectile, target, new int[] { ModContent.ItemType<EyeoftheStorm>(), ModContent.ItemType<HeartoftheElements>() }, ModContent.ProjectileType<CloudElementalMinion>());
            AddVulHexDebuff(list, projectile, target, new int[] { ModContent.ItemType<RoseStone>(), ModContent.ItemType<HeartoftheElements>() }, ModContent.ProjectileType<BrimstoneFireballMinion>(), ModContent.ProjectileType<BrimstoneExplosionMinion>());
            AddVulHexDebuff(list, projectile, target, new int[] { ModContent.ItemType<PearlofEnthrallment>(), ModContent.ItemType<HeartoftheElements>() }, ModContent.ProjectileType<WaterSpearFriendly>(), ModContent.ProjectileType<FrostMistFriendly>(), ModContent.ProjectileType<WaterElementalSong>());

            AddVulHexDebuff(list, projectile, target, new int[] { ModContent.ItemType<ProfanedSoulArtifact>(), ModContent.ItemType<ProfanedSoulCrystal>() }, ModContent.ProjectileType<MiniGuardianDefense>(), /*ModContent.ProjectileType<MiniGuardianSpear>(),*/ ModContent.ProjectileType<MiniGuardianAttack>());
            AddVulHexDebuff(list, projectile, target, ModContent.ItemType<AngelicAlliance>(), ModContent.ProjectileType<AngelicAllianceArchangel>(), ModContent.ProjectileType<AngelRay>());

            AddVulHexDebuff(list, projectile, target, ModContent.ItemType<StatisVoidSash>(), ModContent.ProjectileType<CosmicScythe>());
        }
        private void AddVulHexDebuff(List<int> list, Projectile proj, NPC target, int acc, params int[] projList)
        {
            if (list.Contains(acc))
            {
                foreach (int i in projList)
                {
                    if (proj.type == i)
                    {
                        target.AddBuff(ModContent.BuffType<VulnerabilityHex>(), 120);
                        break;
                    }
                }
            }
        }
        private void AddVulHexDebuff(List<int> list, Projectile proj, NPC target, int[] accs, params int[] projList)
        {
            foreach (int item in accs)
            {
                if (list.Contains(item))
                {
                    foreach (int i in projList)
                    {
                        if (proj.type == i)
                        {
                            target.AddBuff(ModContent.BuffType<VulnerabilityHex>(), 120);
                            break;
                        }
                    }
                }
            }
        }
        /*private void Shortstrike(Player player, Projectile proj, int buffID, float timeInSeconds, int projectileID, float percent = 2)
        {
            if (proj.type == projectileID)
            {
                player.AddBuff(buffID, CalamityUtils.SecondsToFrames(timeInSeconds));
                player.AddCooldown(ShortstrikeCooldown.ID, (int)(CalamityUtils.SecondsToFrames(timeInSeconds) * percent));
                for (float i = 0; i < MathHelper.TwoPi; i += MathHelper.PiOver4 / 3)
                {
                    Dust dust = Dust.NewDustPerfect(proj.Center + proj.velocity, DustID.Electric, Vector2.UnitX.RotatedBy(i) * 3f + proj.velocity);
                    dust.noGravity = true;
                }
            }
        }*/
        public override bool? CanHitNPC(Projectile projectile, NPC target)
        {
            //if (subShotTimer > 0) Main.NewText(subShotTimer);
            if (subShot && subShotTimer < 60)
            {
                return false;
            }

            return base.CanHitNPC(projectile, target);
        }
        public override void OnSpawn(Projectile proj, IEntitySource source)
        {
            Player player = Main.player[proj.owner];

            if (ProjectileID.Sets.SentryShot[proj.type] || proj.sentry)
            {
                IsSentryRelated = true;
            }

            if (source is EntitySource_ItemUse_WithAmmo)
            {
                if (proj.arrow && player.Clamity().gemAmethyst && !player.Clamity().gemFinal && Main.rand.NextBool(3))
                {
                    float d = player.GetTotalDamage<RangedDamageClass>().ApplyTo(4);
                    int p = Projectile.NewProjectile(proj.GetSource_FromAI(), proj.Center, proj.velocity, ModContent.ProjectileType<SharpAmethystProj>(), (int)d, 1f, proj.owner);
                    Main.projectile[p].DamageType = DamageClass.Ranged;
                }
            }
        }
        public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            for (int i = 0; i < extraAI.Length; i++)
                binaryWriter.Write(extraAI[i]);
            binaryWriter.Write(IsSentryRelated);
        }
        public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
        {
            for (int i = 0; i < extraAI.Length; i++)
                extraAI[i] = binaryReader.ReadSingle();
            IsSentryRelated = binaryReader.ReadBoolean();
        }
        public override void AI(Projectile projectile)
        {
            Player player = Main.player[projectile.owner];
            if (projectile.Opacity > 0 && projectile.scale > 0.01f) // Only apply bullet visuals if the bullet is visible
            { 
                if (shatteredBullet || aquarelBullet)
                {
                    float targetDist = Vector2.Distance(player.Center, projectile.Center);
                    if (targetDist < 1400f)
                    {
                        SparkParticle spark = new SparkParticle(projectile.Center + projectile.velocity, -projectile.velocity * 0.05f, false, 2, 0.55f * projectile.scale, (aquarelBullet ? Color.Cyan : Color.LightBlue) * 0.75f);
                        GeneralParticleHandler.SpawnParticle(spark);
                        if (aquarelBullet)
                        {
                            if (Main.rand.NextBool(3))
                            {
                                SparkParticle spark2 = new SparkParticle(projectile.Center + Main.rand.NextVector2Circular(6, 6) * projectile.scale, -projectile.velocity * Main.rand.NextFloat(0.05f, 0.4f), false, 20, 0.4f * projectile.scale, Color.Cyan * 0.75f);
                                GeneralParticleHandler.SpawnParticle(spark2);
                            }
                        }

                        if (Main.rand.NextBool())
                        {
                            Gore bubble = Gore.NewGorePerfect(projectile.GetSource_FromAI(), projectile.position, projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 411);
                            bubble.timeLeft = 9 + Main.rand.Next(7);
                            bubble.scale = Main.rand.NextFloat(0.6f, 1f);
                            bubble.type = Main.rand.NextBool(3) ? 412 : 411;
                        }
                    }
                }
            }
        }
        public override void PostAI(Projectile projectile)
        {
            if (subShot) subShotTimer++;            
        }
    }
}
