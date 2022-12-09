using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LM35tempAndClock.Classes
{
    public class BaseGraphData
    {
        public int Yaxis { get; set; } = 0;

        public int Yaxis1 { get; set; } = 0;

        public int Xaxis { get; set; } = 0;

        public int[] pointArray { get; set; }

        public Color lineColor { get; set; }

        public int lineSize { get; set; }

        public bool newGraph { get; set; } = true;

        public BaseGraphData() { }

        public BaseGraphData(
            int Yaxis, int Yaxis1, int Xaxis, Color lineColor, int lineSize, bool newGraph)
        {
            this.Yaxis = Yaxis;
            this.Yaxis1 = Yaxis1;
            this.Xaxis = Xaxis;
            this.pointArray = new int[1000]; ;
            this.lineColor = lineColor;
            this.lineSize = lineSize;
            this.newGraph = newGraph;
        }
    }
}
