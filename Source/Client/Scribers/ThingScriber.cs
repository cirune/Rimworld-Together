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
        //Functions

        public static Thing[] GetItemsFromString(TransferData transferData)
        {
            List<Thing> things = new List<Thing>();

            for (int i = 0; i < transferData._things.Count(); i++)
            {
                Thing thingToAdd = StringToItem(transferData._things[i]);
                if (thingToAdd != null) things.Add(thingToAdd);
            }

            return things.ToArray();
        }

        public static ThingDataFile ItemToString(Thing thing, int thingCount)
        {
            ThingDataFile thingData = new ThingDataFile();

            Thing toUse = null;
            if (GetItemMinified(thing, thingData)) toUse = thing.GetInnerIfMinified();
            else toUse = thing;

            GetItemID(toUse, thingData);

            GetItemName(toUse, thingData);

            GetItemMaterial(toUse, thingData);

            GetItemQuantity(toUse, thingData, thingCount);

            GetItemQuality(toUse, thingData);

            GetItemHitpoints(toUse, thingData);

            GetItemTransform(toUse, thingData);

            if (ScribeHelper.CheckIfThingIsGenepack(toUse)) GetGenepackDetails(toUse, thingData);
            else if (ScribeHelper.CheckIfThingIsBook(toUse)) GetBookDetails(toUse, thingData);
            else if (ScribeHelper.CheckIfThingIsXenogerm(toUse)) GetXenogermDetails(toUse, thingData);
            else if (ScribeHelper.CheckIfThingIsBladelinkWeapon(toUse)) GetBladelinkWeaponDetails(toUse, thingData);
            else if (ScribeHelper.CheckIfThingIsEgg(toUse)) GetEggDetails(thing, thingData);
            else if (ScribeHelper.CheckIfThingIsRottable(toUse)) GetRotDetails(thing, thingData);
            else if (ScribeHelper.CheckIfThingHasColor(thing)) GetColorDetails(toUse, thingData);;
            return thingData;
        }

        public static Thing StringToItem(ThingDataFile thingData, bool overrideID = false)
        {
            Thing thing = SetItem(thingData);

            if (overrideID) SetItemID(thing, thingData);

            SetItemQuantity(thing, thingData);

            SetItemQuality(thing, thingData);

            SetItemHitpoints(thing, thingData);

            SetItemTransform(thing, thingData);

            if (ScribeHelper.CheckIfThingIsGenepack(thing)) SetGenepackDetails(thing, thingData);
            else if (ScribeHelper.CheckIfThingIsBook(thing)) SetBookDetails(thing, thingData);
            else if (ScribeHelper.CheckIfThingIsXenogerm(thing)) SetXenogermDetails(thing, thingData);
            else if (ScribeHelper.CheckIfThingIsBladelinkWeapon(thing)) SetBladelinkWeaponDetails(thing, thingData);
            else if (ScribeHelper.CheckIfThingIsEgg(thing)) SetEggDetails(thing, thingData);
            else if (ScribeHelper.CheckIfThingIsRottable(thing)) SetRotDetails(thing, thingData);
            else if (ScribeHelper.CheckIfThingHasColor(thing)) SetColorDetails(thing, thingData);
            return thing;
        }

        //Getters

        private static void GetItemID(Thing thing, ThingDataFile thingData)
        {
            try { thingData.ID = thing.ThingID; }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void GetItemName(Thing thing, ThingDataFile thingData)
        {
            try { thingData.DefName = thing.def.defName; }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void GetItemMaterial(Thing thing, ThingDataFile thingData)
        {
            try
            {
                if (ScribeHelper.CheckIfThingHasMaterial(thing)) thingData.MaterialDefName = thing.Stuff.defName;
                else thingData.MaterialDefName = null;
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void GetItemQuantity(Thing thing, ThingDataFile thingData, int thingCount)
        {
            try { thingData.Quantity = thingCount; }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void GetItemQuality(Thing thing, ThingDataFile thingData)
        {
            try { thingData.Quality = ScribeHelper.GetThingQuality(thing); }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void GetItemHitpoints(Thing thing, ThingDataFile thingData)
        {
            try { thingData.Hitpoints = thing.HitPoints; }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void GetItemTransform(Thing thing, ThingDataFile thingData)
        {
            try
            {
                thingData.TransformComponent.Position = new int[] { thing.Position.x, thing.Position.y, thing.Position.z };
                thingData.TransformComponent.Rotation = thing.Rotation.AsInt;
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void GetRotDetails(Thing thing, ThingDataFile thingData) 
        {
            try
            {
                CompRottable comp = thing.TryGetComp<CompRottable>();
                thingData.RotProgress = comp.RotProgress;
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }
        
        private static void GetEggDetails(Thing thing, ThingDataFile thingData) 
        {
            try
            {
                CompHatcher comp = thing.TryGetComp<CompHatcher>();
                thingData.EggData.ruinedPercent = (float)AccessTools.Field(typeof(CompTemperatureRuinable), "ruinedPercent").GetValue(thing.TryGetComp<CompTemperatureRuinable>());
                thingData.EggData.gestateProgress = (float)AccessTools.Field(typeof(CompHatcher), "gestateProgress").GetValue(comp);
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static bool GetItemMinified(Thing thing, ThingDataFile thingData)
        {
            try
            {
                thingData.IsMinified = ScribeHelper.CheckIfThingIsMinified(thing);
                return thingData.IsMinified;
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }

            return false;
        }

        private static void GetColorDetails(Thing thing, ThingDataFile thingData) 
        {
            try
            {
                thingData.Color[0] = thing.DrawColor.r;
                thingData.Color[1] = thing.DrawColor.g;
                thingData.Color[2] = thing.DrawColor.b;
                thingData.Color[3] = thing.DrawColor.a;
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }    
        }

        private static void GetGenepackDetails(Thing thing, ThingDataFile thingData)
        {
            try
            {
                Genepack genepack = (Genepack)thing;
                foreach (GeneDef gene in genepack.GeneSet.GenesListForReading) thingData.GenepackComponent.GenepackDefs.Add(gene.defName);
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void GetBookDetails(Thing thing, ThingDataFile thingData)
        {
            try
            {
                BookComponent bookData = new BookComponent();
                Book book = (Book)thing;
                bookData.Title = book.Title;
                bookData.Description = book.DescriptionDetailed;
                bookData.DescriptionFlavor = book.FlavorUI;

                Type type = book.GetType();
                FieldInfo fieldInfo = type.GetField("mentalBreakChancePerHour", BindingFlags.NonPublic | BindingFlags.Instance);
                bookData.MentalBreakChance = (float)fieldInfo.GetValue(book);

                type = book.GetType();
                fieldInfo = type.GetField("joyFactor", BindingFlags.NonPublic | BindingFlags.Instance);
                bookData.JoyFactor = (float)fieldInfo.GetValue(book);

                book.BookComp.TryGetDoer<BookOutcomeDoerGainSkillExp>(out BookOutcomeDoerGainSkillExp xp);
                if (xp != null)
                {
                    foreach (KeyValuePair<SkillDef, float> pair in xp.Values)
                    {
                        bookData.SkillData.Add(pair.Key.defName, pair.Value);
                    }
                }

                book.BookComp.TryGetDoer<ReadingOutcomeDoerGainResearch>(out ReadingOutcomeDoerGainResearch research);
                if (research != null)
                {
                    type = research.GetType();
                    fieldInfo = type.GetField("values", BindingFlags.NonPublic | BindingFlags.Instance);
                    Dictionary<ResearchProjectDef, float> researchDict = (Dictionary<ResearchProjectDef, float>)fieldInfo.GetValue(research);
                    foreach (ResearchProjectDef key in researchDict.Keys) bookData.ResearchData.Add(key.defName, researchDict[key]);
                }

                thingData.BookComponent = bookData;
                Logger.Warning(bookData.Title);
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void GetXenogermDetails(Thing thing, ThingDataFile thingDataFile) 
        {
            try 
            {
                Xenogerm germData = (Xenogerm)thing;
                foreach (GeneDef gene in germData.GeneSet.GenesListForReading) thingDataFile.XenogermComponent.geneDefs.Add(gene.defName);
                thingDataFile.XenogermComponent.xenoTypeName = germData.xenotypeName;
                thingDataFile.XenogermComponent.iconDef = germData.iconDef.defName;
            } catch (Exception e ) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); } 
        }

        private static void GetBladelinkWeaponDetails(Thing thing, ThingDataFile thingDataFile)
        {
            try
            {
                ThingWithComps personaData = (ThingWithComps)thing;
                List<string> defnames = new List<string>();
                
                CompBladelinkWeapon comp = personaData.GetComp<CompBladelinkWeapon>();
                foreach (WeaponTraitDef trait in comp.TraitsListForReading) defnames.Add(trait.defName);
                thingDataFile.BladelinkWeaponData.traitdefs = defnames.ToArray();

                CompGeneratedNames name = personaData.TryGetComp<CompGeneratedNames>();
                thingDataFile.BladelinkWeaponData.name = name.Name;
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }
        //Setters

        private static Thing SetItem(ThingDataFile thingData)
        {
            try
            {
                ThingDef thingDef = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == thingData.DefName);
                ThingDef defMaterial = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == thingData.MaterialDefName);
                return ThingMaker.MakeThing(thingDef, defMaterial);
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }

            throw new IndexOutOfRangeException(thingData.ToString());
        }

        private static void SetItemID(Thing thing, ThingDataFile thingData)
        {
            try { thing.ThingID = thingData.ID; }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }    
        }

        private static void SetItemQuantity(Thing thing, ThingDataFile thingData)
        {
            try { thing.stackCount = thingData.Quantity; }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void SetItemQuality(Thing thing, ThingDataFile thingData)
        {
            if (thingData.Quality != -1)
            {
                try
                {
                    CompQuality compQuality = thing.TryGetComp<CompQuality>();
                    if (compQuality != null)
                    {
                        QualityCategory iCategory = (QualityCategory)thingData.Quality;
                        compQuality.SetQuality(iCategory, ArtGenerationContext.Outsider);
                    }
                }
                catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
            }
        }

        private static void SetItemHitpoints(Thing thing, ThingDataFile thingData)
        {
            try { thing.HitPoints = thingData.Hitpoints; }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void SetItemTransform(Thing thing, ThingDataFile thingData)
        {
            try
            { 
                thing.Position = new IntVec3(thingData.TransformComponent.Position[0], thingData.TransformComponent.Position[1], thingData.TransformComponent.Position[2]);
                thing.Rotation = new Rot4(thingData.TransformComponent.Rotation);
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void SetRotDetails(Thing thing, ThingDataFile thingData) 
        {
            try
            {
                CompRottable comp = thing.TryGetComp<CompRottable>();
                comp.RotProgress = thingData.RotProgress;
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void SetColorDetails(Thing thing, ThingDataFile thingData) 
        {
            try
            {
                thing.SetColor(new UnityEngine.Color(
                    thingData.Color[0],
                    thingData.Color[1],
                    thingData.Color[2],
                    thingData.Color[3]));
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }
        
        private static void SetEggDetails(Thing thing, ThingDataFile thingData)
        {
            try
            {
                CompHatcher comp = thing.TryGetComp<CompHatcher>();
                AccessTools.Field(typeof(CompHatcher), "gestateProgress").SetValue(comp, thingData.EggData.gestateProgress);
                AccessTools.Field(typeof(CompTemperatureRuinable), "ruinedPercent").SetValue(thing.TryGetComp<CompTemperatureRuinable>(), thingData.EggData.ruinedPercent);
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void SetGenepackDetails(Thing thing, ThingDataFile thingData)
        {
            try
            {
                Genepack genepack = (Genepack)thing;
                List<GeneDef> geneDefs = new List<GeneDef>();
                foreach (string str in thingData.GenepackComponent.GenepackDefs)
                {
                    GeneDef gene = DefDatabase<GeneDef>.AllDefs.First(fetch => fetch.defName == str);
                    geneDefs.Add(gene);
                }
                genepack.Initialize(geneDefs);
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void SetBookDetails(Thing thing, ThingDataFile thingData)
        {
            try
            {
                Book book = (Book)thing;
                Type type = book.GetType();

                FieldInfo fieldInfo = type.GetField("title", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfo.SetValue(book, thingData.BookComponent.Title);

                fieldInfo = type.GetField("description", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfo.SetValue(book, thingData.BookComponent.Description);

                fieldInfo = type.GetField("descriptionFlavor", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfo.SetValue(book, thingData.BookComponent.DescriptionFlavor);

                fieldInfo = type.GetField("mentalBreakChancePerHour", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfo.SetValue(book, thingData.BookComponent.MentalBreakChance);

                fieldInfo = type.GetField("joyFactor", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfo.SetValue(book, thingData.BookComponent.JoyFactor);

                book.BookComp.TryGetDoer<BookOutcomeDoerGainSkillExp>(out BookOutcomeDoerGainSkillExp doerXP);
                if (doerXP != null)
                {
                    type = doerXP.GetType();
                    fieldInfo = type.GetField("values", BindingFlags.NonPublic | BindingFlags.Instance);
                    Dictionary<SkillDef, float> skilldict = new Dictionary<SkillDef, float>();

                    foreach (string str in thingData.BookComponent.SkillData.Keys)
                    {
                        SkillDef skillDef = DefDatabase<SkillDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == str);
                        skilldict.Add(skillDef, thingData.BookComponent.SkillData[str]);
                    }

                    fieldInfo.SetValue(doerXP, skilldict);
                }

                book.BookComp.TryGetDoer<ReadingOutcomeDoerGainResearch>(out ReadingOutcomeDoerGainResearch doerResearch);
                if (doerResearch != null)
                {
                    type = doerResearch.GetType();
                    fieldInfo = type.GetField("values", BindingFlags.NonPublic | BindingFlags.Instance);
                    Dictionary<ResearchProjectDef, float> researchDict = new Dictionary<ResearchProjectDef, float>();

                    foreach (string str in thingData.BookComponent.ResearchData.Keys)
                    {
                        ResearchProjectDef researchDef = DefDatabase<ResearchProjectDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == str);
                        researchDict.Add(researchDef, thingData.BookComponent.ResearchData[str]);
                    }

                    fieldInfo.SetValue(doerResearch, researchDict);
                }
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void SetXenogermDetails(Thing thing, ThingDataFile thingDataFile)
        {
            try
            {
                Xenogerm germData = (Xenogerm)thing;
                List<Genepack> genePacks = new List<Genepack>();
                foreach (string genepacks in thingDataFile.XenogermComponent.geneDefs) 
                {
                    Genepack genepack = new Genepack();
                    List<GeneDef> geneDefs = new List<GeneDef>();
                    geneDefs.Add(DefDatabase<GeneDef>.GetNamed(genepacks));
                    genepack.Initialize(geneDefs);
                    genePacks.Add(genepack);
                }
                germData.Initialize(genePacks, thingDataFile.XenogermComponent.xenoTypeName, DefDatabase<XenotypeIconDef>.GetNamed(thingDataFile.XenogermComponent.iconDef));
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void SetBladelinkWeaponDetails(Thing thing, ThingDataFile thingDataFile)
        {
            try
            {
                ThingWithComps personaWeapon = (ThingWithComps)thing;
                CompBladelinkWeapon comp = personaWeapon.GetComp<CompBladelinkWeapon>();

                List<WeaponTraitDef> traitList = new List<WeaponTraitDef>();
                foreach (string trait in thingDataFile.BladelinkWeaponData.traitdefs) 
                {
                    WeaponTraitDef traitDef = DefDatabase<WeaponTraitDef>.GetNamedSilentFail(trait);
                    traitList.Add(traitDef);
                }
                AccessTools.Field(comp.GetType(), "traits").SetValue(comp, traitList);
                
                CompGeneratedNames name = personaWeapon.GetComp<CompGeneratedNames>();
                Type type = name.GetType();
                FieldInfo field = type.GetField("name", BindingFlags.NonPublic | BindingFlags.Instance);
                field.SetValue(name, thingDataFile.BladelinkWeaponData.name);
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }
    }
}