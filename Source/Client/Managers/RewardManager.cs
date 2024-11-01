using RimWorld;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class RewardManager
    {
        public static void ParsePacket(Packet packet)
        {
            RewardData siteData = Serializer.ConvertBytesToObject<RewardData>(packet.contents);
            ReceiveRewards(siteData);
        }

        private static void ReceiveRewards(RewardData siteData)
        {
            List<Thing> rewards = new List<Thing>();
            foreach (RewardFile reward in siteData._rewardData)
            {
                try
                {
                    ThingDef def = DefDatabase<ThingDef>.AllDefs.First(fetch => fetch.defName == reward.RewardDef);
                    Thing toMake = ThingMaker.MakeThing(def);
                    toMake.stackCount = reward.RewardAmount;
                    toMake.HitPoints = def.BaseMaxHitPoints;
                    rewards.Add(toMake);

                    Logger.Message($"Received {reward.RewardAmount} of {reward.RewardDef}", LogImportanceMode.Verbose);
                }
                catch (Exception e) { Logger.Warning(e.ToString(), LogImportanceMode.Verbose); }
            }

            if (rewards.Count > 0)
            {
                TransferManager.GetTransferedItemsToSettlement(rewards.ToArray(), true, false, false);
                RimworldManager.GenerateLetter("Site rewards", $"You've received your site rewards", LetterDefOf.PositiveEvent);
                Logger.Message("Rewards delivered", LogImportanceMode.Verbose);
            }
        }
    }
}
