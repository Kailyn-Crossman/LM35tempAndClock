using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LM35tempAndClock.Classes
{
    public class LineDrawable : BaseGraphData, IDrawable
    {
        private const int numberOfGraphs = 3;
        public BaseGraphData[] lineGraphs = new BaseGraphData[numberOfGraphs];

        public LineDrawable() : base()
        {
            lineGraphs[0] = new BaseGraphData
                (
                    Yaxis: 0,
                    Yaxis1: 0,
                    Xaxis: 0,
                    lineColor: Colors.Red,
                    lineSize: 1,
                    newGraph: true
                );

            lineGraphs[1] = new BaseGraphData
                (
                    Yaxis: 0,
                    Yaxis1: 0,
                    Xaxis: 0,
                    lineColor: Colors.Blue,
                    lineSize: 2,
                    newGraph: true
                );

            lineGraphs[2] = new BaseGraphData
                (
                    Yaxis: 0,
                    Yaxis1: 0,
                    Xaxis: 0,
                    lineColor: Colors.DarkGreen,
                    lineSize: 3,
                    newGraph: true
                );
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            Random random = new Random();

            for (int graphIndex = 0; graphIndex < lineGraphs.Length; graphIndex++)
            {
                Rect lineGraphRect = new(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);
                DrawLineGraph(canvas, lineGraphRect, lineGraphs[graphIndex]);
                DrawBarGraph(canvas, lineGraphRect, lineGraphs[graphIndex], graphIndex);
            }
        }

        private void DrawBarGraph(ICanvas canvas, Rect lineGraphRect, BaseGraphData barGraph, int graphNumber)
        {
            int barWidth = 10;
            int lineGraphWidth = 1000;
            int barGraphLocation = lineGraphWidth + barWidth / 2 + graphNumber * barWidth;
            int graphHeight = 500;
            canvas.StrokeSize = barWidth;
            canvas.DrawLine(barGraphLocation, graphHeight, barGraphLocation, barGraph.Yaxis);
        }

        private void DrawLineGraph(ICanvas canvas, RectF dirtyRect, BaseGraphData lineGraph)
        {
            if (lineGraph.Xaxis < 2)
            {
                lineGraph.pointArray[lineGraph.Xaxis] = lineGraph.Yaxis1;
                lineGraph.Xaxis++;
                return;
            }
            else if (lineGraph.Xaxis < 1000)
            {
                lineGraph.pointArray[lineGraph.Xaxis] = lineGraph.Yaxis;
                lineGraph.Xaxis++;
            }
            else
            {
                for (int i = 0; i < 999; i++)
                {
                    lineGraph.pointArray[i] = lineGraph.pointArray[i + 1];
                }
                lineGraph.pointArray[999] = lineGraph.Yaxis;
            }
            for (int i = 0; i < lineGraph.Xaxis - 1; i++)
            {
                canvas.StrokeColor = lineGraph.lineColor;
                canvas.StrokeSize = lineGraph.lineSize;
                canvas.DrawLine(i, lineGraph.pointArray[i], i + 1, lineGraph.pointArray[i + 1]);
            }
        }
    }
}
