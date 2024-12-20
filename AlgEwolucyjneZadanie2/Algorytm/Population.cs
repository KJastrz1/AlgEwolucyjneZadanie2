namespace AlgEwolucyjneZadanie2.Algorytm;

public class Population
{
    public List<Individual> Individuals { get; private set; }
    public int Size { get; private set; }
    public double MutationRate { get; private set; }
    public double CrossoverRate { get; private set; }
    public int MaxGenerations { get; private set; }
    public List<double> AverageFitnessHistory { get; private set; } = new List<double>();
    public List<double> MaxFitnessHistory { get; private set; } = new List<double>();
    public List<double> MinFitnessHistory { get; private set; } = new List<double>();

    public Population(int size, int maxGenerations, double mutationRate, double crossoverRate)
    {
        Size = size;
        MaxGenerations = maxGenerations;
        MutationRate = mutationRate;
        CrossoverRate = crossoverRate;
        Individuals = new List<Individual>();
        Initialize();
    }

    private void Initialize()
    {
        var cities = Enumerable.Range(0, Cities.Coordinates.Count).ToList();
        Random rand = new Random();
        for (int i = 0; i < Size; i++)
        {
            var shuffledCities = cities.OrderBy(x => rand.Next()).ToList();
            Individuals.Add(new Individual(shuffledCities));
        }
        // individual evaluates itself in constr
        AverageFitnessHistory.Add(Individuals.Average(ind => ind.Fitness));
        MaxFitnessHistory.Add(Individuals.Max(ind => ind.Fitness));
        MinFitnessHistory.Add(Individuals.Min(ind => ind.Fitness));
    }

    public void Evaluate()
    {
        foreach (var individual in Individuals)
        {
            individual.CalculateFitness();
        }
    }

    public void Selection()
    {
        var totalFitness = Individuals.Sum(ind => ind.Fitness);
        var probabilities = Individuals.Select(ind => ind.Fitness / totalFitness).ToList();

        var newPopulation = new List<Individual>();
        for (int i = 0; i < Size; i++)
        {
            newPopulation.Add(RouletteWheelSelect(probabilities));
        }

        Individuals = newPopulation;
    }

    private Individual RouletteWheelSelect(List<double> probabilities)
    {
        double rand = new Random().NextDouble();
        double cumulative = 0.0;
        for (int i = 0; i < probabilities.Count; i++)
        {
            cumulative += probabilities[i];
            if (rand < cumulative)
                return Individuals[i];
        }
        return Individuals.Last();
    }

    public void Crossover()
    {
        var offspring = new List<Individual>();
        Random rand = new Random();

        for (int i = 0; i < Size / 2; i++)
        {
            var parent1 = Individuals[i * 2];
            var parent2 = Individuals[i * 2 + 1];

            if (rand.NextDouble() < CrossoverRate)
            {
                var (child1, child2) = OrderCrossover(parent1, parent2);
                offspring.Add(child1);
                offspring.Add(child2);
            }
            else
            {
                offspring.Add(parent1);
                offspring.Add(parent2);
            }
        }

        Individuals = offspring;
    }

    private (Individual, Individual) OrderCrossover(Individual parent1, Individual parent2)
    {
        Random rand = new Random();
        int size = parent1.Genes.Count;
        int start = rand.Next(size);
        int end = rand.Next(start, size);

        var child1Genes = new List<int>(new int[size]);
        var child2Genes = new List<int>(new int[size]);

        for (int i = start; i <= end; i++)
        {
            child1Genes[i] = parent1.Genes[i];
            child2Genes[i] = parent2.Genes[i];
        }

        FillRemainingGenes(parent2, child1Genes, start, end);
        FillRemainingGenes(parent1, child2Genes, start, end);

        return (new Individual(child1Genes), new Individual(child2Genes));
    }

    private void FillRemainingGenes(Individual donor, List<int> child, int start, int end)
    {
        int size = child.Count;
        int currentIndex = (end + 1) % size;

        foreach (var gene in donor.Genes)
        {
            if (!child.Contains(gene))
            {
                child[currentIndex] = gene;
                currentIndex = (currentIndex + 1) % size;
            }
        }
    }

    public void Mutate()
    {
        foreach (var individual in Individuals)
        {
            individual.Mutate(MutationRate);
        }
    }

    public Individual GetBestIndividual()
    {
        return Individuals.OrderByDescending(i => i.Fitness).First();
    }

    public Individual GetWorstIndividual()
    {
        return Individuals.OrderByDescending(i => i.Fitness).Last();
    }
   
    public void NextGeneration()
    {
        Evaluate();
        AverageFitnessHistory.Add(Individuals.Average(ind => ind.Fitness));
        MaxFitnessHistory.Add(Individuals.Max(ind => ind.Fitness));
        MinFitnessHistory.Add(Individuals.Min(ind => ind.Fitness));
        Selection();
        Crossover();
        Mutate();
    }

    public void Run()
    {
        for (int generation = 0; generation < MaxGenerations; generation++)
        {
            NextGeneration();
        }
    }
}
