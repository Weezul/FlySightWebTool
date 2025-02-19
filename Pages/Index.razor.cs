using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using FlySightWebTool.Data;
using System.Text;

namespace FlySightWebTool.Pages
{
    public partial class Index
    {
        private Track? _track;
        private string _message;
        private PlotlyDatasource? plotlyDatasource;
        private MapDatasource? mapDatasource;

        public Index()
        {
            _track = null;
            _message = "";
        }

        /// <summary>
        /// Initialize the component.
        /// </summary>
        protected override Task OnInitializedAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handle file selection and read the file content.
        /// </summary>
        /// <param name="e">The file change event arguments.</param>
        private async Task HandleFileSelected(InputFileChangeEventArgs e)
        {
            _message = "Reading file...";
            StateHasChanged();

            var file = e.File;

            if (file == null)
                throw new Exception("File object is null");

            var fileContent = new StringBuilder();

            try
            {
                using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB
                using var reader = new StreamReader(stream);

                var buffer = new char[8192]; // 8 KB buffer
                int bytesRead;

                while ((bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    fileContent.Append(buffer, 0, bytesRead);
                }

                _message = "Computing...";
                StateHasChanged();

                _track = await TrackService.LoadTrackFromFileAsync(fileContent.ToString());

                _message = "";

                if (_track.Data.Count == 0)
                {
                    _message = "No data";
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                _message = ex.Message;
                StateHasChanged();

                Console.WriteLine($"Error reading file: {file.Name}, {ex.Message}");
                throw new Exception($"Error reading file: {file.Name}, {ex.Message}");
            }
        }

        /// <summary>
        /// Update the X-axis value and place a marker on the map.
        /// </summary>
        [JSInvokable]
        public async Task UpdateChartCursor(int index)
        {
            if (_track != null)
            {
                var firstFreeFallIndex = _track.Data.FindIndex(d => d.Phase == FlightPhase.Freefall);
                
                if (_track != null && index >= 0 && firstFreeFallIndex >= 0 && index < _track.Data.Count)
                {
                    var point = _track.Data[firstFreeFallIndex + index];
                    await JSRuntime.InvokeVoidAsync("mapInterop.placeMarker", point.Latitude, point.Longitude);
                }

                //StateHasChanged();
            }
        }

        /// <summary>
        /// Updates tracklog phase to earlier/later exit time
        /// </summary>
        [JSInvokable]
        public async Task AdjustFreefallTrimStart(int seconds)
        {
            if (_track != null)
            {
                await _track.AdjustFreefallTrim(true, seconds);
                loadData();
                //StateHasChanged();
            }
        }

        /// <summary>
        /// Updates tracklog phase to earlier/later exit time
        /// </summary>
        [JSInvokable]
        public async Task AdjustFreefallTrimEnd(int seconds)
        {
            if (_track != null)
            {
                await _track.AdjustFreefallTrim(false, seconds);
                loadData();
                //StateHasChanged();
            }
        }     

        private void loadData()
        {
            if (_track != null)
            {
                plotlyDatasource = new PlotlyDatasource(_track.Data.Where(t => t.Phase == FlightPhase.Freefall).ToList());
                mapDatasource = new MapDatasource(_track.Data.Where(t => t.Phase == FlightPhase.Freefall).ToList());
            }
        }

        /// <summary>
        /// Handle the rendering of the component.
        /// </summary>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {             
                var apiKey = Environment.GetEnvironmentVariable("GoogleMapsApiKey");
                await JSRuntime.InvokeVoidAsync("mapInterop.loadGoogleMapsApi", apiKey);
            }
            else
            {
                if (_track == null || _track.Data.Count == 0)
                    return;

                if (plotlyDatasource == null || mapDatasource == null)
                {
                    loadData();
                }

                // Load chart data      
                if (plotlyDatasource != null)
                {
                    var data = plotlyDatasource.GetData();
                    var layout = plotlyDatasource.GetLayout();

                    var dotNetObjectReference = DotNetObjectReference.Create(this);
                    await JSRuntime.InvokeVoidAsync("plotlyInterop.createChart", "plotlyChart", data, layout, dotNetObjectReference);
                }

                // Load map data
                if (mapDatasource != null)
                {
                    var coordinates = mapDatasource.GetPath();
                    await JSRuntime.InvokeVoidAsync("mapInterop.drawPathOnMap", coordinates);
                }
            }
        }
    }
}
