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
        //Functions

        public static Pawn[] GetHumansFromString(TransferData transferData)
        {
            List<Pawn> humans = new List<Pawn>();

            for (int i = 0; i < transferData._humans.Count(); i++) humans.Add(StringToHuman(transferData._humans[i]));

            return humans.ToArray();
        }

        public static HumanFile HumanToString(Pawn pawn, bool passInventory = true)
        {
            HumanFile humanData = new HumanFile();

            GetHumanID(pawn, humanData);

            GetHumanBioDetails(pawn, humanData);

            GetHumanKind(pawn, humanData);

            GetHumanFaction(pawn, humanData);

            GetHumanHediffs(pawn, humanData);

            if (ModsConfig.BiotechActive)
            {
                GetHumanChildState(pawn, humanData);

                GetHumanXenotype(pawn, humanData);

                GetHumanXenogenes(pawn, humanData);

                GetHumanEndogenes(pawn, humanData);
            }

            GetHumanStory(pawn, humanData);

            GetHumanSkills(pawn, humanData);

            GetHumanTraits(pawn, humanData);

            GetHumanApparel(pawn, humanData);

            GetHumanEquipment(pawn, humanData);

            if (passInventory) GetHumanInventory(pawn, humanData);

            GetHumanFavoriteColor(pawn, humanData);

            GetHumanTransform(pawn, humanData);

            return humanData;
        }

        public static Pawn StringToHuman(HumanFile humanData)
        {
            PawnKindDef kind = SetHumanKind(humanData);

            Faction faction = SetHumanFaction(humanData);

            Pawn pawn = SetHuman(kind, faction, humanData);

            SetHumanID(pawn, humanData);

            SetHumanHediffs(pawn, humanData);

            if (ModsConfig.BiotechActive)
            {
                SetHumanChildState(pawn, humanData);

                SetHumanXenotype(pawn, humanData);

                SetHumanXenogenes(pawn, humanData);

                SetHumanEndogenes(pawn, humanData);
            }

            SetHumanBioDetails(pawn, humanData);

            SetHumanStory(pawn, humanData);

            SetHumanSkills(pawn, humanData);

            SetHumanTraits(pawn, humanData);

            SetHumanApparel(pawn, humanData);

            SetHumanEquipment(pawn, humanData);

            SetHumanInventory(pawn, humanData);

            SetHumanFavoriteColor(pawn, humanData);

            SetHumanTransform(pawn, humanData);

            return pawn;
        }

        //Getters

        private static void GetHumanID(Pawn human, HumanFile humanData)
        {
            try { humanData.ID = human.ThingID; }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void GetHumanBioDetails(Pawn pawn, HumanFile humanData)
        {
            try
            {
                humanData.DefName = pawn.def.defName;
                humanData.Name = pawn.LabelShortCap.ToString();
                humanData.BiologicalAge = pawn.ageTracker.AgeBiologicalTicks.ToString();
                humanData.ChronologicalAge = pawn.ageTracker.AgeChronologicalTicks.ToString();
                humanData.Gender = pawn.gender.ToString();
                
                humanData.HairDefName = pawn.story.hairDef.defName.ToString();
                humanData.HairColor = pawn.story.HairColor.ToString();
                humanData.HeadTypeDefName = pawn.story.headType.defName.ToString();
                humanData.SkinColor = pawn.story.SkinColor.ToString();
                humanData.BeardDefName = pawn.style.beardDef.defName.ToString();
                humanData.BodyTypeDefName = pawn.story.bodyType.defName.ToString();
                humanData.FaceTattooDefName = pawn.style.FaceTattoo.defName.ToString();
                humanData.BodyTattooDefName = pawn.style.BodyTattoo.defName.ToString();
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void GetHumanKind(Pawn pawn, HumanFile humanData)
        {
            try { humanData.KindDef = pawn.kindDef.defName; }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void GetHumanFaction(Pawn pawn, HumanFile humanData)
        {
            try { humanData.FactionDef = pawn.Faction.def.defName; }
            catch (Exception e) 
            { 
                //FIXME
                //Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); 

                // In case it has no apparent faction;
                humanData.FactionDef = Faction.OfPlayer.def.defName;
            }
        }

        private static void GetHumanHediffs(Pawn pawn, HumanFile humanData)
        {
            if (pawn.health.hediffSet.hediffs.Count() > 0)
            {
                List<HediffComponent> toGet = new List<HediffComponent>();

                foreach (Hediff hd in pawn.health.hediffSet.hediffs)
                {
                    try
                    {
                        HediffComponent component = new HediffComponent();
                        component.DefName = hd.def.defName;

                        if (hd.Part != null)
                        {
                            component.PartDefName = hd.Part.def.defName;
                            component.PartLabel = hd.Part.Label;
                        }

                        if (hd.def.CompProps<HediffCompProperties_Immunizable>() != null) component.Immunity = pawn.health.immunity.GetImmunity(hd.def);
                        else component.Immunity = -1f;

                        if (hd.def.tendable)
                        {
                            HediffComp_TendDuration comp = hd.TryGetComp<HediffComp_TendDuration>();
                            if (comp.IsTended)
                            {
                                component.TendQuality = comp.tendQuality;
                                component.TendDuration = comp.tendTicksLeft;
                            } 

                            else 
                            {
                                component.TendDuration = -1;
                                component.TendQuality = -1;
                            }

                            if (comp.TProps.disappearsAtTotalTendQuality >= 0)
                            {
                                Type type = comp.GetType();
                                FieldInfo fieldInfo = type.GetField("totalTendQuality", BindingFlags.NonPublic | BindingFlags.Instance);
                                component.TotalTendQuality = (float)fieldInfo.GetValue(comp);
                            }
                            else component.TotalTendQuality = -1f;
                        } 

                        else 
                        {
                            component.TendDuration = -1;
                            component.TendQuality = -1;
                            component.TotalTendQuality = -1f;
                        }

                        component.Severity = hd.Severity;
                        component.IsPermanent = hd.IsPermanent();

                        toGet.Add(component);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
                }

                humanData.Hediffs = toGet.ToArray();
            }
        }

        private static void GetHumanChildState(Pawn pawn, HumanFile humanData)
        {
            try { humanData.GrowthPoints = pawn.ageTracker.growthPoints; }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void GetHumanXenotype(Pawn pawn, HumanFile humanData)
        {
            try
            {
                if (pawn.genes.Xenotype != null) humanData.Xenotype.DefName = pawn.genes.Xenotype.defName.ToString();
                else humanData.Xenotype.DefName = "null";

                if (pawn.genes.CustomXenotype != null) humanData.Xenotype.CustomXenotypeName = pawn.genes.xenotypeName.ToString();
                else humanData.Xenotype.CustomXenotypeName = "null";
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void GetHumanXenogenes(Pawn pawn, HumanFile humanData)
        {
            if (pawn.genes.Xenogenes.Count() > 0)
            {
                List<XenogeneComponent> toGet = new List<XenogeneComponent>();

                foreach (Gene gene in pawn.genes.Xenogenes)
                {
                    try                 
                    { 
                        XenogeneComponent component = new XenogeneComponent();
                        component.DefName = gene.def.defName;

                        toGet.Add(component);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
                }

                humanData.Xenogenes = toGet.ToArray();
            }
        }

        private static void GetHumanEndogenes(Pawn pawn, HumanFile humanData)
        {
            if (pawn.genes.Endogenes.Count() > 0)
            {
                List<EndogeneComponent> toGet = new List<EndogeneComponent>();

                foreach (Gene gene in pawn.genes.Endogenes)
                {
                    try 
                    {  
                        EndogeneComponent component = new EndogeneComponent();
                        component.DefName = gene.def.defName;

                        toGet.Add(component);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
                }

                humanData.Endogenes = toGet.ToArray();
            }
        }

        private static void GetHumanFavoriteColor(Pawn pawn, HumanFile humanData)
        {
            try { humanData.FavoriteColor = pawn.story.favoriteColor.ToString(); }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void GetHumanStory(Pawn pawn, HumanFile humanData)
        {
            try
            {
                if (pawn.story.Childhood != null) humanData.Stories.ChildhoodStoryDefName = pawn.story.Childhood.defName.ToString();
                else humanData.Stories.ChildhoodStoryDefName = "null";

                if (pawn.story.Adulthood != null) humanData.Stories.AdulthoodStoryDefName = pawn.story.Adulthood.defName.ToString();
                else humanData.Stories.AdulthoodStoryDefName = "null";
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void GetHumanSkills(Pawn pawn, HumanFile humanData)
        {
            if (pawn.skills.skills.Count() > 0)
            {
                List<SkillComponent> toGet = new List<SkillComponent>();

                foreach (SkillRecord skill in pawn.skills.skills)
                {
                    try
                    {
                        SkillComponent component = new SkillComponent();
                        component.DefName = skill.def.defName;
                        component.Level = skill.levelInt;
                        component.Passion = skill.passion.ToString();

                        toGet.Add(component);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
                }

                humanData.Skills = toGet.ToArray();
            }
        }

        private static void GetHumanTraits(Pawn pawn, HumanFile humanData)
        {
            if (pawn.story.traits.allTraits.Count() > 0)
            {
                List<TraitComponent> toGet = new List<TraitComponent>();

                foreach (Trait trait in pawn.story.traits.allTraits)
                {
                    try
                    {
                        TraitComponent component = new TraitComponent();
                        component.DefName = trait.def.defName;
                        component.Degree = trait.Degree;

                        toGet.Add(component);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
                }

                humanData.Traits = toGet.ToArray();
            }
        }

        private static void GetHumanApparel(Pawn pawn, HumanFile humanData)
        {
            if (pawn.apparel.WornApparel.Count() > 0)
            {
                List<ApparelComponent> toGet = new List<ApparelComponent>();

                foreach (Apparel ap in pawn.apparel.WornApparel)
                {
                    try
                    {
                        ThingDataFile thingData = ThingScriber.ItemToString(ap, 1);
                        ApparelComponent component = new ApparelComponent();
                        component.EquippedApparel = thingData;
                        component.WornByCorpse = ap.WornByCorpse;

                        toGet.Add(component);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
                }

                humanData.Apparel = toGet.ToArray();
            }
        }

        private static void GetHumanEquipment(Pawn pawn, HumanFile humanData)
        {
            if (pawn.equipment.Primary != null)
            {
                try
                {
                    ThingWithComps weapon = pawn.equipment.Primary;
                    ThingDataFile thingData = ThingScriber.ItemToString(weapon, weapon.stackCount);
                    humanData.Weapon = thingData;
                }
                catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
            }
        }

        private static void GetHumanInventory(Pawn pawn, HumanFile humanData)
        {
            if (pawn.inventory.innerContainer.Count() != 0)
            {
                List<ItemComponent> toGet = new List<ItemComponent>();

                foreach (Thing thing in pawn.inventory.innerContainer)
                {
                    try
                    {
                        ThingDataFile thingData = ThingScriber.ItemToString(thing, thing.stackCount);
                        ItemComponent component = new ItemComponent();
                        component.Item = thingData;

                        toGet.Add(component);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
                }

                humanData.Items = toGet.ToArray();
            }
        }

        private static void GetHumanTransform(Pawn pawn, HumanFile humanData)
        {
            try
            {
                humanData.Transform.Position = new int[] 
                { 
                    pawn.Position.x,
                    pawn.Position.y, 
                    pawn.Position.z 
                };

                humanData.Transform.Rotation = pawn.Rotation.AsInt;
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        //Setters

        private static PawnKindDef SetHumanKind(HumanFile humanData)
        {
            try { return DefDatabase<PawnKindDef>.AllDefs.First(fetch => fetch.defName == humanData.KindDef); }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }

            return null;
        }

        private static Faction SetHumanFaction(HumanFile humanData)
        {
            try { return Find.FactionManager.AllFactions.First(fetch => fetch.def.defName == humanData.FactionDef); }
            catch (Exception e) 
            { 
                //FIXME
                //Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose);

                // If faction is missing after parsing
                return Faction.OfPlayer;
            }
        }

        private static Pawn SetHuman(PawnKindDef kind, Faction faction, HumanFile humanData)
        {
            try { return PawnGenerator.GeneratePawn(kind, faction); }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }

            return null;
        }

        private static void SetHumanID(Pawn pawn, HumanFile humanData)
        {
            try { pawn.ThingID = humanData.ID; }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }      
        }

        private static void SetHumanBioDetails(Pawn pawn, HumanFile humanData)
        {
            try
            {
                pawn.Name = new NameSingle(humanData.Name);
                pawn.ageTracker.AgeBiologicalTicks = long.Parse(humanData.BiologicalAge);
                pawn.ageTracker.AgeChronologicalTicks = long.Parse(humanData.ChronologicalAge);

                Enum.TryParse(humanData.Gender, true, out Gender humanGender);
                pawn.gender = humanGender;

                pawn.story.hairDef = DefDatabase<HairDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.HairDefName);
                pawn.story.headType = DefDatabase<HeadTypeDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.HeadTypeDefName);
                pawn.style.beardDef = DefDatabase<BeardDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.BeardDefName);
                pawn.story.bodyType = DefDatabase<BodyTypeDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.BodyTypeDefName);
                pawn.style.FaceTattoo = DefDatabase<TattooDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.FaceTattooDefName);
                pawn.style.BodyTattoo = DefDatabase<TattooDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.BodyTattooDefName);

                string hairColor = humanData.HairColor.Replace("RGBA(", "").Replace(")", "");
                string[] isolatedHair = hairColor.Split(',');
                float r = float.Parse(isolatedHair[0]);
                float g = float.Parse(isolatedHair[1]);
                float b = float.Parse(isolatedHair[2]);
                float a = float.Parse(isolatedHair[3]);
                pawn.story.HairColor = new UnityEngine.Color(r, g, b, a);

                string skinColor = humanData.SkinColor.Replace("RGBA(", "").Replace(")", "");
                string[] isolatedSkin = skinColor.Split(',');
                r = float.Parse(isolatedSkin[0]);
                g = float.Parse(isolatedSkin[1]);
                b = float.Parse(isolatedSkin[2]);
                a = float.Parse(isolatedSkin[3]);
                pawn.story.SkinColorBase = new UnityEngine.Color(r, g, b, a);
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void SetHumanHediffs(Pawn pawn, HumanFile humanData)
        {
            try
            {
                pawn.health.RemoveAllHediffs();
                pawn.health.Reset();
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }

            if (humanData.Hediffs.Length > 0)
            {
                for (int i = 0; i < humanData.Hediffs.Length; i++)
                {
                    try
                    {
                        HediffComponent component = humanData.Hediffs[i];
                        HediffDef hediffDef = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == component.DefName);
                        BodyPartRecord bodyPart = pawn.RaceProps.body.AllParts.FirstOrDefault(x => x.def.defName == component.PartDefName && 
                            x.Label == component.PartLabel);

                        Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                        hediff.Severity = component.Severity;

                        if (component.IsPermanent)
                        {
                            HediffComp_GetsPermanent hediffComp = hediff.TryGetComp<HediffComp_GetsPermanent>();
                            hediffComp.IsPermanent = true;
                        }

                        pawn.health.AddHediff(hediff, bodyPart);
                        if (component.Immunity != -1f)
                        {
                            pawn.health.immunity.TryAddImmunityRecord(hediffDef, hediffDef);
                            ImmunityRecord immunityRecord = pawn.health.immunity.GetImmunityRecord(hediffDef);
                            immunityRecord.immunity = component.Immunity;
                        }

                        if (component.TendDuration != -1)
                        {
                            HediffComp_TendDuration comp = hediff.TryGetComp<HediffComp_TendDuration>();
                            comp.tendQuality = component.TendQuality;
                            comp.tendTicksLeft = component.TendDuration;
                        }
                        
                        if (component.TotalTendQuality != -1f) 
                        {
                            HediffComp_TendDuration comp = hediff.TryGetComp<HediffComp_TendDuration>();
                            Type type = comp.GetType();
                            FieldInfo fieldInfo = type.GetField("totalTendQuality", BindingFlags.NonPublic | BindingFlags.Instance);
                            fieldInfo.SetValue(comp, component.TotalTendQuality);
                        }
                    }
                    catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
                }
            }
        }

        private static void SetHumanChildState(Pawn pawn, HumanFile humanData)
        {
            try { pawn.ageTracker.growthPoints = humanData.GrowthPoints; }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void SetHumanXenotype(Pawn pawn, HumanFile humanData)
        {
            try
            {
                if (humanData.Xenotype.DefName != "null")
                {
                    pawn.genes.SetXenotype(DefDatabase<XenotypeDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.Xenotype.DefName));
                }

                if (humanData.Xenotype.CustomXenotypeName != "null")
                {
                    pawn.genes.xenotypeName = humanData.Xenotype.CustomXenotypeName;
                }
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void SetHumanXenogenes(Pawn pawn, HumanFile humanData)
        {
            try { pawn.genes.Xenogenes.Clear(); }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }

            if (humanData.Xenogenes.Length > 0)
            {
                for (int i = 0; i < humanData.Xenogenes.Length; i++)
                {
                    try
                    {
                        XenogeneComponent component = humanData.Xenogenes[i];
                        GeneDef def = DefDatabase<GeneDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == component.DefName);
                        if (def != null) pawn.genes.AddGene(def, true);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
                }
            }
        }

        private static void SetHumanEndogenes(Pawn pawn, HumanFile humanData)
        {
            try { pawn.genes.Endogenes.Clear(); }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }

            if (humanData.Endogenes.Length > 0)
            {
                for (int i = 0; i < humanData.Endogenes.Length; i++)
                {
                    try
                    {
                        EndogeneComponent component = humanData.Endogenes[i];
                        GeneDef def = DefDatabase<GeneDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == component.DefName);
                        if (def != null) pawn.genes.AddGene(def, true);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
                }
            }
        }

        private static void SetHumanFavoriteColor(Pawn pawn, HumanFile humanData)
        {
            try
            {
                float r;
                float g;
                float b;
                float a;

                string favoriteColor = humanData.FavoriteColor.Replace("RGBA(", "").Replace(")", "");
                string[] isolatedFavoriteColor = favoriteColor.Split(',');
                r = float.Parse(isolatedFavoriteColor[0]);
                g = float.Parse(isolatedFavoriteColor[1]);
                b = float.Parse(isolatedFavoriteColor[2]);
                a = float.Parse(isolatedFavoriteColor[3]);
                pawn.story.favoriteColor = new UnityEngine.Color(r, g, b, a);
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void SetHumanStory(Pawn pawn, HumanFile humanData)
        {
            try
            {
                if (humanData.Stories.ChildhoodStoryDefName != "null")
                {
                    pawn.story.Childhood = DefDatabase<BackstoryDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.Stories.ChildhoodStoryDefName);
                }

                if (humanData.Stories.AdulthoodStoryDefName != "null")
                {
                    pawn.story.Adulthood = DefDatabase<BackstoryDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == humanData.Stories.AdulthoodStoryDefName);
                }
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void SetHumanSkills(Pawn pawn, HumanFile humanData)
        {
            if (humanData.Skills.Length > 0)
            {
                for (int i = 0; i < humanData.Skills.Length; i++)
                {
                    try
                    {
                        SkillComponent component = humanData.Skills[i];
                        pawn.skills.skills[i].levelInt = component.Level;

                        Enum.TryParse(component.Passion, true, out Passion passion);
                        pawn.skills.skills[i].passion = passion;
                    }
                    catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
                }
            }
        }

        private static void SetHumanTraits(Pawn pawn, HumanFile humanData)
        {
            try { pawn.story.traits.allTraits.Clear(); }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }

            if (humanData.Traits.Length > 0)
            {
                for (int i = 0; i < humanData.Traits.Length; i++)
                {
                    try
                    {
                        TraitComponent component = humanData.Traits[i];
                        TraitDef traitDef = DefDatabase<TraitDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == component.DefName);
                        Trait trait = new Trait(traitDef, component.Degree);
                        pawn.story.traits.GainTrait(trait);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
                }
            }
        }

        private static void SetHumanApparel(Pawn pawn, HumanFile humanData)
        {
            try
            {
                pawn.apparel.DestroyAll();
                pawn.apparel.DropAllOrMoveAllToInventory();
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }

            if (humanData.Apparel.Length > 0)
            {
                for (int i = 0; i < humanData.Apparel.Length; i++)
                {
                    try
                    {
                        ApparelComponent component = humanData.Apparel[i];
                        Apparel apparel = (Apparel)ThingScriber.StringToItem(component.EquippedApparel);
                        if (component.WornByCorpse) apparel.WornByCorpse.MustBeTrue();
                        else apparel.WornByCorpse.MustBeFalse();

                        pawn.apparel.Wear(apparel);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
                }
            }
        }

        private static void SetHumanEquipment(Pawn pawn, HumanFile humanData)
        {
            try { pawn.equipment.DestroyAllEquipment(); }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }

            if (humanData.Weapon != null)
            {
                try
                {
                    ThingWithComps thing = (ThingWithComps)ThingScriber.StringToItem(humanData.Weapon);
                    pawn.equipment.AddEquipment(thing);
                }
                catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
            }
        }

        private static void SetHumanInventory(Pawn pawn, HumanFile humanData)
        {
            if (humanData.Items.Length > 0)
            {
                for (int i = 0; i < humanData.Items.Length; i++)
                {
                    try
                    {
                        ItemComponent component = humanData.Items[i];
                        Thing thing = ThingScriber.StringToItem(component.Item);
                        pawn.inventory.TryAddAndUnforbid(thing);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
                }
            }
        }

        private static void SetHumanTransform(Pawn pawn, HumanFile humanData)
        {
            try
            {
                pawn.Position = new IntVec3(humanData.Transform.Position[0], humanData.Transform.Position[1], humanData.Transform.Position[2]);
                pawn.Rotation = new Rot4(humanData.Transform.Rotation);
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }
    }
}