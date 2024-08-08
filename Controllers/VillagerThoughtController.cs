using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[Serializable]
public class VillagerThoughtCondition
{
    public string Name;
    public int Index;
    public string[] Strings;
    public int Timeout = 60;
    public int MaxTimeout = 350;
    public int MinTimeout = 50;
    public virtual bool ShouldShow(Villager villager) { return false; }
    public string GetRandom() => Strings[Index++ % Strings.Length];
}
[Serializable]
public class RandomFarmerThoughts : VillagerThoughtCondition
{
    public RandomFarmerThoughts()
    {
        Strings = new string[]
        {
            "I wonder what lies beyond the forest.",
            "I hear whispers of a dragon in the mountains.",
            "The river is teeming with fish today.",
            "I wish I could afford a new plow.",
            "The fields are alive with the colors of summer.",
            "The forest path is dangerous after dark.",
            "I dream of one day becoming a knight.",
            "The merchants bring news from distant lands.",
            "I hope the next winter is not as harsh as the last.",
            "Another day, another mud puddle.",
            "Who needs riches when you have mud?",
            "Pigs live better than we do.",
            "The king's taxes: adding insult to injury.",
            "Village life: where everyone knows your business.",
            "Fields, fields, and more fields.",
            "Roofs that leak with every raindrop.",
            "Adventure? Try fixing the well.",
            "Gossip spreads faster than the plague.",
            "Seasons change, problems don't.",
            "Medieval dentistry: not recommended.",
            "The village bard's 'talents' are... questionable.",
            "Ale: the real currency of the realm.",
            "Dragons? Try avoiding the landlord.",
            "Herbs from Granny: medieval medicine.",
            "Festivals: a reason to forget the mud.",
            "Friday the 13th: black cats and bad luck.",
            "Weeds grow faster than our fortunes.",
            "Medieval life: hard work, little reward.",
            "Castle envy: mud hut realities.",
            "Village life: where boredom thrives.",
            "Our walls keep out the cold... sometimes.",
            "Who needs dreams when you have chores?",
            "Fields: where dreams go to die.",
            "Beggar's can't be choosers, unless they're the king's tax collectors.",
            "Medieval fashion: who needs sleeves?",
            "Festivals: when drinking ale is a civic duty.",
            "The tavern: where everyone knows your ale preference.",
            "Gossip: the original social network.",
            "Why build castles when you can build debt?",
            "Pigs have it better than us, and they know it.",
            "Children: the only reason to keep going.",
            "The village well: where wishes drown in mud.",
            "Thieves: the only ones with thriving businesses.",
            "Winter: when survival is the only luxury.",
            "The baker's bread: harder than the blacksmith's anvil.",
        };
    }
    public override bool ShouldShow(Villager villager)
    {
        return UnityEngine.Random.Range(0, 2000) <= 2;
    }
}


[Serializable]
public class RetiredThought : VillagerThoughtCondition
{
    public RetiredThought()
    {
        Strings = new string[]
        {
            "I can stop working now, finally.",
            "Retirement is a relief after all those years.",
            "No more early mornings in the fields.",
            "I have time to enjoy the sunset now.",
            "My days of hard labor are behind me.",
            "I can spend more time with my family.",
            "It's peaceful to sit by the fireplace.",
            "I cherish the quiet moments of retirement.",
            "No more deadlines or stress.",
            "I can pursue hobbies I never had time for.",
            "The younger ones can take over now.",
            "I've earned my rest after a lifetime of work.",
            "Time to pass on my knowledge to the next generation.",
            "I'm grateful for the years of hard work.",
            "Watching the village grow is my joy now.",
            "I feel a sense of accomplishment in retirement.",
            "My garden has become my sanctuary.",
            "I appreciate the simple pleasures of life.",
            "Reflecting on a life well-lived brings me peace."
        };
        MinTimeout = 50;
        MaxTimeout = 100;
        Timeout = 10;
    }
    public override bool ShouldShow(Villager villager)
    {
        return villager.IsRetired;
    }
}

