using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Cooldowns;
using CalamityMod.Items.Accessories;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Systems.Collections;
using Clamity.Content.Biomes.FrozenHell.Biome;
using Clamity.Content.Bosses.Pyrogen.Drop;
using Clamity.Content.Bosses.Pyrogen.NPCs;
using Clamity.Content.Cooldowns;
using Clamity.Content.Items.Accessories;
using Clamity.Content.Items.Accessories.GemCrawlerDrop;
using Clamity.Content.Items.Materials;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Events;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Clamity
{
    public class ClamityPlayer : ModPlayer
    {
        #region Variables
        public bool realityRelocator;
        public bool wulfrumShortstrike;
        public bool aflameAcc;
        public List<int> aflameAccList;

        //Accessories
        //Other
        public bool pyroSpear;
        //public int pyroSpearCD;
        public bool vampireEX;
        public bool pyroStone;
        public bool pyroStoneVanity;
        public bool hellFlare;
        public bool icicleRing;
        public bool redDie;
        public bool eidolonAmulet;
        public bool metalWings;
        public bool seaShell;
        public bool subcommunity;
        public bool skullOfBloodGod;

        //Crawler big gems
        public bool gemAmethyst;
        public int gemAmethystCooldown;
        public bool gemTopaz;
        public bool gemSapphire;
        public bool gemEmerald;
        public bool gemRuby;
        public bool gemDiamond;
        public int gemDiamondBarrier;
        public int gemDiamondCooldown;
        public bool gemAmber;
        public bool gemFinal;

        //Sentry
        public bool brimScope;
        public bool cyanPearl;

        //Armor
        public bool inflicingMeleeFrostburn;
        public bool frozenParrying;
        public int frozenParryingTime;
        public bool shellfishSetBonus;
        public int shellfishSetBonusProj = -1;

        //Minion
        public bool hellsBell;
        public bool guntera;

        //Buffs-Debuffs
        public bool titanScale;
        public int titanScaleTimer;

        //Pets

        //Mounts
        public bool FlyingChair;
        public int FlyingChairPower;

        public bool ZoneFrozenHell => Player.InModBiome((ModBiome)ModContent.GetInstance<FrozenHell>());
        public override void ResetEffects()
        {
            realityRelocator = false;
            wulfrumShortstrike = false;
            aflameAcc = false;
            aflameAccList = new List<int>();

            pyroSpear = false;
            vampireEX = false;
            pyroStone = false;
            pyroStoneVanity = false;
            hellFlare = false;
            icicleRing = false;
            redDie = false;
            eidolonAmulet = false;
            metalWings = false;
            seaShell = false;
            subcommunity = false;
            skullOfBloodGod = false;

            gemAmethyst = false;
            gemTopaz = false;
            gemSapphire = false;
            gemEmerald = false;
            gemRuby = false;
            gemDiamond = false;
            gemAmber = false;
            gemFinal = false;

            brimScope = false;
            cyanPearl = false;


            inflicingMeleeFrostburn = false;
            frozenParrying = false;
            shellfishSetBonus = false;

            hellsBell = false;
            guntera = false;

            FlyingChair = false;
            FlyingChairPower = 12;
        }
        public override void UpdateDead()
        {
            frozenParryingTime = 0;
        }
        #endregion

        #region On Hit Effect
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.Clamity().IncreasedHeatEffects_PyroStone = pyroStone;

            if (item.DamageType == DamageClass.Melee || item.DamageType == ModContent.GetInstance<TrueMeleeDamageClass>())
            {
                if (pyroSpear && !Player.HasCooldown(PyrospearCooldown.ID))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 vec1 = Vector2.UnitY.RotatedByRandom(1f);
                        Projectile.NewProjectile(item.GetSource_OnHit(target), target.Center + vec1 * 500f, -vec1.RotatedByRandom(0.1f) * 20f, ModContent.ProjectileType<SoulOfPyrogenSpear>(), item.damage / 2, 1f, Player.whoAmI, target.whoAmI);
                    }
                    Player.AddCooldown(PyrospearCooldown.ID, 100);
                }
                if (inflicingMeleeFrostburn)
                    target.AddBuff(BuffID.Frostburn, 180);
                if (titanScale)
                    titanScaleTimer = 10 * 60;
            }
            if (hellFlare)
                CalamityUtils.Inflict246DebuffsNPC(target, BuffID.OnFire3);

        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.DamageType == ModContent.GetInstance<TrueMeleeDamageClass>())
            {
                PyroSpearEffect(proj, target);
                if (inflicingMeleeFrostburn)
                    target.AddBuff(BuffID.Frostburn, 180);
                if (titanScale)
                    titanScaleTimer = 10 * 60;
            }
            if (proj.DamageType is MagicDamageClass)
            {
                if (gemDiamondCooldown == 0)
                {
                    gemDiamondBarrier += gemFinal ? 3 : 1;
                    gemDiamondCooldown = 30;
                }
                else
                    gemDiamondCooldown = (int)(gemDiamondCooldown * 0.9f);
            }
            if (proj.Calamity().stealthStrike)
            {
                PyroSpearEffect(proj, target);
            }
            if (proj.Clamity().IsSentryRelated)
            {
                if (brimScope)
                {
                    target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 60);
                }
            }
        }
        private void PyroSpearEffect(Projectile proj, NPC target)
        {
            if (pyroSpear && !Player.HasCooldown(PyrospearCooldown.ID))
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 vec1 = Vector2.UnitY.RotatedByRandom(1f);
                    Projectile.NewProjectile(proj.GetSource_OnHit(target), target.Center + vec1 * 500f, -vec1.RotatedByRandom(0.1f) * 20f, ModContent.ProjectileType<SoulOfPyrogenSpear>(), proj.damage / 2, 1f, Player.whoAmI, target.whoAmI);
                }
                Player.AddCooldown(PyrospearCooldown.ID, 100);
            }
        }
        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            //Copyed Calamity's Summon penalty for compatibilty of changes to it
            bool isSummon = proj.CountsAsClass<SummonDamageClass>();
            if (isSummon)
            {
                Item heldItem = Player.HeldItem;

                bool wearingForbiddenSet = Player.armor[0].type == ItemID.AncientBattleArmorHat && Player.armor[1].type == ItemID.AncientBattleArmorShirt && Player.armor[2].type == ItemID.AncientBattleArmorPants;

                bool forbiddenWithMagicWeapon = wearingForbiddenSet && heldItem.CountsAsClass<MagicDamageClass>();
                bool gemTechBlueGem = Player.Calamity().GemTechSet && Player.Calamity().GemTechState.IsBlueGemActive;

                bool crossClassNerfDisabled = forbiddenWithMagicWeapon || Player.Calamity().fearmongerSet || gemTechBlueGem || Player.Calamity().profanedCrystalBuffs || DD2Event.Ongoing;
                crossClassNerfDisabled |= CalamityProjectileSets.MinionWhichIgnoresSummonerNerf[proj.type];

                // If this projectile is a summon, its owner is holding an item, and the cross class nerf isn't disabled from equipment:
                if (isSummon && heldItem.type > ItemID.None && !crossClassNerfDisabled)
                {
                    bool heldItemIsClassedWeapon = !heldItem.CountsAsClass<SummonDamageClass>() && (
                        heldItem.CountsAsClass<MeleeDamageClass>() ||
                        heldItem.CountsAsClass<RangedDamageClass>() ||
                        heldItem.CountsAsClass<MagicDamageClass>() ||
                        heldItem.CountsAsClass<ThrowingDamageClass>()
                    );

                    bool heldItemIsTool = heldItem.pick > 0 || heldItem.axe > 0 || heldItem.hammer > 0;
                    bool heldItemCanBeUsed = heldItem.useStyle != ItemUseStyleID.None;
                    bool heldItemIsAccessoryOrAmmo = heldItem.accessory || heldItem.ammo != AmmoID.None;
                    bool heldItemIsExcludedByModCall = CalamityProjectileSets.MinionWhichIgnoresSummonerNerf[heldItem.type];

                    if (heldItemIsClassedWeapon && heldItemCanBeUsed && !heldItemIsTool && !heldItemIsAccessoryOrAmmo && !heldItemIsExcludedByModCall)
                    {
                        float basePenalty = 0.75f;
                        float reversedPenalty = 1 / basePenalty;
                        //modifiers.FinalDamage *= BalancingConstants.SummonerCrossClassNerf;
                        if (cyanPearl && proj.Clamity().IsSentryRelated)
                        {
                            modifiers.FinalDamage *= (1 + reversedPenalty) / 2;
                        }
                    }
                }
            }
        }
        #endregion

        #region Hurt Effect
        public override void OnHurt(Player.HurtInfo info)
        {
            if (metalWings)
            {
                float percent = info.Damage / (float)Player.statLifeMax2;
                float recivingFlyTime = Player.wingTimeMax * percent / 2;
                if (Player.wingTime + recivingFlyTime > Player.wingTimeMax)
                    Player.wingTime = Player.wingTimeMax;
                else
                    Player.wingTime += recivingFlyTime;
            }
        }
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (frozenParrying && frozenParryingTime > 15)
            {
                if (!Player.HasCooldown(ParryCooldown.ID))
                {
                    Player.GiveUniversalIFrames(Player.ComputeParryIFrames(), true);
                    modifiers.SetMaxDamage(1);
                    modifiers.DisableSound();
                }
                SoundEngine.PlaySound(in PyrogenShield.BreakSound, new Vector2?(Player.Center));
                Player.AddCooldown(ParryCooldown.ID, 10 * 60, false);
                Player.AddBuff(BuffID.Frozen, 60);
            }
        }
        #endregion

        #region Updates
        public override void UpdateEquips()
        {
            foreach (Item i in Player.armor)
            {
                if (!i.IsAir)
                    if (i.Calamity().AppliedEnchantment != null)
                    {
                        if (i.Calamity().AppliedEnchantment.Value.ID == 10000)
                            aflameAccList.Add(i.type);
                    }
            }
            if (aflameAccList.Count > 0)
            {
                Player.AddBuff(ModContent.BuffType<WeakBrimstoneFlames>(), 1);
            }
        }
        public override void PostUpdateEquips()
        {
            if (titanScaleTimer > 0)
                titanScaleTimer--;
            int maxGemBarrier = gemFinal ? 120 : gemDiamond ? 20 : 0;
            if (gemDiamondBarrier > maxGemBarrier)
                gemDiamondBarrier = maxGemBarrier;
            if (gemDiamondCooldown > 0)
                gemDiamondCooldown--;
        }
        public override void PostUpdateMiscEffects()
        {
            var cooldownList = Player.GetDisplayedCooldowns();
            bool flagSurface = Player.Center.Y < Main.worldSurface * 16f;
            bool flagWet = Main.raining & flagSurface || Player.dripping || Player.wet && !Player.lavaWet && !Player.honeyWet;

            StatModifier statModifier;
            if (pyroStone || pyroStoneVanity)
            {
                //Main.NewText("ClamityPlayer messenge: " + pyroStone.ToString() + " " + pyroStoneVanity.ToString());
                IEntitySource sourceAccessory = Player.GetSource_Accessory(FindAccessory(ModContent.ItemType<PyroStone>()));
                statModifier = Player.GetBestClassDamage();
                int damage = (int)statModifier.ApplyTo(70f);
                if (Player.whoAmI == Main.myPlayer && Player.ownedProjectileCounts[ModContent.ProjectileType<PyroShieldAccessory>()] == 0)
                    Projectile.NewProjectile(sourceAccessory, Player.Center, Vector2.Zero, ModContent.ProjectileType<PyroShieldAccessory>(), damage, 0.0f, Player.whoAmI);
            }
            if (hellFlare)
            {
                if (Player.statLife > (int)(Player.statLifeMax2 * 0.75))
                {
                    Player.GetCritChance<GenericDamageClass>() += 10;
                }
                if (Player.statLife < (int)(Player.statLifeMax2 * 0.25))
                {
                    Player.endurance += 0.1f;
                }
            }
            if (eidolonAmulet)
            {
                if (Player.Calamity().oceanCrestTimer > 0 | flagWet)
                    Player.GetDamage<GenericDamageClass>() += 0.1f;
            }
            if (seaShell && flagWet)
            {
                Player.endurance += 0.05f;
                Player.statDefense += 3;
                Player.moveSpeed += 0.15f;
            }
            if (frozenParrying && frozenParryingTime > 0)
                frozenParryingTime--;
            if (frozenParrying && (Player.HasCooldown(ParryCooldown.ID) || frozenParryingTime > 0))
            {
                Player.buffImmune[47] = false;
                /*for (int i = 0; i < cooldownList.Count; i++)
                {
                    CooldownInstance cooldown = cooldownList[i];

                    if (cooldown.duration >= 9 * 60)
                    {

                    }
                }
                //Player.buffImmune[47] = false;
                Player.controlJump = false;
                Player.controlDown = false;
                Player.controlLeft = false;
                Player.controlRight = false;
                Player.controlUp = false;
                Player.controlUseItem = false;
                Player.controlUseTile = false;
                Player.controlThrow = false;
                Player.gravDir = 1f;
                Player.velocity = Vector2.Zero;
                Player.velocity.Y = -0.1f;
                Player.RemoveAllGrapplingHooks();*/
            }
            if (subcommunity)
            {
                float baseBoost = TheSubcommunity.CalculatePower();
                Player.pickSpeed += baseBoost * TheSubcommunity.MiningSpeedMult;
                Player.luck += baseBoost * TheSubcommunity.LuckMult;
                Player.fishingSkill += (int)(baseBoost * TheSubcommunity.FishingPower);
                Player.tileSpeed += baseBoost * TheSubcommunity.TileAndWallPlacingSpeedMult;
                Player.wallSpeed += baseBoost * TheSubcommunity.TileAndWallPlacingSpeedMult;
                Player.tileRangeX += (int)(baseBoost * TheSubcommunity.TileRangeMult);
                Player.tileRangeY += (int)(baseBoost * TheSubcommunity.TileRangeMult);
            }
        }
        public Item FindAccessory(int itemID)
        {
            for (int index = 0; index < 10; ++index)
            {
                if (this.Player.armor[index].type == itemID)
                    return this.Player.armor[index];
            }
            return new Item();
        }
        public override void PostUpdate()
        {
            // Remove set bonus minion if bonus is not active
            if (!shellfishSetBonus && shellfishSetBonusProj != -1)
            {
                Projectile proj = Main.projectile[shellfishSetBonusProj];
                if (proj.active)
                    proj.Kill();

                shellfishSetBonusProj = -1;
            }
        }
        #endregion

        #region Modify Stats
        public override void UpdateBadLifeRegen()
        {
            if (icicleRing && Player.statLife > Player.statLifeMax2 / 3)
            {
                if (Player.lifeRegen > 0)
                    Player.lifeRegen = 0;
                Player.lifeRegen -= 30;
            }
        }
        public override void PreUpdateMovement()
        {
            if (Player.whoAmI != Main.myPlayer || !FlyingChair)
                return;
            if (Player.controlLeft)
            {
                Player.velocity.X = -FlyingChairPower;
                Player.ChangeDir(-1);
            }
            else if (this.Player.controlRight)
            {
                Player.velocity.X = FlyingChairPower;
                Player.ChangeDir(1);
            }
            else
                Player.velocity.X = 0.0f;
            if (Player.controlUp || Player.controlJump)
                Player.velocity.Y = -FlyingChairPower;
            else if (Player.controlDown)
            {
                Player.velocity.Y = FlyingChairPower;
                if (Collision.TileCollision(Player.position, Player.velocity, Player.width, Player.height, true, gravDir: (int)this.Player.gravDir).Y == 0)
                    Player.velocity.Y = 0.5f;
            }
            else
                Player.velocity.Y = 0.0f;
            if (CalamityKeybinds.ExoChairSlowdownHotkey.Current)
            {
                Player.velocity = Player.velocity / 2;
            }
        }
        public override void ModifyLuck(ref float luck)
        {
            if (redDie)
            {
                for (int i = 3; i < 9; i++)
                {
                    Item item = Player.armor[i];
                    if (item.type == ModContent.ItemType<OldDie>())
                    {
                        luck -= 0.2f;
                    }
                    luck *= 1.5f;
                    luck += 0.2f;
                }
            }
        }
        #endregion

        #region Other
        public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            if (eidolonAmulet 
                //&& Player.Calamity().RustyMedallionCooldown <= 0
                )
            {
                /* TODO: rework this
                int d = (int)Player.GetTotalDamage<AverageDamageClass>().ApplyTo(damage / 5);
                //int d = damage / 5;
                d = Player.ApplyArmorAccDamageBonusesTo(d);

                Vector2 startingPosition = Main.MouseWorld - Vector2.UnitY.RotatedByRandom(0.4f) * 1250f;
                Vector2 directionToMouse = (Main.MouseWorld - startingPosition).SafeNormalize(Vector2.UnitY).RotatedByRandom(0.1f);
                int drop = Projectile.NewProjectile(source, startingPosition, directionToMouse * 15f, ModContent.ProjectileType<AcidBarrelDrop>(), d, 0f, Player.whoAmI);
                if (drop.WithinBounds(Main.maxProjectiles))
                {
                    Main.projectile[drop].penetrate = 3;
                    Main.projectile[drop].DamageType = DamageClass.Generic;
                }
                Player.Calamity().RustyMedallionCooldown = RustyMedallion.AcidCreationCooldown / 2;
                */
            }
            if (gemFinal)
            {
                WeightedRandom<Action<EntitySource_ItemUse_WithAmmo, Player, Vector2, Vector2, int, float>> rand =
                    new Terraria.Utilities.WeightedRandom<Action<EntitySource_ItemUse_WithAmmo, Player, Vector2, Vector2, int, float>>((int)Main.GlobalTimeWrappedHourly);
                if (gemAmethystCooldown <= 0)
                    rand.Add((source, player, center, velocity, damage, kb) =>
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                float d = player.GetTotalDamage<RangedDamageClass>().ApplyTo(damage / 5);
                                Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.1f), ModContent.ProjectileType<SharpAmethystProj>(), (int)d, kb, player.whoAmI);
                            }
                            player.Clamity().gemAmethystCooldown = 30;
                        }, item.DamageType is RangedDamageClass ? 2f : 1f);
                rand.Add((source, proj, center, velocity, damage, kb) =>
                    {

                    }, item.DamageType is MeleeDamageClass ? 2f : 1f);
                rand.Add((source, proj, center, velocity, damage, kb) =>
                    {

                    }, item.DamageType is MagicDamageClass ? 2f : 1f);
                rand.Get().Invoke(source, Player, position, velocity, damage, knockback);
            }
            return true;
        }
        public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
        {
            bool flag = !attempt.inHoney && !attempt.inLava;
            if (flag)
            {
                //if (Player.ZoneDesert && Main.hardMode && attempt.uncommon && Main.rand.NextBool(7))
                //    itemDrop = ModContent.ItemType<FishOfFlame>();
                if (Player.Calamity().ZoneSunkenSea && Main.rand.NextBool(5))
                    itemDrop = ModContent.ItemType<CoralskinFoolfish>();
            }
        }
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (Player.dead)
            {
                return;
            }
            if (CalamityKeybinds.NormalityRelocatorHotKey.JustPressed && realityRelocator && Main.myPlayer == Player.whoAmI && !Player.CCed)
            {
                Vector2 vector;
                vector.X = Main.mouseX + Main.screenPosition.X;
                if (Player.gravDir == 1f)
                {
                    vector.Y = (float)Main.mouseY + Main.screenPosition.Y - (float)base.Player.height;
                }
                else
                {
                    vector.Y = Main.screenPosition.Y + Main.screenHeight - Main.mouseY;
                }

                vector.X -= Player.width / 2;
                if (vector.X > 50f && vector.X < (Main.maxTilesX * 16 - 50) && vector.Y > 50f && vector.Y < (Main.maxTilesY * 16 - 50) && !Collision.SolidCollision(vector, Player.width, Player.height))
                {
                    Player.Teleport(vector, 4);
                    NetMessage.SendData(MessageID.TeleportPlayerThroughPortal, -1, -1, null, 0, Player.whoAmI, vector.X, vector.Y, 1);
                }
            }
            if (CalamityKeybinds.AccessoryParryHotKey.JustPressed && !Player.HasCooldown(ParryCooldown.ID))
            {
                if (frozenParrying && frozenParryingTime == 0)
                {
                    Player.Calamity().GeneralScreenShakePower = 2.5f;
                    SoundEngine.PlaySound(in Cryogen.ShieldRegenSound, Player.Center);
                    frozenParryingTime = 30;
                }
            }

        }
        #endregion
    }
}
