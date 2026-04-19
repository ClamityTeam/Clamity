using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Clamity.Commons;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Clamity.Content.Items.Weapons.Summon.Whips
{
    public class TriInfecta : BaseWhipItem
    {
        private const int SmallUseTime = 40;
        private const int MediumUseTime = 30;
        private const int LargeUseTime = 20;

        //private int combo = 1;

        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            //Item.DefaultToWhip(ModContent.ProjectileType<TriInfectaSmallProj>(), 40, 4, 8, SmallUseTime);
            Item.DefaultToWhip(ModContent.ProjectileType<TriInfectaSpawner>(), 40, 4, 4, 50);
            Item.autoReuse = true;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Orange;
            Item.channel = true;
        }

        public override bool MeleePrefix()
        {
            return true;
        }

        /*public override void UpdateInventory(Player player)
        {
            switch (combo)
            {
                case 1:
                    Item.useTime = Item.useAnimation = SmallUseTime;
                    Item.shoot = ModContent.ProjectileType<TriInfectaSmallProj>();
                    break;
                case 2:
                    Item.useTime = Item.useAnimation = MediumUseTime;
                    Item.shoot = ModContent.ProjectileType<TriInfectaMedProj>();
                    break;
                case 3:
                case 4:
                    Item.useTime = Item.useAnimation = LargeUseTime;
                    Item.shoot = ModContent.ProjectileType<TriInfectaLargeProj>();
                    break;
            }
        }*/

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            /*Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            combo++;
            if (combo > 4)
                combo = 1;

            return false;*/
            return player.ownedProjectileCounts[type] < 1;
        }

        /*public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CrimtaneBar, 5)
                .AddIngredient(ModContent.ItemType<BloodSample>(), 7)
                .AddIngredient(ItemID.Vertebrae, 6)
                .AddTile(TileID.DemonAltar)
                .Register();
        }*/
    }

    public class TriInfectaSpawner : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon.Whips";

        private bool stoppedChanneling1;

        private bool stoppedChanneling2;

        private bool runOnce = true;

        public override string Texture => this.GetPath().Replace("Spawner", "LargeProj");

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.hide = true;
            Projectile.manualDirectionChange = true;
            Projectile.timeLeft = 20;
        }

        public override void AI()
        {
            Player Player = Main.player[Projectile.owner];
            ClamityPlayer cata = Player.Clamity();

            float speed = Player.HeldItem.shootSpeed;
            bool channeling = Player.channel;
            Vector2 rotatedRelativePoint = Player.RotatedRelativePoint(Player.MountedCenter, false, true);
            if (Main.myPlayer == Projectile.owner)
            {
                if ((channeling || Projectile.timeLeft > 2) && !Player.noItems && !Player.CCed && Player.HasAmmo(Player.HeldItem))
                {
                    ClamityUtils.UpdateHeldProjDoVelocity(Player, rotatedRelativePoint, Projectile);
                }
                else
                {
                    stoppedChanneling1 = true;
                    if (stoppedChanneling2)
                    {
                        Projectile.Kill();
                    }
                    else
                    {
                        ClamityUtils.UpdateHeldProjDoVelocity(Player, rotatedRelativePoint, Projectile);
                    }
                }
            }
            if (Player.itemTime <= 2)
            {
                Player.itemTime = 2;
            }
            if (Player.itemAnimation <= 2)
            {
                Player.itemAnimation = 2;
            }
            if (Projectile.timeLeft > Player.itemAnimationMax)
            {
                Projectile.timeLeft = Player.itemAnimationMax;
            }
            else if (Projectile.timeLeft <= 2)
            {
                Projectile.timeLeft = 2;
            }
            ClamityUtils.UpdateHeldProj(Player, rotatedRelativePoint, 60f, Projectile, setTimeleft: false, updateArm: false);
            float[] attackSpeedScale = new float[3] { 0.5f, 0.33f, 0.15f };
            if (Main.myPlayer == Projectile.owner)
            {
                if (runOnce)
                {
                    ClamityGlobalProjectile.MimicLuxorAndDynamoCells(Projectile, Player.itemTimeMax);
                    runOnce = false;
                }
                if (Projectile.ai[0] == 1f || Projectile.ai[0] == (int)(Player.itemAnimationMax * attackSpeedScale[0]))
                {
                    Vector2 velocity = Projectile.velocity * speed;
                    int proj = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, velocity, ModContent.ProjectileType<TriInfectaLargeProj>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    TriInfectaLargeProj obj = Main.projectile[proj].ModProjectile as TriInfectaLargeProj;
                    obj.spawner = Projectile;
                    //Main.NewText(velocity);
                }
                if (Projectile.ai[1] == (int)(Player.itemAnimationMax * attackSpeedScale[1]))
                {
                    Vector2 velocity = Projectile.velocity * speed;
                    int proj = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, velocity, ModContent.ProjectileType<TriInfectaMedProj>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    TriInfectaMedProj obj = Main.projectile[proj].ModProjectile as TriInfectaMedProj;
                    obj.spawner = Projectile;
                    //Main.NewText(velocity.ToString());
                }
                if (Projectile.ai[2] == (int)(Player.itemAnimationMax * attackSpeedScale[2]))
                {
                    Vector2 velocity = Projectile.velocity * speed;
                    int proj = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, velocity, ModContent.ProjectileType<TriInfectaSmallProj>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    TriInfectaSmallProj obj = Main.projectile[proj].ModProjectile as TriInfectaSmallProj;
                    obj.spawner = Projectile;
                    //Main.NewText(velocity.ToString());
                }
            }
            for (int i = 0; i < 3; i++)
            {
                Projectile.ai[i] += 1f;
                if (Projectile.ai[i] > Player.itemAnimationMax * 4f * attackSpeedScale[i])
                {
                    if (stoppedChanneling1)
                    {
                        stoppedChanneling2 = true;
                    }
                    Projectile.ai[i] = 0f;
                }
            }
            //Main.NewText(Player.HeldItem.shootSpeed + " " +(int)Projectile.ai[0] + " " + (int)Projectile.ai[1] + " " + (int)Projectile.ai[2]);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

    }

    public class TriInfectaSmallProj : BaseWhipProjectile
    {
        public Projectile spawner;
        private bool reset = true;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2; // cal trail mode
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetWhipStats()
        {
            Projectile.localNPCHitCooldown = -1;
            Projectile.WhipSettings.Segments = 10;
            Projectile.WhipSettings.RangeMultiplier = 0.4f;

            segmentRotation = -MathF.PI / 2;
            fishingLineColor = Color.Purple;
            dustAmount = 2;
            swingDust = DustID.Blood;
        }

        public static void StandartWhipAIMotion(Projectile Projectile, ref float Timer, ref bool reset, ref List<Vector2> whipPoints, ref Projectile spawner)
        {
            Player obj = Main.player[Projectile.owner];
            Projectile.rotation = Utils.ToRotation(Projectile.velocity) + (float)Math.PI / 2f;
            Projectile.Center = Vector2.Lerp(Projectile.Center, whipPoints[whipPoints.Count - 1], 1f);
            Projectile.spriteDirection = ((Projectile.velocity.X >= 0f) ? 1 : (-1));
            Timer++;
            float swingTime = obj.itemAnimationMax * Projectile.MaxUpdates;
            if (!(Timer >= swingTime))
            {
                return;
            }
            if (reset)
            {
                Timer = 4f;
                reset = false;
                if (spawner != null)
                {
                    Projectile.velocity = spawner.velocity;
                }
            }
            else
            {
                Projectile.Kill();
            }
        }

        public override void WhipAIMotion()
        {
            TriInfectaSmallProj.StandartWhipAIMotion(Projectile, ref Timer, ref reset, ref whipPoints, ref spawner);

            /*Player obj = Main.player[Projectile.owner];
            Projectile.rotation = Utils.ToRotation(Projectile.velocity) + (float)Math.PI / 2f;
            Projectile.Center = Vector2.Lerp(Projectile.Center, whipPoints[whipPoints.Count - 1], 1f);
            Projectile.spriteDirection = ((Projectile.velocity.X >= 0f) ? 1 : (-1));
            Timer++;
            float swingTime = obj.itemAnimationMax * Projectile.MaxUpdates;
            if (!(Timer >= swingTime))
            {
                return;
            }
            if (reset)
            {
                Timer = 4f;
                reset = false;
                if (spawner != null)
                {
                    Projectile.velocity = spawner.velocity;
                }
            }
            else
            {
                Projectile.Kill();
            }*/
        }

        public override void WhipTipParticles(Vector2 tipCoord, Color lightingCol, int dustID, int dustNum)
        {
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPosition = tipCoord + Main.rand.NextVector2Circular(Projectile.width / 2f, Projectile.height / 2f);
                HeavySmokeParticle smoke = new HeavySmokeParticle(
                    trailPosition,
                    Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    Color.Lerp(Color.Red, Color.Crimson, Main.rand.NextFloat()),
                    Main.rand.Next(100, 200),
                    Main.rand.NextFloat(0.1f, 0.2f),
                    0.4f
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }
    }

    public class TriInfectaMedProj : BaseWhipProjectile
    {
        public Projectile spawner;
        private bool reset = true;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetWhipStats()
        {
            whipSegment2 = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Segment2");
            Projectile.localNPCHitCooldown = -1;
            Projectile.WhipSettings.Segments = 15;
            Projectile.WhipSettings.RangeMultiplier = 0.35f;

            segmentRotation = -MathF.PI / 2;
            fishingLineColor = Color.DarkGreen;
            dustAmount = 4;
            swingDust = DustID.Blood;
        }

        public override void WhipAIMotion()
        {
            TriInfectaSmallProj.StandartWhipAIMotion(Projectile, ref Timer, ref reset, ref whipPoints, ref spawner);
        }

        public override void WhipTipParticles(Vector2 tipCoord, Color lightingCol, int dustID, int dustNum)
        {
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPosition = tipCoord + Main.rand.NextVector2Circular(Projectile.width / 2f, Projectile.height / 2f);
                HeavySmokeParticle smoke = new HeavySmokeParticle(
                    trailPosition,
                    Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    Color.Lerp(Color.Red, Color.Orange, Main.rand.NextFloat()),
                    Main.rand.Next(100, 200),
                    Main.rand.NextFloat(0.1f, 0.2f),
                    0.6f
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }
    }

    public class TriInfectaLargeProj : BaseWhipProjectile
    {
        public Projectile spawner;
        private bool reset = true;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetWhipStats()
        {
            whipSegment2 = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Segment2");
            Projectile.localNPCHitCooldown = -1;
            Projectile.WhipSettings.Segments = 15;
            Projectile.WhipSettings.RangeMultiplier = 0.3f;

            segmentRotation = -MathF.PI / 2;
            fishingLineColor = Color.Red;
            tagDebuff = ModContent.BuffType<TriInfectaDebuff>();
            dustAmount = 6;
            swingDust = DustID.Blood;
        }

        public override void WhipAIMotion()
        {
            TriInfectaSmallProj.StandartWhipAIMotion(Projectile, ref Timer, ref reset, ref whipPoints, ref spawner);
        }

        public override void WhipOnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<BurningBlood>(), 300);

            if (Main.rand.NextFloat() <= 0.35f)
                for (int i = 0; i < 5; i++)
                {
                    HeavySmokeParticle smoke = new HeavySmokeParticle(
                        target.Center + Main.rand.NextVector2Circular(15, 15),
                        Main.rand.NextVector2Circular(1f, 1f),
                        Color.Crimson,
                        Main.rand.Next(150, 255), // opacity
                        Main.rand.NextFloat(0.2f, 0.3f), // size
                        0.6f
                    );
                    GeneralParticleHandler.SpawnParticle(smoke);
                }
        }

        public override void WhipTipParticles(Vector2 tipCoord, Color lightingCol, int dustID, int dustNum)
        {
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPosition = tipCoord + Main.rand.NextVector2Circular(Projectile.width / 2f, Projectile.height / 2f);
                HeavySmokeParticle smoke = new HeavySmokeParticle(
                    trailPosition,
                    Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    Color.Lerp(Color.Orange, Color.Red, Main.rand.NextFloat()),
                    Main.rand.Next(100, 200), // opacity
                    Main.rand.NextFloat(0.1f, 0.2f), // size
                    0.8f // length
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }
    }

    public class TriInfectaDebuff : ModBuff, ILocalizedModType
    {
        public new string LocalizationCategory => "Buffs.Whips";
        public override string Texture => Clamity.buffPath;

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            BuffID.Sets.IsATagBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<GlobalTagDebuffs>().activeTag = Type;
            npc.GetGlobalNPC<GlobalTagDebuffs>().triIndex = buffIndex;
        }
    }
}