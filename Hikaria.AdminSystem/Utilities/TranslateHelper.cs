using GameData;
using TheArchive.Core.ModulesAPI;

namespace Hikaria.AdminSystem.Utilities
{
    public class TranslateHelper
    {
        public static string EnemyName(uint id)
        {
            if (!EnemyID2NameLookup.TryGetValue(id, out string Name))
            {
                Name = $"{EnemyDataBlock.GetBlock(id).name} [{id}]";
            }
            return Name;
        }

        private static Dictionary<uint, string> EnemyID2NameLookup = new();

        private static CustomSetting<List<EnemyIDNameData>> EnemyIDNames = new("EnemyIDNameLookup", new(), new Action<List<EnemyIDNameData>>((data) =>
        {
            EnemyID2NameLookup.Clear();
            foreach (var item in data)
            {
                foreach (var id in item.IDs)
                {
                    EnemyID2NameLookup.TryAdd(id, item.Name);
                }
            }
        }));

        public class EnemyIDNameData
        {
            public List<uint> IDs { get; set; }

            public string Name { get; set; }
        }
    }
}
