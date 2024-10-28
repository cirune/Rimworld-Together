using System.IO;
using Verse;

namespace GameClient
{
    public static class RTScriber
    {
        private static readonly string scribeTreeName = "Tree";

        private static readonly string scribeNodeName = "Node";

        public static string ThingToScribe(Thing toSave)
        {
            Scribe.saver.InitSaving(Path.Combine(Master.scribeFolderPath, "LatestSentScribe.xml"), scribeTreeName);
            Scribe_Deep.Look(ref toSave, scribeNodeName);
            Scribe.saver.FinalizeSaving();
            
            return File.ReadAllText(Path.Combine(Master.scribeFolderPath, "LatestSentScribe.xml"));
        }

        public static Thing ScribeToThing(string scribeData, bool hasCustomID)
        {
            File.WriteAllText(Path.Combine(Master.scribeFolderPath, "LatestReceivedScribe.xml"), scribeData);

            Scribe.loader.InitLoading(Path.Combine(Master.scribeFolderPath, "LatestReceivedScribe.xml"));
            Thing toLoad = null;
            Scribe_Deep.Look(ref toLoad, scribeNodeName);
            if (!hasCustomID) toLoad.thingIDNumber = Find.UniqueIDsManager.GetNextThingID();
            Scribe.loader.FinalizeLoading();

            return toLoad;
        }
    }
}