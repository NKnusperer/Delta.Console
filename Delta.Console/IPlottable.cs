using Delta.Utilities.Datatypes;

namespace Delta.Console
{
    /// <summary>
    /// Need to be implemented if you want to draw a plot on the graph
    /// </summary>
    public interface IPlottable
    {
        /// <summary>
        /// If true, the min and max value will be the lowest and highest point in the history of visible graph points.
        /// Use this if you don't have a fixed range of valid values like the count of current visible ojects.
        /// MinValue and MaxValue will be ignored.
        /// </summary>
        bool UseAutoScale { get; set; }

        /// <summary>
        /// The lowest possible value that your graph will be have.
        /// Use this for a fixed range like percent (0% to 100%).
        /// Will be ignored if AutoScale is true.
        /// </summary>
        float MinValue { get; set; }

        /// <summary>
        /// The highest possible value that your graph will be have.
        /// Use this for a fixed range like percent (0% to 100%).
        /// Will be ignored if AutoScale is true.
        /// </summary>
        float MaxValue { get; set; }

        /// <summary>
        /// The name of the graph.
        /// Will be used for the caption.
        /// </summary>
        string PlotName { get; set; }

        /// <summary>
        /// The color of the graph and the caption
        /// </summary>
        Color PlotColor { get; set; }

        /// <summary>
        /// Will be called every frame to add a new point to the graph.
        /// It's recommend to return a cached value and update it only every second or something like that.
        /// </summary>
        /// <returns>The latest graph point</returns>
        float UpdatePlot();
    }
}