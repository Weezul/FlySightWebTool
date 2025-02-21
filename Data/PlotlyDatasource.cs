namespace FlySightWebTool.Data;

public class PlotlyDatasource
{
    private readonly List<TrackLog> _data;
    private double[] FlightTime => _data.Select(t => t.FlightTimeStamp).ToArray();
    private object[]? Data;
    private object? Layout;

    public PlotlyDatasource(List<TrackLog> data)
    {
        _data = data;
    }

    /// <summary>
    /// Get the data series for the chart.
    /// </summary>
    /// <returns>An array of data series objects.</returns>
    public object[] GetData()
    {
        if (Data == null)
        {
            Data = new object[]
            {
                CreateSeries(t => t.Height, "Height (AGL)", "y1", "grey"),
                CreateSeries(t => t.GlideRatio, "Glide Ratio", "y2", "lime"),
                CreateSeries(t => t.VelocityDownKmh, "Speed Vert (km/h)", "y3", "red"),
                CreateSeries(t => t.VelocityGroundKmh, "Speed Ground (km/h)", "y3", "cyan"), // Grouped with Speed Vert
                CreateSeries(t => t.VelocityTotalKmh, "Speed Total (km/h)", "y3", "blue"), // Grouped with Speed Vert
                CreateSeries(t => t.AccelerationDown, "V Accl (m/s²)", "y4", "orange"),
                CreateSeries(t => t.AccelerationGround, "H Accl (m/s²)", "y4", "yellow"), // Grouped with V Accl
                CreateSeries(t => t.AccelerationTotal, "Accl (m/s²)", "y4", "purple") // Grouped with V Accl
            };
        }

        return Data;
    }

    /// <summary>
    /// Get the layout configuration for the chart.
    /// </summary>
    /// <returns>An object representing the layout configuration.</returns>
    public object GetLayout()
    {
        if (Layout == null)
        {
            Layout = new
            {                
                xaxis = new { title = new { text = "Time (s)", font = new { color = "white" } }, tickfont = new { color = "white" } },
                yaxis = CreateYAxis("Height (m)", ".0f", true, "left", 0),
                yaxis2 = CreateYAxis("Glide Ratio", ".1f", true, "left", 0.1, "y"),
                yaxis3 = CreateYAxis("Speed (km/h)", ".0f", true, "right", 1, "y"),
                yaxis4 = CreateYAxis("Acceleration (m/s²)", ".1f", true, "right", 0.9, "y"),
                legend = new
                {
                    orientation = "h", // horizontal layout
                    yanchor = "bottom",
                    y = 1.1, // position above the graph
                    xanchor = "center",
                    x = 0.5, // center horizontally
                    font = new { color = "white" }
                },
                plot_bgcolor = "rgba(0,0,0,0)", // Transparent plot background
                paper_bgcolor = "rgba(0,0,0,0)"  // Transparent paper background
            };
        }

        return Layout;
    }

    /// <summary>
    /// Create a data series for the chart.
    /// </summary>
    /// <param name="valueSelector">A function to select the value from the TrackLog.</param>
    /// <param name="name">The name of the series.</param>
    /// <param name="yAxis">The Y-axis to which the series belongs.</param>
    /// <param name="color">The color of the series.</param>
    /// <param name="hidden">Indicates whether the series should be hidden by default.</param>
    /// <returns>An object representing the data series.</returns>
    private object CreateSeries(Func<TrackLog, double> valueSelector, string name, string yAxis, string color, bool hidden = false)
    {
        var yData = _data.Select(valueSelector).ToArray();

        return new
        {
            x = FlightTime,
            y = yData,
            type = "scatter",
            mode = "lines",
            name = name,
            yaxis = yAxis,
            line = new { shape = "spline", color = color }            
        };
    }

    /// <summary>
    /// Create a Y-axis configuration for the chart.
    /// </summary>
    /// <param name="name">The name of the Y-axis.</param>
    /// <param name="tickFormat">The tick format for the Y-axis.</param>
    /// <param name="showLabels">Indicates whether to show labels on the Y-axis.</param>
    /// <param name="side">The side of the chart where the Y-axis is placed.</param>
    /// <param name="position">The position of the Y-axis.</param>
    /// <param name="overlaying">The overlaying configuration for the Y-axis.</param>
    /// <returns>An object representing the Y-axis configuration.</returns>
    private object CreateYAxis(string name, string tickFormat = ".1f", bool showLabels = false, string side = "left", double position = 0, string? overlaying = null)
    {
        return new
        {
            title = showLabels ? name : "",  // Show title only if labels are enabled
            titlefont = new { color = "White" },
            tickformat = tickFormat,
            side = side,
            position = position,
            showticklabels = showLabels,  // Hide tick labels unless specified
            showgrid = false,
            zeroline = false,
            overlaying = overlaying, // Overlay on the same plot
            tickfont = new { color = "white" }
        };
    }
}