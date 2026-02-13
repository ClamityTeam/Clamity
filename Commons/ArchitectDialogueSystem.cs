using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Clamity.Commons
{
    public enum ArchitectDialoguePhase : short
    {
        None = -1,
        Abyss = 1,
        PostWoB = 2,
    }
    public class ArchitectDialogueSystem : ModSystem
    {
        public static bool GottaGoFast = false;
        public static int GottaGoFastSpeed = 5;

        public static ArchitectDialoguePhase Phase = ArchitectDialoguePhase.None;
        private static ArchitectDialogueEvent[] currentSequence = null;
        public static int currentSequenceIndex = 0;

        public static int CurrentDialogueDelay = 0;

        internal struct ArchitectDialogueEvent
        {
            private const int DefaultFrameDelay = 180;

            internal int FrameDelay;
            internal string LocalizationKey;
            internal Func<bool> skipCondition;

            public ArchitectDialogueEvent()
            {
                FrameDelay = DefaultFrameDelay;
                LocalizationKey = null;
                skipCondition = null;
            }
            public ArchitectDialogueEvent(string key)
            {
                LocalizationKey = key;
                FrameDelay = DefaultFrameDelay;
                skipCondition = null;
            }
            public ArchitectDialogueEvent(string key, int delay = DefaultFrameDelay, Func<bool> skipFunc = null)
            {
                LocalizationKey = key;
                FrameDelay = delay;
                skipCondition = skipFunc;
            }

            public readonly bool ShouldDisplay()
            {
                if (skipCondition is null)
                    return true;
                return !skipCondition.Invoke();
            }
        }

        internal static Dictionary<ArchitectDialoguePhase, ArchitectDialogueEvent[]> ArchitectDialogue;

        public override void Load()
        {
            ArchitectDialogueEvent[] abyss = new ArchitectDialogueEvent[]
            {
                new ("Mods.Clamity.Dialogues.Architect.Abyss.Text1"),
                new ("Mods.Clamity.Dialogues.Architect.Abyss.Text2"),
                new ("Mods.Clamity.Dialogues.Architect.Abyss.Text3"),
                new ("Mods.Clamity.Dialogues.Architect.Abyss.Text4"),
            };
            ArchitectDialogueEvent[] postwob = new ArchitectDialogueEvent[]
            {
                new ("Mods.Clamity.Dialogues.Architect.PostWoB.Text1"),
                new ("Mods.Clamity.Dialogues.Architect.PostWoB.Text2"),
                new ("Mods.Clamity.Dialogues.Architect.PostWoB.Text3"),
                new ("Mods.Clamity.Dialogues.Architect.PostWoB.Text4"),
            };
            ArchitectDialogue = new Dictionary<ArchitectDialoguePhase, ArchitectDialogueEvent[]> {
                { ArchitectDialoguePhase.Abyss, abyss },
                { ArchitectDialoguePhase.PostWoB, postwob },
            };
        }
        public override void Unload()
        {
            ArchitectDialogue = null;
        }

        public static void StartDialogue(ArchitectDialoguePhase phaseToRun)
        {
            Phase = phaseToRun;
            bool validDialogueFound = ArchitectDialogue.TryGetValue(Phase, out var dialogueListToUse);
            if (validDialogueFound)
            {
                currentSequence = dialogueListToUse;
                currentSequenceIndex = 0;
            }

            CurrentDialogueDelay = 4;
        }
        public override void PostUpdateWorld()
        {
            if (Main.LocalPlayer.Calamity().ZoneAbyssLayer4 && !ClamitySystem.dialogueArchitectAbyss)
            {
                StartDialogue(ArchitectDialoguePhase.Abyss);
                ClamitySystem.dialogueArchitectAbyss = true;
            }

            Tick();
        }
        internal static void Tick()
        {
            // If the phase isn't defined properly, don't do anything.
            if (Phase == ArchitectDialoguePhase.None)
                return;

            if (currentSequenceIndex < currentSequence.Length)
            {
                // If it's time to display dialogue, do so.
                if (CurrentDialogueDelay == 0 && currentSequenceIndex < currentSequence.Length)
                {
                    // Skip over all lines that should be skipped to find the first one that should not be skipped.
                    bool hasMoreDialogue = GetNextUnskippedDialogue(currentSequence, currentSequenceIndex, out int currentIndex);
                    if (hasMoreDialogue)
                    {
                        ArchitectDialogueEvent line = currentSequence[currentSequenceIndex];

                        // Display dialogue and set appropriate delay, if this dialogue shouldn't be skipped.
                        if (line.skipCondition is null || !line.skipCondition.Invoke())
                        {
                            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(Language.GetTextValue(line.LocalizationKey)), Color.Magenta);
                            CurrentDialogueDelay = line.FrameDelay;
                        }

                        // Move onto the next dialogue line.
                        currentSequenceIndex = currentIndex + 1;
                    }
                }
                // Otherwise, decrement the existing delay.
                else
                    --CurrentDialogueDelay;

                // Gotta Go Fast Mode
                if (GottaGoFast && CurrentDialogueDelay > GottaGoFastSpeed)
                    CurrentDialogueDelay = GottaGoFastSpeed;
            }
            else
            {
                CurrentDialogueDelay = 0;
            }

            // If the end of a sequence has been reached, stay in this state indefinitely.
            // Allow the boss spawn countdown to hit zero and the next boss to appear without showing any dialogue or causing any delays.

            // However, if Boss Rush is not occurring, reset all variables.
            /*if (!BossRushEvent.BossRushActive)
            {
                Phase = ArchitectDialoguePhase.None;
                currentSequence = null;
                currentSequenceIndex = 0;
                CurrentDialogueDelay = 0;
            }*/
        }

        private static bool GetNextUnskippedDialogue(ArchitectDialogueEvent[] sequence, int index, out int newIndex)
        {
            int tryIndex = index;
            while (tryIndex < sequence.Length)
            {
                ArchitectDialogueEvent lineToTry = currentSequence[tryIndex];
                if (lineToTry.skipCondition is not null && lineToTry.skipCondition.Invoke())
                {
                    ++tryIndex;
                    continue;
                }

                newIndex = tryIndex;
                return true;
            }

            newIndex = -1;
            return false;
        }
    }
}
