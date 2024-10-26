using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Shared;
using Verse;

namespace GameClient
{
    public static class AnimalScriber
    {
        //Functions

        public static Pawn[] GetAnimalsFromString(TransferData transferData)
        {
            List<Pawn> animals = new List<Pawn>();

            for (int i = 0; i < transferData._animals.Count(); i++) animals.Add(StringToAnimal(transferData._animals[i]));

            return animals.ToArray();
        }

        public static AnimalFile AnimalToString(Pawn animal)
        {
            AnimalFile animalData = new AnimalFile();

            GetAnimalID(animal, animalData);

            GetAnimalBioDetails(animal, animalData);

            GetAnimalKind(animal, animalData);

            GetAnimalFaction(animal, animalData);

            GetAnimalHediffs(animal, animalData);

            GetAnimalSkills(animal, animalData);

            GetAnimalTransform(animal, animalData);

            return animalData;
        }

        public static Pawn StringToAnimal(AnimalFile animalData, bool overrideID = false)
        {
            PawnKindDef kind = SetAnimalKind(animalData);

            Faction faction = SetAnimalFaction(animalData);

            Pawn animal = SetAnimal(kind, faction, animalData);

            if (overrideID) SetAnimalID(animal, animalData);

            SetAnimalBioDetails(animal, animalData);

            SetAnimalHediffs(animal, animalData);

            SetAnimalSkills(animal, animalData);

            SetAnimalTransform(animal, animalData);

            return animal;
        }

        //Getters

        private static void GetAnimalID(Pawn animal, AnimalFile animalData)
        {
            try { animalData.ID = animal.ThingID; }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }


        private static void GetAnimalBioDetails(Pawn animal, AnimalFile animalData)
        {
            try
            {
                animalData.DefName = animal.def.defName;
                animalData.Name = animal.LabelShortCap.ToString();
                animalData.BiologicalAge = animal.ageTracker.AgeBiologicalTicks.ToString();
                animalData.ChronologicalAge = animal.ageTracker.AgeChronologicalTicks.ToString();
                animalData.Gender = animal.gender.ToString();
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void GetAnimalKind(Pawn animal, AnimalFile animalData)
        {
            try { animalData.KindDef = animal.kindDef.defName; }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void GetAnimalFaction(Pawn animal, AnimalFile animalData)
        {
            try { animalData.FactionDef = animal.Faction.def.defName; }
            catch (Exception e) 
            { 
                //FIXME
                //Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); 

                // In case it has no apparent faction;
                animalData.FactionDef = Faction.OfPlayer.def.defName;
            }
        }

        private static void GetAnimalHediffs(Pawn animal, AnimalFile animalData)
        {
            if (animal.health.hediffSet.hediffs.Count() > 0)
            {
                List<HediffComponent> toGet = new List<HediffComponent>();

                foreach (Hediff hd in animal.health.hediffSet.hediffs)
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

                        component.Severity = hd.Severity;
                        component.IsPermanent = hd.IsPermanent();

                        toGet.Add(component);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
                }

                animalData.Hediffs = toGet.ToArray();
            }
        }

        private static void GetAnimalSkills(Pawn animal, AnimalFile animalData)
        {
            if (animal.training == null) return;

            List<TrainableComponent> toGet = new List<TrainableComponent>();

            foreach (TrainableDef trainable in DefDatabase<TrainableDef>.AllDefsListForReading)
            {
                try
                {
                    TrainableComponent component = new TrainableComponent();
                    component.DefName = trainable.defName;
                    component.CanTrain = animal.training.CanAssignToTrain(trainable).Accepted;
                    component.HasLearned = animal.training.HasLearned(trainable);
                    component.IsDisabled = animal.training.GetWanted(trainable);

                    toGet.Add(component);
                }
                catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
            }

            animalData.Trainables = toGet.ToArray();
        }

        private static void GetAnimalTransform(Pawn animal, AnimalFile animalData)
        {
            try
            {
                animalData.Transform.Position = new int[] { animal.Position.x, animal.Position.y, animal.Position.z};
                animalData.Transform.Rotation = animal.Rotation.AsInt;
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        //Setters

        private static PawnKindDef SetAnimalKind(AnimalFile animalData)
        {
            try { return DefDatabase<PawnKindDef>.AllDefs.First(fetch => fetch.defName == animalData.DefName); }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }

            return null;
        }

        private static Faction SetAnimalFaction(AnimalFile animalData)
        {
            try { return Find.FactionManager.AllFactions.First(fetch => fetch.def.defName == animalData.FactionDef); }
            catch (Exception e) 
            { 
                //FIXME
                //Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose);

                // If faction is missing after parsing
                return Faction.OfPlayer;
            }
        }

        private static Pawn SetAnimal(PawnKindDef kind, Faction faction, AnimalFile animalData)
        {
            try { return PawnGenerator.GeneratePawn(kind, faction); }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }

            return null;
        }

        private static void SetAnimalID(Pawn animal, AnimalFile animalData)
        {
            try { animal.ThingID = animalData.ID; }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }  
        }

        private static void SetAnimalBioDetails(Pawn animal, AnimalFile animalData)
        {
            try
            {
                animal.Name = new NameSingle(animalData.Name);
                animal.ageTracker.AgeBiologicalTicks = long.Parse(animalData.BiologicalAge);
                animal.ageTracker.AgeChronologicalTicks = long.Parse(animalData.ChronologicalAge);

                Enum.TryParse(animalData.Gender, true, out Gender animalGender);
                animal.gender = animalGender;
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }

        private static void SetAnimalHediffs(Pawn animal, AnimalFile animalData)
        {
            try
            {
                animal.health.RemoveAllHediffs();
                animal.health.Reset();
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }

            if (animalData.Hediffs.Length > 0)
            {
                for (int i = 0; i < animalData.Hediffs.Length; i++)
                {
                    try
                    {
                        HediffComponent component = animalData.Hediffs[i];
                        HediffDef hediffDef = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == component.DefName);
                        BodyPartRecord bodyPart = animal.RaceProps.body.AllParts.ToList().Find(x => x.def.defName == component.PartDefName &&
                            x.Label == component.PartLabel);

                        Hediff hediff = HediffMaker.MakeHediff(hediffDef, animal, bodyPart);
                        hediff.Severity = component.Severity;

                        if (component.IsPermanent)
                        {
                            HediffComp_GetsPermanent hediffComp = hediff.TryGetComp<HediffComp_GetsPermanent>();
                            hediffComp.IsPermanent = true;
                        }

                        animal.health.AddHediff(hediff);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
                }
            }
        }

        private static void SetAnimalSkills(Pawn animal, AnimalFile animalData)
        {
            if (animalData.Trainables.Length > 0)
            {
                for (int i = 0; i < animalData.Trainables.Length; i++)
                {
                    try
                    {
                        TrainableComponent component = animalData.Trainables[i];
                        TrainableDef trainable = DefDatabase<TrainableDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == component.DefName);
                        if (component.CanTrain) animal.training.Train(trainable, null, complete: component.HasLearned);
                    }
                    catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
                }
            }
        }

        private static void SetAnimalTransform(Pawn animal, AnimalFile animalData)
        {
            try
            {
                animal.Position = new IntVec3(animalData.Transform.Position[0], animalData.Transform.Position[1], animalData.Transform.Position[2]);
                animal.Rotation = new Rot4(animalData.Transform.Rotation);
            }
            catch (Exception e) { Logger.Warning(e.ToString(), CommonEnumerators.LogImportanceMode.Verbose); }
        }
    }
}