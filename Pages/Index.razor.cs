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
                throw new Exception($"Error reading file: {file.Name}, {ex.Message}");
            }
        }

        /// <summary>
        /// Update the X-axis value and place a marker on the map.
        /// </summary>
        /// <param name="index">The index of the data point.</param>
        [JSInvokable]
        public async Task UpdateXAxisValue(int index)
        {
            if (_track != null && index >= 0 && index < _track.Data.Count)
            {
                var point = _track.Data[index];
                await JSRuntime.InvokeVoidAsync("mapInterop.placeMarker", point.Latitude, point.Longitude);
            }

            StateHasChanged();
        }

        /// <summary>
        /// Handle the rendering of the component.
        /// </summary>
        /// <param name="firstRender">Indicates whether this is the first render.</param>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("mapInterop.initializeMap");
            }
            else
            {
                if (_track == null || _track.Data.Count == 0)
                    return;

                var plotlyDatasource = new PlotlyDatasource(_track);
                var data = plotlyDatasource.GetData();
                var layout = plotlyDatasource.GetLayout();

                // Load chart data
                var dotNetObjectReference = DotNetObjectReference.Create(this);
                await JSRuntime.InvokeVoidAsync("plotlyInterop.createChart", "plotlyChart", data, layout, dotNetObjectReference);

                // Load map data
                var coordinates = _track.Data.Select(d => new { lat = d.Latitude, lng = d.Longitude }).ToList();
                await JSRuntime.InvokeVoidAsync("mapInterop.drawPathOnMap", coordinates);
            }
        }
    }
}
