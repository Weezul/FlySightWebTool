namespace FlySightWebTool.Data;

public class MapDatasource
{
    private readonly List<TrackLog> _data;

    public MapDatasource(List<TrackLog> data)
    {
        _data = data;
    }

    /// <summary>
    /// Get the coordinate data for the map path
    /// </summary>
    /// <returns>An array of coords objects.</returns>
    public List<object> GetPath()
    {
        return _data.Select(d => new { lat = d.Latitude, lng = d.Longitude }).ToList<Object>();
    }
}