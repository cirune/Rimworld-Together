using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Shared;
using UnityEngine.Assertions.Must;
using Verse;

namespace GameClient
{
    public static class HumanScriber
    {
        public static HumanFile ToString(Pawn human)
        {
            HumanFile humanFile = new HumanFile();

            humanFile.ID = human.ThingID;

            humanFile.ScribeData = RTScriber.ThingToScribe(human);

            return humanFile;
        }

        public static Pawn FromString(HumanFile file, bool overrideID = false)
        {
            return (Pawn)RTScriber.ScribeToThing(file.ScribeData, overrideID);
        }
    }
}