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
        public static AnimalFile ToString(Pawn animal)
        {
            AnimalFile animalData = new AnimalFile();

            animalData.ID = animal.ThingID;

            animalData.ScribeData = RTScriber.ThingToScribe(animal);

            return animalData;
        }

        public static Pawn FromString(AnimalFile file, bool overrideID = false)
        {
            return (Pawn)RTScriber.ScribeToThing(file.ScribeData, overrideID);
        }
    }
}