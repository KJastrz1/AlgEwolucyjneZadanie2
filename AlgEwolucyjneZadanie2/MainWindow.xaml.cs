using AlgEwolucyjneZadanie2.Algorytm;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AlgEwolucyjneZadanie1;

public partial class MainWindow : Window
{
    private Individual? BestIndividual { get; set; }
    private Individual? WorstIndividual { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        this.WindowState = WindowState.Maximized;
        SetDefaultValues();
    }

    private void SetDefaultValues()
    {
        PopulationSizeInput.Text = "30";
        MaxGenerationsInput.Text = "20";
        CrossoverRateInput.Text = "0.7";
        MutationRateInput.Text = "0.1";
        RepeatCountInput.Text = "1000";
    }

    private async void RunButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetParameters(out int populationSize, out int maxGenerations, out double crossoverRate, out double mutationRate, out int repeatCount))
            return;

        ResultsTextBlock.Text = "Ładowanie...";
        var avgFitnessSummary = new double[maxGenerations];
        var maxFitnessSummary = new double[maxGenerations];
        var minFitnessSummary = new double[maxGenerations];
        double bestOverallFitnessSum = 0;
        BestIndividual = null;
        WorstIndividual = null;

        for (int i = 0; i < repeatCount; i++)
        {
            var population = await Task.Run(() =>
            {
                var pop = new Population(populationSize, maxGenerations, mutationRate, crossoverRate);
                pop.Run();
                return pop;
            });

            var currentBestIndividual = population.GetBestIndividual();
            var currentWorstIndividual = population.GetWorstIndividual();

            if (BestIndividual == null || currentBestIndividual.Fitness > BestIndividual.Fitness)
                BestIndividual = currentBestIndividual;

            if (WorstIndividual == null || currentWorstIndividual.Fitness < WorstIndividual.Fitness)
                WorstIndividual = currentWorstIndividual;

            bestOverallFitnessSum += currentBestIndividual.Fitness;

            for (int j = 0; j < maxGenerations; j++)
            {
                avgFitnessSummary[j] += population.AverageFitnessHistory[j];
                maxFitnessSummary[j] += population.MaxFitnessHistory[j];
                minFitnessSummary[j] += population.MinFitnessHistory[j];
            }
        }

        var avgFitness = avgFitnessSummary.Select(x => Math.Round(x / repeatCount, 4)).ToList();
        var maxFitness = maxFitnessSummary.Select(x => Math.Round(x / repeatCount, 4)).ToList();
        var minFitness = minFitnessSummary.Select(x => Math.Round(x / repeatCount, 4)).ToList();

        double averageBestFitness = Math.Round(bestOverallFitnessSum / repeatCount, 4);

        ResultsTextBlock.Text = $"Miasta:\n{Cities.ToStringRepresentation()}\n\n" +
                                $"Średnie wyniki dla {repeatCount} powtórzeń algorytmu:\n" +
                                $"Wielkość populacji: {populationSize}, Liczba iteracji: {maxGenerations}, " +
                                $"Wsp. krzyżowania: {crossoverRate}, Wsp. mutacji: {mutationRate}\n" +
                                $"Najlepszy osobnik: {BestIndividual}\n" +
                                $"Najgorszy osobnik: {WorstIndividual}\n" +
                                $"Średnie (z powtórzeń algorytmu) przystosowanie najlepszego osobnika: {averageBestFitness}\n";

        DrawPlots(avgFitness, maxFitness, minFitness);
        DrawGridWithCities(CityCanvasBestRoute, BestIndividual, Brushes.Green);
        DrawGridWithCities(CityCanvasWorstRoute, WorstIndividual, Brushes.Red);
    }

    private void DrawPlots(List<double> avgFitness, List<double> maxFitness, List<double> minFitness)
    {
        DrawPlot(PlotAverage, avgFitness, "Średnie przystosowanie");
        DrawPlot(PlotMax, maxFitness, "Maksymalne przystosowanie");
        DrawPlot(PlotMin, minFitness, "Minimalne przystosowanie");
    }

    private void DrawPlot(PlotView plotView, List<double> fitnessHistory, string title)
    {
        var plotModel = new PlotModel { Title = title };
        var lineSeries = new LineSeries();
        for (int i = 0; i < fitnessHistory.Count; i++)
        {
            lineSeries.Points.Add(new DataPoint(i, fitnessHistory[i]));
        }
        plotModel.Series.Add(lineSeries);
        plotView.Model = plotModel;
    }

    private void DrawGridWithCities(Canvas canvas, Individual? individual, Brush routeColor)
    {
        canvas.Children.Clear();
        double cellSize = canvas.Width / 10;

        for (int i = 0; i <= 10; i++)
        {
            double offset = i * cellSize;

            var horizontalLine = new Line
            {
                X1 = 0,
                Y1 = offset,
                X2 = canvas.Width,
                Y2 = offset,
                Stroke = Brushes.Black,
                StrokeThickness = 0.5
            };
            canvas.Children.Add(horizontalLine);

            var verticalLine = new Line
            {
                X1 = offset,
                Y1 = 0,
                X2 = offset,
                Y2 = canvas.Height,
                Stroke = Brushes.Black,
                StrokeThickness = 0.5
            };
            canvas.Children.Add(verticalLine);
        }

        var cityNames = "ABCDEFGHIJ";
        for (int i = 0; i < Cities.Coordinates.Count; i++)
        {
            var city = Cities.Coordinates[i];
            double x = city.x * cellSize;
            double y = city.y * cellSize;

            var cityPoint = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = Brushes.Red
            };

            Canvas.SetLeft(cityPoint, x - cityPoint.Width / 2);
            Canvas.SetTop(cityPoint, y - cityPoint.Height / 2);

            var cityLabel = new TextBlock
            {
                Text = cityNames[i].ToString(),
                Foreground = Brushes.Black,
                FontSize = 12
            };

            Canvas.SetLeft(cityLabel, x + 5);
            Canvas.SetTop(cityLabel, y - 10);

            canvas.Children.Add(cityPoint);
            canvas.Children.Add(cityLabel);
        }

        DrawRoute(canvas, individual, routeColor, cellSize);
    }

    private void DrawRoute(Canvas canvas, Individual? route, Brush color, double cellSize)
    {
        if (route == null) return;

        var coordinates = route.Genes.Select(g => Cities.Coordinates[g]).ToList();
        coordinates.Add(Cities.Coordinates[route.Genes[0]]);

        for (int i = 0; i < coordinates.Count - 1; i++)
        {
            var (x1, y1) = coordinates[i];
            var (x2, y2) = coordinates[i + 1];

            var line = new Line
            {
                X1 = x1 * cellSize,
                Y1 = y1 * cellSize,
                X2 = x2 * cellSize,
                Y2 = y2 * cellSize,
                Stroke = color,
                StrokeThickness = 2
            };

            double t = 0.75;
            double arrowX = line.X1 + t * (line.X2 - line.X1);
            double arrowY = line.Y1 + t * (line.Y2 - line.Y1);

            double dx = line.X2 - line.X1;
            double dy = line.Y2 - line.Y1;
            double length = Math.Sqrt(dx * dx + dy * dy);

            double ux = dx / length;
            double uy = dy / length;

            double arrowSize = 10;

            var arrowHead = new Polygon
            {
                Fill = color,
                Points = new PointCollection
                {
                    new System.Windows.Point(arrowX, arrowY),
                    new System.Windows.Point(arrowX - arrowSize * uy - arrowSize * ux, arrowY + arrowSize * ux - arrowSize * uy),
                    new System.Windows.Point(arrowX + arrowSize * uy - arrowSize * ux, arrowY - arrowSize * ux - arrowSize * uy)
                }
            };

            canvas.Children.Add(line);
            canvas.Children.Add(arrowHead);
        }
    }

    private bool TryGetParameters(out int populationSize, out int maxGenerations, out double crossoverRate, out double mutationRate, out int repeatCount)
    {
        populationSize = 0;
        maxGenerations = 0;
        crossoverRate = 0;
        mutationRate = 0;
        repeatCount = 0;

        if (!int.TryParse(PopulationSizeInput.Text, out populationSize) || populationSize <= 1)
        {
            ResultsTextBlock.Text = "Nieprawidłowa wielkość populacji. Musi być liczbą całkowitą większą od jeden.";
            return false;
        }
        if (!int.TryParse(MaxGenerationsInput.Text, out maxGenerations) || maxGenerations <= 0)
        {
            ResultsTextBlock.Text = "Nieprawidłowa liczba iteracji. Musi być liczbą całkowitą większą od zera.";
            return false;
        }
        if (!double.TryParse(CrossoverRateInput.Text.Replace('.', ','), out crossoverRate) || crossoverRate < 0 || crossoverRate > 1)
        {
            ResultsTextBlock.Text = "Nieprawidłowa wartość współczynnika krzyżowania. Musi być liczbą w zakresie [0, 1].";
            return false;
        }
        if (!double.TryParse(MutationRateInput.Text.Replace('.', ','), out mutationRate) || mutationRate < 0 || mutationRate > 1)
        {
            ResultsTextBlock.Text = "Nieprawidłowa wartość współczynnika mutacji. Musi być liczbą w zakresie [0, 1].";
            return false;
        }
        if (!int.TryParse(RepeatCountInput.Text, out repeatCount) || repeatCount < 1 || repeatCount > 10000)
        {
            ResultsTextBlock.Text = "Nieprawidłowa liczba powtórzeń. Musi być liczbą całkowitą w zakresie 1-10 000.";
            return false;
        }
        return true;
    }
}
