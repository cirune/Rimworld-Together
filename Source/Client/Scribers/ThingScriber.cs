using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Shared;
using Verse;

namespace GameClient
{
    public static class ThingScriber
    {
        public static ThingFile ThingToString(Thing thing, int thingCount)
        {
            ThingFile thingData = new ThingFile();

            thingData.ID = thing.ThingID;

            thingData.ScribeData = RTScriber.ThingToScribe(thing, thingCount);

            return thingData;
        }

        public static Thing StringToThing(ThingFile thingData, bool overrideID = false)
        {
            return RTScriber.ScribeToThing(thingData.ScribeData, overrideID);
        }
    }
}