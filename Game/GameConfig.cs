using System.Linq;
using UnityEngine;

public static class GameConfig
{
    [Category("General")]
    #region GENERAL
    [UsefullInfo("General game ticks per second. Controls everything.", isCheatToChange: true)]
    public static float GAME_TICK_PER_SECONDS = 0.5f;

    [UsefullInfo("Which tick a day will pass", isCheatToChange: false)]
    public static int DAY_CHANGE_PER_TICK = 50;

    [UsefullInfo("Enable cheats by setting this to true.")]
    public static bool CHEATS_ENABLED = Application.isEditor;

    [UsefullInfo("Speed of researching new building", isCheatToChange: true)]
    public static float RESEARCH_PER_TICK = 1f;

    [UsefullInfo("How much you will get when destroying a  place", isCheatToChange: true)]
    public static float REFUND_PERCENT = 50;

    [UsefullInfo("Start date of your game.", isCheatToChange: false)]
    public static int START_YEAR = 1;

    [UsefullInfo("No tutorial", editorOnly: true)]
    public static bool SKIP_TUTORIAL = true;

    [UsefullInfo("If auto save turned off you can save it with SHIFT+S and load with SHIFT+L")]
    public static bool AUTO_SAVE = !Application.isEditor;
    #endregion



    [Category("Villagers")]
    #region VILLAGERS
    [UsefullInfo("Villagers' walking speed.", isCheatToChange: true)]
    public static float VILLAGER_SPEED = 1f;

    [UsefullInfo("Villagers' walking speed while carrying stuff.", isCheatToChange: true)]
    public static float VILLAGER_CARRY_SPEED = VILLAGER_SPEED / 2f;

    [UsefullInfo("Villagers need this amount of food to spawn.", isCheatToChange: true)]
    public static int VILLAGER_SPAWN_FOOD = 25;

    [UsefullInfo("Villagers eat at this tick.", isCheatToChange: true)]
    public static int VILLAGER_REQUEST_NEED_TICK = 4;

    [UsefullInfo("Villagers eat this amount of food when the tick happens.", isCheatToChange: true)]
    public static int VILLAGER_REQEST_CONSUME_TICK = 120;

    [UsefullInfo("A new villager will be spawned at this tick on buildings.", isCheatToChange: true)]
    public static int NEW_VILLAGER_TICK = 10;

    [UsefullInfo("Corpses will decay by this amount per tick.", isCheatToChange: true)]
    public static int CORPSE_DECAY_PER_TICK = 1;

    [UsefullInfo("Villagers will age after this amount of ticks passes.", isCheatToChange: true)]
    public static float AGE_TICK = 60;

    [UsefullInfo("Villagers can die after this age due to old age.", isCheatToChange: true)]
    public static short DEATH_START_AGE = 60;
    #endregion

    #region WORK
    [Category("Work")]

    [UsefullInfo("Villagers and places pay tax after this amount of ticks passes.", isCheatToChange: true)]
    public static int TAX_TICK = 20;

    //[UsefullInfo("Assign work to villagers every this many ticks.", isCheatToChange: true)]  NOT USEFULL
    public static int ASSIGN_WORK_TO_VILLAGERS_TICK = 3;

    [UsefullInfo("Villagers will only work up to this age.", isCheatToChange: true)]
    public static int WORKING_MAX_AGE = 65;

    [UsefullInfo("The distance between houses and workplaces determines whether villagers can work or not. Each tile's width is 2.56f")]
    public static float HOME_WORK_MAX_DISTANCE = 2.56f * 5f; //2.56 is tile size

    [UsefullInfo("Each farmer produce this amount of gold", isCheatToChange: true)]
    public static int FARMER_TAX = 1;

    [UsefullInfo("Each Worker produce this amount of gold", isCheatToChange: true)]
    public static int WORKER_TAX = 3;

    [UsefullInfo("Each Artisan produce this amount of gold", isCheatToChange: true)]
    public static int ARTISAN_TAX = 6;

    [UsefullInfo("Cost of 1 Farmers working", isCheatToChange: true)]
    public static int FARMER_WORK_COST = 2;
    [UsefullInfo("Cost of 1 Worker working", isCheatToChange: true)]
    public static int WORKER_WORK_COST = 5;

    [UsefullInfo("Cost of 1 Artisan working", isCheatToChange: true)]
    public static int ARTISAN_WORK_COST = 12;
    #endregion

    [Category("Fire")]
    #region FIRE
    [UsefullInfo("Fire chance per tick per building.", isCheatToChange: true)]
    public static float FIRE_CHANCE = 1f / 3500f;

    [UsefullInfo("Fire spread chance to nearby buildings per tick.", isCheatToChange: true)]
    public static float FIRE_SPREAD_PERCENTAGE = 4f;

    [UsefullInfo("Villagers will notice fires within this radius.", isCheatToChange: true)]
    public static float FIRE_NOTICE_RADIUS = 5;

    [UsefullInfo("Fire increases by this amount per tick.", isCheatToChange: true)]
    public static float FIRE_SPEED = 0.7f;

    [UsefullInfo("Fire will start spreading after this percentage.", isCheatToChange: true)]
    public static int FIRE_SPREAD_STARTS_AT = 80;

    [UsefullInfo("Minimum number of firefighters required.", isCheatToChange: true)]
    public static int MIN_FIREFIGHTER_COUNT = 3;
    #endregion

    [Category("Editor")]
    #region EDITOR
    [UsefullInfo("Magic Y coordinate for the editor.", editorOnly: true)]
    public static float MAGIC_Y = 1.26f;


    [UsefullInfo("Extra debug info")]
    public static bool DEBUG_INFO = false;

    [UsefullInfo]
    public static string ERROR = "";

    public static int VILLAGER_MOVEMENT_SLOWING_STARTING_AGE = 50;

    public static int VILLAGER_THOUGHT_TICK = 7;

    public static float GOBLIN_SPEED = 1;

    public static int GOBLIN_TICK = 300;

    public static int GetUpgradePrice(WorkTier tier)
    {
        switch (tier)
        {
            case WorkTier.FARMER:
                return 100;
            case WorkTier.WORKER:
                return 250;
            case WorkTier.ARTISAN:
                return 500;
        }
        return int.MaxValue;
    }
    public static int GetTierCost(WorkTier tier, int workerCount)
    {
        switch (tier)
        {
            case WorkTier.FARMER:
                return FARMER_WORK_COST * workerCount;
            case WorkTier.WORKER:
                return WORKER_WORK_COST * workerCount;
            case WorkTier.ARTISAN:
                return ARTISAN_WORK_COST * workerCount;
        }
        return 0;
    }
    public static int GetTax(WorkTier tier)
    {
        switch (tier)
        {
            case WorkTier.FARMER:
                return FARMER_TAX;
            case WorkTier.WORKER:
                return WORKER_TAX;
            case WorkTier.ARTISAN:
                return ARTISAN_TAX;
        }
        return 0;
    }
    public static int GetFoodLevel(Village village)
    {
        int totalFood = 0;
        for (int i = 0; i < village.Requirements.Count; i++)
        {
            var rq = village.Requirements[i];
            if (IsFoodStats(rq.Stats))
            {
                totalFood += rq.CurrentAmount;
            }
        }
        return totalFood;
    }
    public static bool IsFoodStats(Stats stats)
    {
        switch (stats)
        {
            case Stats.POTATO:
            case Stats.EGG:
            case Stats.FISH:
            case Stats.BREAD:
                return true;
        }
        return false;
    }

    #endregion
}
