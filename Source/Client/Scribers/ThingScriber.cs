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
        public static ThingFile ToString(Thing thing, int thingCount)
        {
            ThingFile thingData = new ThingFile();

            thingData.ID = thing.ThingID;

            thingData.ScribeData = RTScriber.ThingToScribe(thing, thingCount);

            return thingData;
        }

        public static Thing FromString(ThingFile thingData, bool overrideID = false)
        {
            return RTScriber.ScribeToThing(thingData.ScribeData, overrideID);
        }
    }
}