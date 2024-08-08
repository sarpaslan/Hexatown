
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class VillagerFactory
{
    private static System.Random random = new System.Random();
    private static readonly List<string> FirstNames = new List<string>
    {
        // Boy Names
        "James", "John", "Robert", "Michael", "William", "David", "Richard", "Joseph", "Thomas", "Charles",
        "Christopher", "Daniel", "Matthew", "Anthony", "Mark", "Donald", "Steven", "Paul", "Andrew", "Joshua",
        "Kenneth", "Kevin", "Brian", "George", "Edward", "Ronald", "Timothy", "Jason", "Jeffrey", "Ryan",
        "Jacob", "Gary", "Nicholas", "Eric", "Jonathan", "Stephen", "Larry", "Justin", "Scott", "Brandon",
        "Benjamin", "Samuel", "Frank", "Gregory", "Alexander", "Raymond", "Patrick", "Jack", "Dennis", "Jerry",

        "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth", "Barbara", "Susan", "Jessica", "Sarah", "Karen",
        "Nancy", "Lisa", "Betty", "Margaret", "Sandra", "Ashley", "Kimberly", "Emily", "Donna", "Michelle",
        "Dorothy", "Carol", "Amanda", "Melissa", "Deborah", "Stephanie", "Rebecca", "Sharon", "Laura", "Cynthia",
        "Kathleen", "Amy", "Shirley", "Angela", "Helen", "Anna", "Brenda", "Pamela", "Nicole", "Emma",
        "Samantha", "Katherine", "Christine", "Debra", "Rachel", "Catherine", "Carolyn", "Janet", "Ruth", "Maria"
    };

    private static readonly List<string> LastNames = new List<string>
    {
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez",
        "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin",
        "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson",
        "Walker", "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores",
        "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", "Carter", "Roberts"
    };

    public static Villager CreateVillager(PopulationPlace home, Vector3 worldPosition)
    {
        string lastName = string.Empty;
        if (home.Villagers.Count > 0)
        {
            var name = home.Villagers[0].Name;
            if (!string.IsNullOrEmpty(name))
            {
                var names = name.Split(" ");
                lastName = names[names.Length - 1];
            }
        }

        if (string.IsNullOrEmpty(lastName))
            lastName = LastNames[random.Next(LastNames.Count)];

        string firstName = FirstNames[random.Next(FirstNames.Count)];
        GameController.Stats.Add(Stats.POP, 1);
        return new Villager()
        {
            Age = (short)(8 + random.Next(3)),
            Name = $"{firstName} {lastName}",
            Home = (PopulationPlace)home,
            SpawnPosition = worldPosition,
            Spawned = false,
            IsMoving = false,
            CancelJobTokenSource = new CancellationTokenSource(),
            Tier = (home as Village).Tier

        };
    }
}