public class GettingOlderThought : VillagerThoughtCondition
{
    public GettingOlderThought()
    {
        Strings = new string[]
        {
            "I feel old now.",
            "Am I going to die soon?",
            "My bones hurts with every step.",
            "I remember when I was young and strong.",
            "The years have not been kind to me.",
            "I worry I won't see another harvest.",
            "Time has taken its toll on me.",
            "My eyesight is failing me.",
            "I can no longer work like I used to.",
            "The younger generation has taken over.",
            "I hope my wisdom is still of use.",
            "My hands shake more each day.",
            "Will I see my grandchildren grow up?",
            "My strength fades with each sunrise.",
            "I miss the days of my youth.",
            "How many winters do I have left?",
            "I feel the weight of my years.",
            "My memory isn't what it used to be.",
            "I move slower than ever before.",
            "Old age is a cruel companion."
        };
        Timeout = 10;
        MaxTimeout = 200;
    }
    public override bool ShouldShow(Villager villager)
    {
        if (villager.Age >= GameConfig.VILLAGER_MOVEMENT_SLOWING_STARTING_AGE)
        {
            return true;
        }
        return false;
    }
}
public class NoClothesThought : VillagerThoughtCondition
{
    public NoClothesThought()
    {
        Strings = new string[]
        {
            "I wish we had proper clothes to wear.",
            "Why won't the king help us get new clothes?",
            "Our wardrobes are empty and worn.",
            "Will we ever see clothes again?",
            "Children wonder why they don't have proper clothes.",
            "Even beggars are better clothed than us.",
            "The lack of clothing makes us cold and miserable.",
            "Our prayers for new clothes go unanswered.",
            "Elders recall the days of abundant clothing.",
            "The absence of new clothes puzzles us villagers.",
            "I dream of wardrobes full of new clothes.",
            "This place is officially a clothing-free zone, according to the signs.",
            "Everyone looks so ragged without clothes.",
            "When will we see new clothes in the village?",
            "It's been ages since we had new clothes.",
            "Our clothes are patched beyond recognition.",
            "The cold bites harder without proper clothing.",
        };
        Timeout = 400;
    }

    public override bool ShouldShow(Villager villager)
    {
        return UnityEngine.Random.Range(0, 10) > 8 && villager.Home.Requirements.First(t => t.Stats == Stats.CLOTHES).CurrentAmount == 0;
    }
}

public class NoFishThought : VillagerThoughtCondition
{
    public NoFishThought()
    {
        Strings = new string[]
        {
            "I wish we had fish to eat.",
            "Why is there no fish despite all these waters?",
            "Why won't the king help us get fish?",
            "The rivers and lakes are devoid of life.",
            "Fish used to be abundant here, what happened?",
            "Our meals lack the taste of fresh fish.",
            "Will we ever taste fish again?",
            "I miss when fish was a staple in our diet.",
            "Children ask about fish they've never seen.",
            "Even seagulls find no fish to feast upon.",
            "Fishermen return with empty nets from barren waters.",
            "The absence of fish darkens our meals.",
            "Our prayers for a good catch go unanswered.",
            "Elders recall the abundance of fish in their youth.",
            "The lack of fish puzzles us villagers.",
            "I dream of fish filling our nets.",
            "The river's silence mirrors our longing for fish.",
            "This place is officially a fish-free zone, according to the signs.",
            "The village pond reflects the absence of fishermen.",
            "With so much ocean around, who needs fish, right?"
        };
        Timeout = 300;

    }
    public override bool ShouldShow(Villager villager)
    {
        return UnityEngine.Random.Range(0, 10) > 8 && villager.Home.Requirements.First(t => t.Stats == Stats.FISH).CurrentAmount == 0;
    }
}
public class NoFoodThought : VillagerThoughtCondition
{
    public NoFoodThought()
    {
        Strings = new string[]
        {
            "We need food.",
            "Our bellies are empty.",
            "When will we eat again?",
            "No food today either.",
            "Why is there no food?",
            "Our kids are hungry.",
            "Even the animals are hungry.",
            "Nothing grows in the fields.",
            "Every day is harder without food.",
            "We remember when we had plenty.",
            "Will we ever have enough food?",
            "No food means no joy.",
            "We pray for food.",
            "Old folks talk of better times.",
            "Why is this happening to us?",
            "I dream of full tables.",
            "Hunger hurts us all.",
            "No food, no strength.",
            "The market is empty.",
            "Hunger is tearing us apart."
        };
        MinTimeout = 50;
        MaxTimeout = 100;
        Timeout = 30;
    }

