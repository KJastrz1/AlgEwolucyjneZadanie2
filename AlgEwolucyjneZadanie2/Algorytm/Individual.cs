namespace AlgEwolucyjneZadanie2.Algorytm;

public class Individual
{
    public List<int> Genes { get; private set; }
    public double Fitness { get; private set; }

    public Individual(List<int> genes)
    {
        Genes = new List<int>(genes);
        CalculateFitness();
    }

    public void CalculateFitness()
    {
        double totalDistance = 0;
        for (int i = 0; i < Genes.Count - 1; i++)
        {
            totalDistance += CalculateDistance(Genes[i], Genes[i + 1]);
        }
        totalDistance += CalculateDistance(Genes[Genes.Count - 1], Genes[0]);
        Fitness = 1 / totalDistance; 
    }

    private double CalculateDistance(int city1, int city2)
    {
        (int x1, int y1) = Cities.Coordinates[city1];
        (int x2, int y2) = Cities.Coordinates[city2];
        return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }

    public void Mutate(double mutationRate)
    {
        Random rand = new Random();
        if (rand.NextDouble() < mutationRate)
        {
            int index1 = rand.Next(Genes.Count);
            int index2 = rand.Next(Genes.Count);
            (Genes[index1], Genes[index2]) = (Genes[index2], Genes[index1]); 
        }
        CalculateFitness();
    }

    public override string ToString()
    {
        var cityNames = "ABCDEFGHIJ";
        var geneRepresentation = string.Join(" -> ", Genes.Select(g => cityNames[g]));
        return $"Route: {geneRepresentation}, Fitness: {Fitness:F4}";
    }
}
