using OxyPlot;
using OxyPlot.ImageSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitRepoTracker.Plots
{
    public class PlotGenerator
    {
        static byte m_alpha = 150;
        static List<OxyColor> m_oxyPlotColors = new List<OxyColor>
        {
            OxyColor.FromArgb(m_alpha, 0, 112, 188),
            OxyColor.FromArgb(m_alpha, 216, 83, 23),
            OxyColor.FromArgb(m_alpha, 235, 176, 32),
            OxyColor.FromArgb(m_alpha, 125, 46, 142),
            OxyColor.FromArgb(m_alpha, 119, 172, 48),
            OxyColor.FromArgb(m_alpha, 76, 189, 237),
        };

        class DayCommits
        {
            public DateTime First { get; set; }
            public DateTime Last { get; set; }

            public List<Commit> Commits { get; set; } = new List<Commit>();
        }
        public static void UserActivityPlot(List<Commit> commits, string outputFilename)
        {
            try
            {
                commits = commits.FindAll(c => c.Author != "Unknown");

                Random random = new Random();
                PlotModel plot = new PlotModel();
                plot.PlotType = PlotType.XY;
                plot.Axes.Add(new OxyPlot.Axes.LinearAxis()
                { Minimum = 0, Maximum = 1, Position = OxyPlot.Axes.AxisPosition.Left, IsAxisVisible = false });
                plot.Axes.Add(new OxyPlot.Axes.LinearAxis()
                { Minimum = 0, Maximum = 1, Position = OxyPlot.Axes.AxisPosition.Bottom, IsAxisVisible = false });
                OxyPlot.Legends.Legend legend = new OxyPlot.Legends.Legend()
                {
                    //LegendTitle = "Authors",
                    LegendPosition = OxyPlot.Legends.LegendPosition.BottomCenter,
                    LegendPlacement = OxyPlot.Legends.LegendPlacement.Outside,
                    LegendOrientation = OxyPlot.Legends.LegendOrientation.Horizontal,
                    LegendBackground = OxyColor.FromAColor(200, OxyColors.White)
                };
                plot.Legends.Add(legend);

                Dictionary<string, OxyPlot.Series.ScatterSeries> authorSeries = new Dictionary<string, OxyPlot.Series.ScatterSeries>();
                Dictionary<string, double> authorSeriesHeight = new Dictionary<string, double>();
                DateTime start = DateTime.Now, end = new DateTime(2000, 1, 1);
                
                foreach (Commit commit in commits)
                {
                    if (commit.Date < start)
                        start = commit.Date;
                    if (commit.Date > end)
                        end = commit.Date;
                    if (!authorSeries.ContainsKey(commit.Author))
                    {
                        int colorIndex = authorSeries.Count;
                        authorSeries[commit.Author] = new OxyPlot.Series.ScatterSeries()
                        {
                            Background = OxyColors.Transparent,
                            MarkerStrokeThickness = 0,
                            MarkerType = MarkerType.Square,
                            MarkerFill = m_oxyPlotColors[colorIndex],
                            MarkerStroke = m_oxyPlotColors[colorIndex],
                            Title = commit.Author
                        };
                        plot.Series.Add(authorSeries[commit.Author]);
                    }
                }
                int i = 0;
                double yOffset = 0.2;
                double yWidth = 0.6;
                double xOffset = 0.1;
                double xWidth = 0.8;
                foreach (string author in authorSeries.Keys)
                {
                    authorSeriesHeight[author] = yOffset + yWidth * (i / (double)(authorSeries.Count - 1));
                    i++;
                }

                foreach (Commit commit in commits)
                {
                    double y = authorSeriesHeight[commit.Author];
                    double x = xOffset + xWidth * (double)(commit.Date - start).TotalSeconds / (double)(end - start).TotalSeconds;
                    authorSeries[commit.Author].Points.Add(new OxyPlot.Series.ScatterPoint(x, y) { Size = 3 });
                }
                
                plot.InvalidatePlot(true);
                PngExporter.Export(plot, outputFilename, 600, 400);
            }
            catch(Exception ex)
            {
                System.IO.File.WriteAllText("plot-generator-log.txt", ex.ToString());
                Console.WriteLine(ex);
            }
        }

        public static void DeadlinesProgressPlot(List<Commit> commits, List<Evaluation.Deadline> deadlines,
            string outputFilename)
        {
            try
            {
                PlotModel plot = new PlotModel();
                plot.PlotType = PlotType.XY;
                plot.Axes.Add(new OxyPlot.Axes.LinearAxis()
                { Minimum = 0, Maximum = 100, Position = OxyPlot.Axes.AxisPosition.Left, IsAxisVisible = true });
                plot.Axes.Add(new OxyPlot.Axes.LinearAxis()
                { Minimum = 0, Maximum = 1, Position = OxyPlot.Axes.AxisPosition.Bottom, IsAxisVisible = false });
                OxyPlot.Legends.Legend legend = new OxyPlot.Legends.Legend()
                {
                    //LegendTitle = "Test",
                    LegendPosition = OxyPlot.Legends.LegendPosition.BottomCenter,
                    LegendPlacement = OxyPlot.Legends.LegendPlacement.Outside,
                    LegendOrientation = OxyPlot.Legends.LegendOrientation.Horizontal,
                    LegendBackground = OxyColor.FromAColor(200, OxyColors.White)
                };
                plot.Legends.Add(legend);

                int colorIndex = 0;
                List<OxyPlot.Series.LineSeries> series = new List<OxyPlot.Series.LineSeries>();
                foreach (Evaluation.Deadline deadline in deadlines)
                {
                    OxyPlot.Series.LineSeries newSeries = new OxyPlot.Series.LineSeries()
                    {
                        Background = OxyColors.Transparent,
                        Color = m_oxyPlotColors[colorIndex],
                        MarkerStrokeThickness = 0,
                        MarkerFill = m_oxyPlotColors[colorIndex],
                        MarkerStroke = m_oxyPlotColors[colorIndex],
                        Title = deadline.Name
                    };
                    series.Add(newSeries);
                    plot.Series.Add(newSeries);
                    colorIndex++;
                }

                //double yOffset = 10;
                //double yWidth = 0.8;
                double xOffset = 0.1;
                double xWidth = 0.8;
                DateTime start = deadlines.Count > 0 ? deadlines[0].Start : new DateTime(DateTime.Now.Year, 1, 1);
                DateTime end = deadlines.Count > 0 ? deadlines[deadlines.Count - 1].End : new DateTime(DateTime.Now.Year, 12, 31);

                commits.Sort((x, y) => x.Date.CompareTo(y.Date));

                foreach (Commit commit in commits)
                {
                    for (int i = 0; i < deadlines.Count; i++)
                    {
                        if (i < commit.Stats.DeadlineTestsResults.Count)
                        {
                            double x = xOffset + xWidth * (double)(commit.Date - start).TotalSeconds
                                / (double)(end - start).TotalSeconds;
                            double y = //yOffset + yWidth *
                                commit.Stats.DeadlineTestsResults[i].PercentPassed();
                            if (y > 0)
                                series[i].Points.Add(new DataPoint(x, y));
                        }
                    }
                }
                //Add now as a data point
                if (commits.Count > 0)
                {
                    Commit lastCommit = commits[commits.Count - 1];
                    if (lastCommit.Date < DateTime.Now)
                    {
                        for (int i = 0; i < deadlines.Count; i++)
                        {
                            if (i < lastCommit.Stats.DeadlineTestsResults.Count)
                            {
                                double x = xOffset + xWidth * (double)(DateTime.Now - start).TotalSeconds
                                    / (double)(end - start).TotalSeconds;
                                double y = //yOffset + yWidth *
                                    lastCommit.Stats.DeadlineTestsResults[i].PercentPassed();
                                if (y > 0)
                                    series[i].Points.Add(new DataPoint(x, y));
                            }
                        }
                    }
                }


                //Minimums and deadline ends

                colorIndex = 0;
                foreach (Evaluation.Deadline deadline in deadlines)
                {
                    OxyColor color = OxyColor.FromArgb(80, m_oxyPlotColors[colorIndex].R, m_oxyPlotColors[colorIndex].G, m_oxyPlotColors[colorIndex].B);
                    OxyPlot.Series.LineSeries newSeries = new OxyPlot.Series.LineSeries()
                    {
                        Background = OxyColors.Transparent,
                        Color = color,
                        MarkerFill = color,
                        MarkerStroke = color
                    };
                    double deadlineStartX = xOffset + xWidth * (double)(deadline.Start - start).TotalSeconds
                                / (double)(end - start).TotalSeconds;
                    double deadlineEndX = xOffset + xWidth * (double)(deadline.End - start).TotalSeconds
                                / (double)(end - start).TotalSeconds;
                    //newSeries.Points.Add(new DataPoint(deadlineStartX, 80));
                    newSeries.Points.Add(new DataPoint(deadlineEndX, 80));
                    newSeries.Points.Add(new DataPoint(deadlineEndX, 00));
                    plot.Series.Add(newSeries);
                    colorIndex++;
                }

                plot.InvalidatePlot(true);
                PngExporter.Export(plot, outputFilename, 600, 400);
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("plot-generator-log.txt", ex.ToString());
                Console.WriteLine(ex);
            }
        }
    }
}
