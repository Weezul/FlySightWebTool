namespace FlySightWebTool.Data;

public class PlotlyDatasource
{
    private Track Track;
    private double[] flightTime => Track.Data.Select(t => t.FlightTimeStamp).ToArray();    

    public PlotlyDatasource(Track track)
    {
        Track = track;
    }

    public object[] getData()
    {
        return new object[]
            {
                CreateSeries(t => t.Height, "Height (AGL)", "y1", "grey"),
                CreateSeries(t => t.GlideRatio, "Glide Ratio", "y2", "lime"),
                CreateSeries(t => t.VelocityDownKmh, "Speed Vert (km/h)", "y3", "red"),
                CreateSeries(t => t.VelocityGroundKmh, "Speed Ground (km/h)", "y3", "cyan"), // Grouped with Speed Vert
                CreateSeries(t => t.VelocityTotalKmh, "Speed Total (km/h)", "y3", "blue"), // Grouped with Speed Vert
                CreateSeries(t => t.AccelerationDown, "V Accl (m/s²)", "y4", "orange", true),
                CreateSeries(t => t.AccelerationGround, "H Accl (m/s²)", "y4", "yellow", true), // Grouped with V Accl
                CreateSeries(t => t.AccelerationTotal, "Accl (m/s²)", "y4", "purple", true) // Grouped with V Accl
            };
    }

    public object getLayout()
    {
        return new
        {
            title = new { text = "Flight Data Visualization", font = new { color = "white" } },
            xaxis = new { title = new { text = "Time (s)", font = new { color = "white" } }, tickfont = new { color = "white" } },
            yaxis = CreateYAxis("Height (m)", ".0f", true, "left", 0),
            yaxis2 = CreateYAxis("Glide Ratio", ".1f", true, "left", 0.1, "y"),
            yaxis3 = CreateYAxis("Speed (km/h)", ".0f", true, "right", 1, "y"),
            yaxis4 = CreateYAxis("Acceleration (m/s²)", ".1f", true, "right", 0.9, "y"),
            legend = new
            {
                orientation = "h", // horizontal layout
                yanchor = "top",
                //y = -0.5, // position below the graph
                xanchor = "center",
                x = 0.5, // center horizontally
                font = new { color = "white" }
            },
            plot_bgcolor = "rgba(0,0,0,0)", // Transparent plot background
            paper_bgcolor = "rgba(0,0,0,0)"  // Transparent paper background
        };
    }

    public object CreateSeries(Func<TrackLog, double> valueSelector, string name, string yAxis, string color, bool hidden = false)
    {
        var yData = Track.Data.Select(valueSelector).ToArray();

        return new
        {
            x = flightTime,
            y = yData,
            type = "scatter",
            mode = "lines",
            name = name,
            yaxis = yAxis,
            line = new { shape = "spline", color = color }, // Hardcoded color
            visible = hidden ? "legendonly" : "true" // Hide series by default but keep in legend
        };
    }

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