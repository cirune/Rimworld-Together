namespace Shared 
{
    public class SiteConfigFile 
    {
        public string DefName;

        public string RewardDefName;

        public SiteConfigFile(string defName, string rewardDefName)
        {
            DefName = defName;
            RewardDefName = rewardDefName;
        }
    }
}