using StardewValley.GameData;

namespace CraneGameOverhaul {
    public class PrizeDataModel {

        public CommonList? CommonList { get; set; } = null;
        public RareList? RareList { get; set; } = null;
        public DeluxeList? DeluxeList { get; set; } = null;
        public List<ItemField>? LeftSecretPrizes { get; set; } = null;
        public List<ItemField>? RightSecretPrizes { get; set;} = null;
        public List<ItemField>? LeftDecorationPrizes { get; set; } = null;
        public List<ItemField>? RightDecorationPrizes { get; set; } = null;
        
    }

    public class CommonList {
        public List<ItemField>? Prizes { get; set; } = null;
    }

    public class RareList {
        public List<ItemField>? Prizes { get; set; } = null;
    }

    public class DeluxeList {
        public List<ItemField>? Prizes { get; set; } = null;
    }

    public class ItemField : GenericSpawnItemDataWithCondition {



    }
}
