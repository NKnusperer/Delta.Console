using System.Collections.Generic;
using Delta.Engine;
using Delta.Rendering.Basics.Drawing;
using Delta.Utilities.Datatypes;
using Delta.Rendering.Basics.Fonts;
using Delta.Utilities.Datatypes.Advanced;
using System.Linq;

namespace Delta.Console
{
    /// <summary>
    /// Draw a IPlottable object on the graph
    /// </summary>
    class PlotManager
    {
        #region Private
        /// <summary>
        /// The object thats implement the IPlottable interface to draw a plot
        /// </summary>
        private IPlottable plot;
        /// <summary>
        /// History of plotted data
        /// </summary>
        private List<PlotData> plotData;
        /// <summary>
        /// Font to display infos for the current plot
        /// </summary>
        private Font plotFont;
        #endregion

        #region ctor
        public PlotManager(IPlottable plot)
        {
            // Initialize
            this.plot = plot;
            plotData = new List<PlotData>();
            plotFont = new Font(Font.Default, plot.PlotColor, HorizontalAlignment.Left, VerticalAlignment.Top);
        }
        #endregion

        #region Draw
        public void Draw(Rectangle graphDimension, float fontHeight, int order)
        {
            updateData();
            drawPlot(graphDimension);
            drawInfoText(graphDimension, fontHeight, order);
        }
        #endregion

        #region updateData
        private void updateData()
        {
            // Space to move the to left (0.01f/sec)
            var offset = Time.Delta / 100;

            // Add offset for each item in the collection
            for (int i = 0; i < plotData.Count; i++)
            {
                var item = plotData[i];
                item.RelativeXPosition -= offset;
                plotData[i] = item;
            }

            // Get lates value and add it
            plotData.Add(new PlotData() { Value = plot.UpdatePlot() });
        }
        #endregion

        #region drawPlot
        private void drawPlot(Rectangle graphDimension)
        {
            // Scale values down to fit the graph dimension
            // http://stackoverflow.com/questions/2675196/c-sharp-method-to-scale-values
            float m, c;
            if (plot.UseAutoScale)
            {
                m = (graphDimension.Bottom - graphDimension.Top) / (plotData.Max(x => x.Value) - plotData.Min(x => x.Value));
                c = 0f - plotData.Min(x => x.Value) * m;
            }
            else
            {
                m = (graphDimension.Bottom - graphDimension.Top) / (plot.MaxValue - plot.MinValue);
                c = 0f - plot.MinValue * m;
            }

            for (int i = 0; i < plotData.Count - 1; i++)
            {
                // Check if the item is gone out of the graph
                if (graphDimension.Right + plotData[i].RelativeXPosition < graphDimension.X)
                {
                    // Remove this items
                    plotData.RemoveAt(i);
                    plotData.RemoveAt(i + 1);
                }
                else
                {
                    // Calculate Y position
                    var p1Y = m * plotData[i].Value + c;
                    var p2Y = m * plotData[i + 1].Value + c;

                    // Calculate start and endpoint to draw the line
                    var startPoint = new Point(graphDimension.Right + plotData[i].RelativeXPosition, graphDimension.Bottom - p1Y);
                    var endPont = new Point(graphDimension.Right + plotData[i + 1].RelativeXPosition, graphDimension.Bottom - p2Y);
                    Line.Draw(startPoint, endPont, plot.PlotColor);
                }
            }
        }
        #endregion

        #region drawInfoText
        private void drawInfoText(Rectangle graphDimension, float fontHeight, int order)
        {
            // Render string
            var text = string.Format("{0}: {1} avg: {2} min: {3} max: {4}",
                plot.PlotName,
                plotData.Last().Value.ToString("F4"),
                plotData.Average(x => x.Value).ToString("F4"),
                plotData.Min(x => x.Value).ToString("F4"),
                plotData.Max(x => x.Value).ToString("F4"));

            // Finaly draw text under the graph
            plotFont.Draw(text, new Rectangle(graphDimension.Left, graphDimension.Bottom + (order * fontHeight), ScreenSpace.DrawArea.Width, fontHeight));
        }
        #endregion
    }

    /// <summary>
    /// Helper structur to scroll the plot data, only used by the PlotManager
    /// </summary>
    struct PlotData
    {
        /// <summary>
        /// The value of the plot point
        /// </summary>
        public float Value;
        /// <summary>
        /// The current X position relative to the graphDimension
        /// </summary>
        public float RelativeXPosition;
    }
}