public class Person
{
    public int Id { get; set; }
    public bool Infected { get; set; } = false;
    public bool Recovered { get; set; } = false;
}

public class DiseaseSpreadSimulation
{
    private Dictionary<int, Person> people;
    private Dictionary<int, List<int>> connections;
    private Random random = new Random();

    public DiseaseSpreadSimulation(Dictionary<int, List<int>> connections)
    {
        this.connections = connections;
        people = new Dictionary<int, Person>();

        foreach (var connection in connections)
        {
            if (!people.ContainsKey(connection.Key))
            {
                people[connection.Key] = new Person { Id = connection.Key };
            }

            foreach (var personId in connection.Value)
            {
                if (!people.ContainsKey(personId))
                {
                    people[personId] = new Person { Id = personId };
                }
            }
        }
    }

    public async Task RunSimulationAsync(double infectionProbability, double recoveryProbability)
    {
        int initialInfected = random.Next(1, people.Count + 1);
        people[initialInfected].Infected = true;
        Console.WriteLine($"Initial infection: Person {initialInfected}");

        bool changesMade;
        do
        {
            changesMade = false;

            List<Task> tasks = new List<Task>();

            foreach (var person in people.Values)
            {
                if (person.Infected && !person.Recovered)
                {
                    tasks.Add(Task.Run(() => ProcessPerson(person, infectionProbability, recoveryProbability)));
                }
            }

            await Task.WhenAll(tasks);

            foreach (var person in people.Values)
            {
                if (person.Infected && !person.Recovered)
                {
                    if (random.NextDouble() < recoveryProbability)
                    {
                        person.Recovered = true;
                        Console.WriteLine($"Person {person.Id} recovered");
                        changesMade = true;
                    }
                }
            }
        }
        while (changesMade);

        Console.WriteLine("Simulation complete.");
        PrintResults();
    }

    private void ProcessPerson(Person person, double infectionProbability, double recoveryProbability)
    {
        foreach (var neighborId in connections[person.Id])
        {
            var neighbor = people[neighborId];

            if (!neighbor.Infected && !neighbor.Recovered)
            {
                if (random.NextDouble() < infectionProbability)
                {
                    neighbor.Infected = true;
                    Console.WriteLine($"Person {neighbor.Id} got infected by person {person.Id}");
                }
            }
        }
    }

    private void PrintResults()
    {
        Console.WriteLine("\nResults:");
        foreach (var person in people.Values)
        {
            Console.WriteLine($"Person {person.Id}: " +
                $"Infected={person.Infected}, " +
                $"Recovered={person.Recovered}");
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var connections = new Dictionary<int, List<int>>
        {
            { 1, new List<int> { 2, 5 } },
            { 2, new List<int> { 1, 3 } },
            { 3, new List<int> { 2, 4 } },
            { 4, new List<int> { 3, 5 } },
            { 5, new List<int> { 1, 4 } }
        };

        var simulation = new DiseaseSpreadSimulation(connections);

        double infectionProbability = 0.3;
        double recoveryProbability = 0.5; 

        await simulation.RunSimulationAsync(infectionProbability, recoveryProbability);
    }
}
