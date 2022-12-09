using CommunityToolkit.Mvvm.ComponentModel;
using LM35tempAndClock.Classes;
using LM35tempAndClock.Model;
using System.Timers;

namespace LM35tempAndClock.View;

[ObservableObject]
public partial class GraphView
{
    Random random = new Random();

    public int Yaxis = 0;
    public double degrees = 0;
    public int count = 0;
    public int graphHeight = 500;
    public int temperature = 400;

    public TempData tempData { get; set; } = new TempData();


    public GraphView()
    {
        InitializeComponent();

        Loaded += MainPage_Loaded;
    }

    private void MainPage_Loaded(object sender, EventArgs e)
    {
        var timer = new System.Timers.Timer(16);
        timer.Elapsed += new ElapsedEventHandler(DrawNewPointOnGraph);
        timer.Start();
    }
    //(readingValue)
    private void DrawNewPointOnGraph(object sender, ElapsedEventArgs e)
    {

       // temperature = Convert.ToInt32(tempData.LblTemperature);
        var graphicsView = this.LineGraphView;
        var lineGraphDrawable = (LineDrawable)graphicsView.Drawable;

        double angle = Math.PI * degrees++ / 180.0;

        lineGraphDrawable.lineGraphs[0].Yaxis = (int)((graphHeight / 2 * Math.Sin(angle)) + graphHeight / 2);
        lineGraphDrawable.lineGraphs[1].Yaxis = (int)((graphHeight / 2 * Math.Cos(angle)) + graphHeight / 2);

        lineGraphDrawable.lineGraphs[2].Yaxis = temperature;//numberbanana

        //if (temperature < 0)
        //{
        //    temperature = graphHeight;
        //}
        graphicsView.Invalidate();
    }
}