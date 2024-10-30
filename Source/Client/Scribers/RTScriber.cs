using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class RTScriber
    {
        public static StringWriter stringWriter;

        private static readonly string scribeTreeName = "Tree";

        private static readonly string scribeNodeName = "Node";

        public static string ThingToScribe(Thing toSave, int customCount = -1)
        {
            ClientValues.ToggleUsingScriber(true);

            try
            {
                int originalCount = toSave.stackCount;
                if (customCount != -1) toSave.stackCount = customCount;

                Scribe.saver.InitSaving("", scribeTreeName);

                Scribe_Deep.Look(ref toSave, scribeNodeName);
                
                Scribe.saver.FinalizeSaving();

                if (customCount != -1) toSave.stackCount = originalCount;
            }
            catch (Exception e) { Logger.Error(e.ToString(), LogImportanceMode.Verbose); };

            ClientValues.ToggleUsingScriber(false);

            return stringWriter.ToString();
        }

        public static Thing ScribeToThing(string scribeData, bool hasCustomID)
        {
            ClientValues.ToggleUsingScriber(true);

            Thing toLoad = null;

            try
            {
                Scribe.loader.InitLoading(scribeData);

                Scribe_Deep.Look(ref toLoad, scribeNodeName);

                Scribe.loader.FinalizeLoading();

                if (!hasCustomID) toLoad.thingIDNumber = Find.UniqueIDsManager.GetNextThingID();
            }
            catch (Exception e) { Logger.Error(e.ToString(), LogImportanceMode.Verbose); };

            ClientValues.ToggleUsingScriber(false);

            return toLoad;
        }
    }
}