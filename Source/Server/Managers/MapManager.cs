using Shared;

namespace GameServer
{
    public static class MapManager
    {
        //Variables

        public readonly static string fileExtension = ".mpmap";

        public static void ParsePacket(ServerClient client, Packet packet)
        {
            MapData mapData = Serializer.ConvertBytesToObject<MapData>(packet.contents);
            SaveUserMap(client, mapData._mapFile);
        }

        public static void SaveUserMap(ServerClient client, CompressedFile file)
        {
            string savingDirectory = Path.Combine(Master.mapsPath, client.userFile.Username);
            if (!Directory.Exists(savingDirectory)) Directory.CreateDirectory(savingDirectory);
            Serializer.ObjectBytesToFile(Path.Combine(savingDirectory, file.Instructions + fileExtension), file);

            Logger.Message($"[Save map] > {client.userFile.Username} > {file.Instructions}");
        }

        public static void DeleteMap(string path)
        {
            File.Delete(path);

            Logger.Warning($"[Remove map] > {Path.GetFileNameWithoutExtension(path)}");
        }

        public static string[] GetAllMaps()
        {
            return Directory.GetFiles(Master.mapsPath, "*.mpmap", SearchOption.AllDirectories);
        }

        public static bool CheckIfMapExists(int mapTileToCheck)
        {
            string toFind = GetAllMaps().FirstOrDefault(fetch => Path.GetFileNameWithoutExtension(fetch) == mapTileToCheck.ToString());
            if (toFind != null) return true;
            else return false;
        }

        public static string[] GetAllMapsFromUsername(string username)
        {
            return Directory.GetFiles(Path.Combine(Master.mapsPath, username));
        }

        public static CompressedFile GetUserMapFromTile(string username, int mapTileToGet)
        {
            string path = Path.Combine(Master.mapsPath, username, mapTileToGet + fileExtension);
            return Serializer.FileBytesToObject<CompressedFile>(path);
        }
    }
}
