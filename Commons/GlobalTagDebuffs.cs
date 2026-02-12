using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Sounds;

namespace Clamity.Commons
{
    public class GlobalTagDebuffs : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int exowhipIndex = -1;
        public int detlIndex = -1;
        public int elemlashIndex = -1;
        public int baldualIndex = -1;
        public int plagbrinIndex = -1;
        public int stormIndex = -1;
        public int triIndex = -1;
        public int terrawhipIndex = -1; // this is for tier 2 terra whip
        public int terralashIndex = -1; // this is for tier 1 terra whip

        public int activeTag = -1;
        int removedTagImmunity = -1;
        public int tagDamage = 0;
        private int _tagCrit = 0;

        public int tagCrit
        {
            get => _tagCrit;
            set
            {
                if (value > _tagCrit)
                    _tagCrit = value;
            }
        }

        //extras

        public bool Stormlash_BeenSparked;

        public override void ResetEffects(NPC npc)
        {
            exowhipIndex = -1;
            detlIndex = -1;
            elemlashIndex = -1;
            baldualIndex = -1;
            plagbrinIndex = -1;
            stormIndex = -1;
            terrawhipIndex = -1; // this is for tier 2 terra whip
            terralashIndex = -1; // this is for tier 1 terra whip

            activeTag = -1;
            tagDamage = 0;
            _tagCrit = 0;

            Stormlash_BeenSparked = false;
        }

        public void RemoveTagDebuffs(NPC npc)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            foreach (int buff in npc.buffType)
            {
                if (buff < BuffID.Sets.IsATagBuff.Length && buff > -1)
                {
                    if (BuffID.Sets.IsATagBuff[buff] == true)
                    {
                        if (npc.FindBuffIndex(buff) < npc.buffType.Length && npc.FindBuffIndex(buff) >= 0)
                        {
                            npc.DelBuff(npc.FindBuffIndex(buff));
                        }
                    }
                }
            }

            ResetEffects(npc);
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            base.DrawEffects(npc, ref drawColor);

            if (Main.gameMenu || Main.gamePaused)
                return;

