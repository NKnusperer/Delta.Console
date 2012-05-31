using System.Collections.Generic;
using Delta.Utilities.Datatypes;
using Delta.Engine;
using Delta.Rendering.Basics.Drawing;

namespace Delta.Console
{
    /// <summary>
    /// Draw the graph and manages the plots to draw
    /// </summary>
    class GraphManager
    {
        #region Private
        /// <summary>
        /// Line color for the rows and columns 
        /// </summary>
        private Color gridColor;
        /// <summary>
        /// The dimension of the performance graph
        /// </summary>
        private Rectangle graphDimension;
        /// <summary>
        /// Saves all plots to draw
        /// </summary>
        private List<PlotManager> plots;
        /// <summary>
        /// Quadratic space of the consoleFont
        /// </summary>
        private float fontHeight;
        #endregion

        #region ctror
        public GraphManager()
        {
            // Initialize
            gridColor = new Color(0, 128, 64);
            graphDimension = new Rectangle();
            plots = new List<PlotManager>();

            // Recalculate only when the window size has been changed
            Application.Window.ResizeEvent += () =>
            {
                calculateGridDimension();
            };

            // Initialize calculation
            calculateGridDimension();
        }
        #endregion

        #region Draw
        public void Draw()
        {
            drawGrid();
            drawPlots();
        }
        #endregion

        #region calculateGraphDimension
        private void calculateGridDimension()
        {
            // Re-calculate the quadratic space of the consoleFont height
            fontHeight = ScreenSpace.ToQuadraticSpace(new Size(15f)).Height;

            graphDimension.Left = ScreenSpace.DrawArea.Left + 0.01f;
            graphDimension.Bottom = ScreenSpace.DrawArea.Bottom - ((plots.Count + 1) * 0.01f);
            graphDimension.Width = ScreenSpace.DrawArea.Right / 4;
            graphDimension.Height = 0.1f;
        }
        #endregion

        #region drawGrid
        private void drawGrid()
        {
            // The base rectangle
            Rect.DrawOutline(graphDimension, gridColor);

            // Iterate each column
            for (float i = graphDimension.Top; i < graphDimension.Bottom; i += 0.01f)
            {
                // Draw 10 column with a distance of 0.01f
                var startPoint = new Point(graphDimension.X, i);
                var endPoint = new Point(graphDimension.X + graphDimension.Width, i);
                Line.Draw(startPoint, endPoint, gridColor);
            }
        }
        #endregion

        #region drawPlots
        private void drawPlots()
        {
            for (int i = 0; i < plots.Count; i++)
            {
                plots[i].Draw(graphDimension, fontHeight, i);
            }
        }
        #endregion

        #region AddPlotToGraph
        public void AddPlotToGraph(IPlottable plot)
        {
            plots.Add(new PlotManager(plot));
        }
        #endregion
    }
}