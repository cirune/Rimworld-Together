using System;
using System.Linq;
using RimWorld;
using Verse;

namespace GameClient
{
    public static class ScribeHelper
    {
        //Checks if transferable thing is a human

        public static bool CheckIfThingIsHuman(Thing thing)
        {
            try
            {
                if (thing.def.defName == "Human") return true;
                else return false;
            }
            catch { return false; }
        }

        //Checks if transferable thing is an animal

        public static bool CheckIfThingIsAnimal(Thing thing)
        {
            try
            {
                PawnKindDef animal = DefDatabase<PawnKindDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == thing.def.defName);
                if (animal != null) return true;
                else return false;
            }
            catch { return false; }
        }

        //Checks if transferable thing is an item that can have a growth state

        public static bool CheckIfThingCanGrow(Thing thing)
        {
            try
            {
                Plant plant = thing as Plant;
                _ = plant.Growth;
                return true;
            }

            // Don't log to avoid countless spam logs
            catch { return false; }
        }

        //Checks if transferable thing has a material

        public static bool CheckIfThingHasMaterial(Thing thing)
        {
            try
            {
                if (thing.Stuff != null) return true;
                else return false;
            }
            catch { return false; }
        }

        //Gets the quality of a transferable thing

        public static int GetThingQuality(Thing thing)
        {
            try
            {
                QualityCategory qc = QualityCategory.Normal;
                thing.TryGetQuality(out qc);

                return (int)qc;
            }
            catch { return 0; }
        }

        //Checks if transferable thing is minified

        public static bool CheckIfThingIsMinified(Thing thing)
        {
            try
            {
                if (thing.def == ThingDefOf.MinifiedThing || thing.def == ThingDefOf.MinifiedTree) return true;
                else return false;
            }
            catch { return false; }
        }

        public static bool CheckIfThingIsBook(Thing thing)
        {
            try
            {
                if (thing.def.defName == ThingDefOf.TextBook.defName) return true;
                else if (thing.def.defName == ThingDefOf.Schematic.defName) return true;
                else if (thing.def.defName == ThingDefOf.Novel.defName) return true;

                if (!ModsConfig.AnomalyActive) return false;
                else if (thing.def.defName == ThingDefOf.Tome.defName) return true;
                return false;
            }
            catch { return false; }
        }

        public static bool CheckIfThingIsGenepack(Thing thing)
        {
            try
            {
                if (!ModsConfig.BiotechActive) return false;

                if (thing.def.defName == ThingDefOf.Genepack.defName) return true;
                return false;
            }
            catch { return false; }
        }

        public static bool CheckIfThingIsXenogerm(Thing thing) 
        {
            try
            {
                if (!ModsConfig.BiotechActive) return false;
                else if (thing.def.defName == ThingDefOf.Xenogerm.defName) return true;
                else return false;
            }
            catch { return false; }
        }

        public static bool CheckIfThingIsBladelinkWeapon(Thing thing)
        {
            try
            {
                if (!ModsConfig.RoyaltyActive) return false;
                else if (thing.TryGetComp<CompBladelinkWeapon>() != null) return true;
                else return false;
            }
            catch { return false; }
        }
        
        public static bool CheckIfThingIsEgg(Thing thing)
        {
            try
            {
                if (thing.TryGetComp<CompHatcher>() != null) return true;
                else return false;
            }
            catch { return false; }
        }

        public static bool CheckIfThingIsRottable(Thing thing)
        {
            try
            {
                if (thing.TryGetComp<CompRottable>() != null) return true;
                else return false;
            }
            catch { return false; }
        }

        public static bool CheckIfThingHasColor(Thing thing)
        {
            try
            {
                if (thing.TryGetComp<CompColorable>() != null) return true;
                else return false;
            }
            catch { return false; }
        }
    }
}