    public override bool ShouldShow(Villager villager)
    {
        return UnityEngine.Random.Range(0, 10) > 6 && villager.Home.Requirements.Where(t => GameConfig.IsFoodStats(t.Stats))
        .All(t => t.CurrentAmount == 0);
    }
}

public class NoGoldThought : VillagerThoughtCondition
{
    public NoGoldThought()
    {
        Strings = new string[]
       {
            "No gold, no future.",
            "This king is going to kill us all.",
            "Our gold stock is empty, what will we do?",
            "We can't keep the fires burning without gold.",
            "The children cry for food, but we have nothing.",
            "Will we survive another week without gold?",
            "No gold means no trade, our village is doomed.",
            "How can we defend ourselves without money?",
            "The crops are failing, and we can't buy seeds.",
            "The town is falling apart with no money for repairs.",
            "I haven't seen a gold coin in months.",
            "The merchants laugh at our poverty.",
            "We used to be a prosperous village.",
            "Now we are just beggars in our own land.",
            "Without gold, even the animals are starving.",
            "Our hopes diminish with each passing day.",
            "The rich get richer, while we suffer in poverty.",
            "The king's taxes have drained us dry.",
            "What will become of us with no gold?"
       };
        Timeout = 500;
    }
    public override bool ShouldShow(Villager villager)
    {
        return GameController.Stats.Get(Stats.GOLD) <= 15;
    }
}


public class VillagerThoughtController : MonoBehaviour
{
    [SerializeField]
    private List<VillagerThoughtCondition> m_conditions = new List<VillagerThoughtCondition>();
    public static VillagerThoughtController Instance;
    public void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        m_conditions.Add(new RetiredThought() { Name = typeof(RetiredThought).ToString() });
        m_conditions.Add(new GettingOlderThought() { Name = typeof(GettingOlderThought).ToString() });
        m_conditions.Add(new NoFishThought() { Name = typeof(NoFishThought).ToString() });
        m_conditions.Add(new NoClothesThought() { Name = typeof(NoClothesThought).ToString() });
        m_conditions.Add(new NoFoodThought() { Name = typeof(NoFoodThought).ToString() });
        m_conditions.Add(new NoGoldThought() { Name = typeof(NoGoldThought).ToString() });
        GameController.Instance.OnGameTick += OnGameTick;
    }
    void OnDestroy()
    {
        GameController.Instance.OnGameTick -= OnGameTick;
    }

    private void OnGameTick(int obj)
    {
        if (obj % GameConfig.VILLAGER_THOUGHT_TICK != 0)
            return;

        foreach (var condition in m_conditions)
        {
            condition.Timeout--;
            if (condition.Timeout > 0) continue;
            foreach (var v in VillagerController.Villagers)
            {
                if (v.HasLabel) continue;
                if (v.Home?.Tick <= 250) continue;
                if (!v.Spawned || v.Dead) continue;
                if (condition.ShouldShow(v))
                {
                    GameScreen.Instance.ShowLabelAsync(v, condition.GetRandom(), 0.3f);
                    condition.Timeout = UnityEngine.Random.Range(condition.MinTimeout, condition.MaxTimeout);
                    break;
                }
            }
        }
    }
}