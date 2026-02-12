using CalamityMod.Items.Placeables;
using CalamityMod.Items.Placeables.Astral;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Clamity
{
    public class ClamitySystem : ModSystem
    {
        public Dictionary<int, List<int>> enchantebleAccessories;

        internal static bool _downedClamitas;
        public static bool downedClamitas
        {
            get
            {
                return _downedClamitas;
            }
            set
            {
                if (!value)
                {
                    _downedClamitas = false;
                }
                else
                {
                    NPC.SetEventFlagCleared(ref _downedClamitas, -1);
                }
            }
        }
        internal static bool _downedPyrogen;
        public static bool downedPyrogen
        {
            get
            {
                return _downedPyrogen;
            }
            set
            {
                if (!value)
                {
                    _downedPyrogen = false;
                }
                else
                {
                    NPC.SetEventFlagCleared(ref _downedPyrogen, -1);
                }
            }
        }
        internal static bool _generatedFrozenHell;
        public static bool generatedFrozenHell
        {
            get
            {
                return _generatedFrozenHell;
            }
            set
            {
                if (!value)
                {
                    _generatedFrozenHell = false;
                }
                else
                {
                    NPC.SetEventFlagCleared(ref _generatedFrozenHell, -1);
                }
            }
        }
        internal static bool _downedWallOfBronze;
        public static bool downedWallOfBronze
        {
            get
            {
                return _downedWallOfBronze;
            }
            set
            {
                if (!value)
                {
                    _downedWallOfBronze = false;
                }
                else
                {
                    NPC.SetEventFlagCleared(ref _downedWallOfBronze, -1);
                }
            }
        }
        internal static bool _dialogueArchitectAbyss;
        public static bool dialogueArchitectAbyss
        {
            get
            {
                return _dialogueArchitectAbyss;
            }
            set
            {
                if (!value)
                {
                    _dialogueArchitectAbyss = false;
                }
                else
                {
                    NPC.SetEventFlagCleared(ref _dialogueArchitectAbyss, -1);
                }
            }
        }
        internal static bool _dialogueArchitectPostWoB;
        public static bool dialogueArchitectPostWoB
        {
            get
            {
                return _dialogueArchitectPostWoB;
            }
            set
            {
                if (!value)
                {
                    _dialogueArchitectPostWoB = false;
                }
                else
                {
                    NPC.SetEventFlagCleared(ref _dialogueArchitectPostWoB, -1);
                }
            }
        }
        internal static void ResetAllFlags()
        {
            downedClamitas = false;
            downedPyrogen = false;
            downedWallOfBronze = false;
            generatedFrozenHell = false;

            dialogueArchitectAbyss = false;
            dialogueArchitectPostWoB = false;
        }
        public override void Load()
        {
            enchantebleAccessories = new();
        }
        public override void Unload()
        {
            enchantebleAccessories = null;
        }
        private void AddEnchantebleAccessories(int acc, params int[] projList)
        {
            foreach (int i in projList)
            {
                enchantebleAccessories.Add(acc, projList.ToList<int>());
            }
        }
        public override void OnWorldLoad()
        {
            ResetAllFlags();
        }
        public override void OnWorldUnload()
        {
            ResetAllFlags();
        }
        public override void SaveWorldData(TagCompound tag)
        {
            List<string> list = new List<string>();
            if (downedClamitas)
                list.Add("clamitas");
            if (downedPyrogen)
                list.Add("pyrogen");
            if (downedWallOfBronze)
                list.Add("wob");
            tag["downedFlagsClamity"] = list;
            tag["generatedFrozenHell"] = generatedFrozenHell;

            list = new List<string>();
            if (dialogueArchitectAbyss)
                list.Add("abyss");
            if (dialogueArchitectPostWoB)
                list.Add("wob");
            tag["Clamity:Dialogues"] = list;
        }
        public override void LoadWorldData(TagCompound tag)
        {
            IList<string> list = tag.GetList<string>("downedFlagsClamity");
            downedClamitas = list.Contains("clamitas");
            downedPyrogen = list.Contains("pyrogen");
            downedWallOfBronze = list.Contains("wob");
            generatedFrozenHell = tag.GetBool("generatedFrozenHell");

            list = tag.GetList<string>("Clamity:Dialogues");
            dialogueArchitectAbyss = list.Contains("abyss");
            dialogueArchitectPostWoB = list.Contains("wob");
        }
        public static int AnySandBlock;
        public static int AnyGemHook;
        public override void AddRecipeGroups()
        {
            ClamitySystem.AnySandBlock = RecipeGroup.RegisterGroup("AnySandBlock", new RecipeGroup((Func<string>)(() => LangHelper.GetText("Misc.RecipeGroup.AnySandBlock")), new int[5]
            {
                ItemID.SandBlock, ItemID.EbonsandBlock, ItemID.PearlsandBlock, ItemID.CrimsandBlock, ModContent.ItemType<AstralSand>()
            }));
            ClamitySystem.AnyGemHook = RecipeGroup.RegisterGroup("AnyGemHook", new RecipeGroup((Func<string>)(() => LangHelper.GetText("Misc.RecipeGroup.AnyGemHook")), new int[7]
            {
                ItemID.AmethystHook, ItemID.TopazHook, ItemID.SapphireHook, ItemID.EmeraldHook, ItemID.RubyHook, ItemID.AmberHook, ItemID.DiamondHook
            }));
        }
        public override void NetSend(BinaryWriter writer)
        {
            BitsByte flags = new BitsByte();
            flags[0] = downedClamitas;
            flags[1] = downedPyrogen;
            flags[2] = downedWallOfBronze;

            writer.Write(flags);

            writer.Write(generatedFrozenHell);

            flags = new BitsByte();
            flags[0] = dialogueArchitectAbyss;
            flags[1] = dialogueArchitectPostWoB;
            writer.Write(flags);
        }
        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            downedClamitas = flags[0];
            downedPyrogen = flags[1];
            downedWallOfBronze = flags[2];

            generatedFrozenHell = reader.ReadBoolean();

            flags = reader.ReadByte();
            dialogueArchitectAbyss = flags[0];
            dialogueArchitectPostWoB = flags[1];
        }
    }
}
