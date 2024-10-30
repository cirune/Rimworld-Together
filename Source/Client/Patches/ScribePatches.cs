using System;
using System.IO;
using System.Xml;
using HarmonyLib;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    [HarmonyPatch(typeof(ScribeSaver), nameof(ScribeSaver.InitSaving))]
    public static class PatchSaving
    {
        [HarmonyPrefix]
        public static bool DoPre(ScribeSaver __instance, ref XmlWriter ___writer, string documentElementName)
        {
            if (Network.state == ClientNetworkState.Disconnected) return true;
            else if (!ClientValues.isUsingScriber) return true;
            else
            {
                try
                {
                    Scribe.mode = LoadSaveMode.Saving;
                    
                    XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                    xmlWriterSettings.Indent = true;
                    xmlWriterSettings.IndentChars = "\t";

                    RTScriber.stringWriter = new StringWriter();
                    ___writer = XmlWriter.Create(RTScriber.stringWriter, xmlWriterSettings);
                    ___writer.WriteStartDocument();
                    __instance.EnterNode(documentElementName);
                }

                catch (Exception e)
                {
                    Logger.Error($"Exception while init save patched scribe: {e}");
                    __instance.ForceStop();
                    throw;
                }

                return false;
            }
        }
    }

    [HarmonyPatch(typeof(ScribeLoader), nameof(ScribeLoader.InitLoading))]
    public static class PatchLoading
    {
        [HarmonyPrefix]
        public static bool DoPre(ScribeLoader __instance, string filePath)
        {
            if (Network.state == ClientNetworkState.Disconnected) return true;
            else if (!ClientValues.isUsingScriber) return true;
            else
            {
                try
                {
                    using (StringReader input = new StringReader(filePath))
                    {
                        using XmlTextReader reader = new XmlTextReader(input);
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.Load(reader);

                        __instance.curXmlParent = xmlDocument.DocumentElement;
                    }

                    Scribe.mode = LoadSaveMode.LoadingVars;
                }

                catch (Exception e)
                {
                    Logger.Error($"Exception while init load patched scribe: {e}");
                    __instance.ForceStop();
                    throw;
                }

                return false;
            }
        }
    }

    
}