            //sparking effect from stormlash tag
            if (Stormlash_BeenSparked)
            {
                Vector2 position = npc.Center + ClamityUtils.ProportionalOffset(npc, 1.2f);
                Vector2 velocity = -Vector2.UnitY * Main.rand.NextFloat(0.2f, 2.8f);

                CreateDust(DustID.Electric, velocity, position, scale: Main.rand.NextFloat(0.5f, 0.8f));
            }
        }
        public Dust CreateDust(int type, Vector2 vel, Vector2 pos, Color col = default, float scale = 1f, int alpha = 0, bool rotate = false, bool noGrav = true)
        {
            var d = Dust.NewDustPerfect(pos, type);

            d.position = pos;
            d.velocity = !rotate ? vel : vel.RotatedByRandom(MathHelper.TwoPi);
            d.color = (col == default) ? Color.White : col;
            d.alpha = alpha;
            d.scale = scale;
            d.noGravity = noGrav;

            return d; //returns the dust so you can freely modify it afterwards if needed
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (projectile.owner == Main.myPlayer && !projectile.trap && (projectile.minion || ProjectileID.Sets.MinionShot[projectile.type]))
            {
                // Exo Whip Tag ( adjusted )
                /*if (activeTag == ModContent.BuffType<ExoWhipDebuff>() && exowhipIndex > -1)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (Main.rand.NextFloat() < 0.15f)
                        {
                            Vector2 position = npc.Center + new Vector2(Main.rand.Next(-400, 400), -600f);
                            position.Y -= 100;
                            Vector2 heading = projectile.Center - position;
                            if (heading.Y < 0f)
                            {
                                heading.Y *= -1f;
                            }
                            if (heading.Y < 20f)
                            {
                                heading.Y = 20f;
                            }
                            heading.Normalize();
                            float speedX = heading.X * 5;
                            float speedY = heading.Y * 5;
                            heading *= new Vector2(speedX, speedY).Length();

                            int proj = Projectile.NewProjectile(Projectile.InheritSource(projectile), position, new Vector2(speedX, speedY), ModContent.ProjectileType<Exobeam>(), 340, projectile.knockBack, projectile.owner);
                            Main.projectile[proj].minion = true;
                            Main.projectile[proj].DamageType = DamageClass.Summon;

                            if (Main.rand.NextFloat() < 0.07f) // 7% chance
                            {
                                Player player = Main.player[projectile.owner];
                                int healAmount = (int)(player.statLifeMax2 * 0.10f);
                                player.statLife += healAmount;
                                player.HealEffect(healAmount);
                            }
                        }
                    }
                    npc.DelBuff(exowhipIndex);
                }*/

                // D.E.T.L Tag
                /*if (activeTag == ModContent.BuffType<detlDebuff>() && detlIndex > -1)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (Main.rand.NextFloat() < 0.25f)
                        {
                            Vector2 position = npc.Center + new Vector2(Main.rand.Next(-400, 400), -600f);
                            position.Y -= 100;
                            Vector2 heading = projectile.Center - position;
                            if (heading.Y < 0f)
                            {
                                heading.Y *= -1f;
                            }
                            if (heading.Y < 20f)
                            {
                                heading.Y = 20f;
                            }
                            heading.Normalize();
                            float speedX = heading.X * 20;
                            float speedY = heading.Y * 20;
                            heading *= new Vector2(speedX, speedY).Length();

                            int proj = Projectile.NewProjectile(Projectile.InheritSource(projectile), position, new Vector2(speedX, speedY), ModContent.ProjectileType<WingmanShot>(), 100, projectile.knockBack, projectile.owner);
                            Main.projectile[proj].minion = true;
                            Main.projectile[proj].DamageType = DamageClass.Summon;

                            if (Main.rand.NextFloat() < 0.025f)
                            {
                                proj = Projectile.NewProjectile(Projectile.InheritSource(projectile), position, new Vector2(speedX, speedY), ModContent.ProjectileType<WingmanGrenade>(), 250, projectile.knockBack, projectile.owner);
                                Main.projectile[proj].minion = true;
                                Main.projectile[proj].DamageType = DamageClass.Summon;
                            }
                        }
                    }
                    npc.DelBuff(detlIndex);
                }*/

                // Elemental Lash Tag
                /*if (activeTag == ModContent.BuffType<ElementalLashDebuff>() && elemlashIndex > -1)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (Main.rand.NextFloat() < 0.25f)
                        {
                            Vector2 position = npc.Center + new Vector2(Main.rand.Next(-140, 140), -140f);
                            Vector2 heading = projectile.Center - position;
                            if (heading.Y < 0f)
                            {
                                heading.Y *= -1f;
                            }
                            if (heading.Y < 10f)
                            {
                                heading.Y = 10f;
                            }
                            heading.Normalize();
                            float speedX = heading.X * 5;
                            float speedY = heading.Y * 10;
                            heading *= new Vector2(speedX, speedY).Length();

                            int proj = Projectile.NewProjectile(Projectile.InheritSource(projectile), position, new Vector2(speedX, speedY), ModContent.ProjectileType<ElementalFlare>(), 165, projectile.knockBack, projectile.owner);
                            Main.projectile[proj].minion = true;
                            Main.projectile[proj].DamageType = DamageClass.Summon;
                        }
                    }
                    npc.DelBuff(elemlashIndex);
                }*/

                // Stormlion Whip Tag 
                /*if (activeTag == BuffType<StormlionWhipDebuff>() && stormIndex > -1)
                {
                    //if they have been zapped already, dont zap them again until they are unzapped
                    if (!Stormlash_BeenSparked)
                    {
                        Vector2 pos = npc.Center + ProportionalOffset(npc, 0.1f);

                        float x = Main.rand.NextFloat(-150f, 150f);

                        if (Abs(x) < 65)
                            x = 65 * Sign(x);

                        Vector2 spawn = pos + new Vector2(x, Main.rand.NextFloat(-700f, -610f));

                        SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Pitch = -0.4f, Volume = 0.5f }, pos);
                        SoundEngine.PlaySound(CommonCalamitySounds.LightningSound with { Pitch = -0.1f, Volume = 0.8f }, pos);

                        pos.Screenshake(10f, 2f, 500f, 4);

                        StormlionWhipLightning lightning = Projectile.NewProjectileDirect(
                            npc.GetSource_OnHurt(projectile),
                            spawn, 
                            Vector2.Zero,
                            ProjectileType<StormlionWhipLightning>(),
                            projectile.damage * 3,
                            0f, 
                            projectile.owner
                        ).As<StormlionWhipLightning>();

                        lightning.startLocation = pos;
                        lightning.endLocation = spawn;

                        npc.DelBuff(stormIndex);
                        stormIndex = -1;

                        npc.AddBuff(BuffType<StormlionWhipDebuff2>(), 360);
                    }
                }*/

                // TriInfecta Whip Tag
                /*if (activeTag == ModContent.BuffType<TriInfectaDebuff>() && triIndex > -1)
                {
                    if (Main.rand.NextFloat() <= 0.20f)
                    {
                        for (int i = 0; i < Main.rand.Next(3, 5); i++)
                        {
                            float speed = 5f;
                            Vector2 randomVector = new Vector2(speed * (Main.rand.Next(-15, 15) / 10f), -speed * (Main.rand.Next(-10, 10) / 10f));

                            int proj = Projectile.NewProjectile(Projectile.InheritSource(projectile), projectile.Center, randomVector, ModContent.ProjectileType<Blood>(), 20, projectile.knockBack, projectile.owner);
                            Main.projectile[proj].DamageType = DamageClass.Summon;
                        }
                    }

                    npc.DelBuff(triIndex);
                    triIndex = -1;
                }*/


                // Terra Whip Tag
                /*if (activeTag == ModContent.BuffType<TerraWhipDebuff>() && terrawhipIndex > -1)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (Main.rand.NextFloat() < 0.15f) // 15% chance
                        {
                            Vector2 position = npc.Center + new Vector2(Main.rand.Next(-180, 180), -180f);
                            Vector2 heading = projectile.Center - position;
                            if (heading.Y < 0f)
                            {
                                heading.Y *= -1f;
                            }
                            if (heading.Y < 15f)
                            {
                                heading.Y = 15f;
                            }
                            heading.Normalize();
                            float speedX = heading.X * 5;
                            float speedY = heading.Y * 15;
                            heading *= new Vector2(speedX, speedY).Length();

                            int proj = Projectile.NewProjectile(Projectile.InheritSource(projectile), position, new Vector2(speedX, speedY), ModContent.ProjectileType<WildfireBloomFlare>(), 175, projectile.knockBack, projectile.owner);
                            Main.projectile[proj].minion = true;
                            Main.projectile[proj].DamageType = DamageClass.Summon;

                            if (Main.rand.NextFloat() < 0.10f) // 10% chance
                            {
                                proj = Projectile.NewProjectile(Projectile.InheritSource(projectile), position, new Vector2(speedX, speedY), ModContent.ProjectileType<TerratomereSwordBeam>(), 260, projectile.knockBack, projectile.owner);
                                Main.projectile[proj].minion = true;
                                Main.projectile[proj].DamageType = DamageClass.Summon;
                            }

                            if (Main.rand.NextFloat() < 0.05f) // 5% chance
                            {
                                Player player = Main.player[projectile.owner];
                                int healAmount = (int)(player.statLifeMax2 * 0.10f);
                                player.statLife += healAmount;
                                player.HealEffect(healAmount);
                            }
                        }
                    }
                    npc.DelBuff(terrawhipIndex);
                }*/
            }
        }
    }
}