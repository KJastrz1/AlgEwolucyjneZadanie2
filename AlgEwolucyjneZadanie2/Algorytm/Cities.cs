namespace AlgEwolucyjneZadanie2.Algorytm;

public static class Cities
{
    public static List<(int x, int y)> Coordinates = new List<(int, int)>
    {
        (4, 4), (1, 1), (8, 9), (2, 10), (4, 10), (6, 9), (5, 6), (1, 8), (8, 7), (9, 4)
    };

    public static string ToStringRepresentation()
    {
        var cityNames = "ABCDEFGHIJ";
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < Coordinates.Count; i++)
        {
            result.Append($"{cityNames[i]}: ({Coordinates[i].x}, {Coordinates[i].y}) ");
        }
        return result.ToString();
    }
}
