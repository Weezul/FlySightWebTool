using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using FlySightWebTool.Data;
using System.Text;

namespace FlySightWebTool.Pages
{
    public partial class Index
    {
        private Track? track;
        private string message = "";
        private int currentIndex;

        public Index()
        {                        
            track = null;
            message = "";
            currentIndex = 0;
        }

        protected override Task OnInitializedAsync()
        {
            return Task.CompletedTask;  
        }

        private async Task HandleFileSelected(InputFileChangeEventArgs e)
        {
            message = "Reading file..."; //Reading file
            this.StateHasChanged();

            var file = e.File;

            if (file == null) 
                throw new Exception($"File object is null");

            StringBuilder fileContent = new StringBuilder();

            try
            {
                using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB

                using var reader = new StreamReader(stream);

                char[] buffer = new char[8192]; // 8 KB buffer
                int bytesRead;

                while ((bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    fileContent.Append(buffer, 0, bytesRead);
                }

                message = "Computing..."; //Calculating
                this.StateHasChanged();

                track = await TrackService.LoadTrackFromFileAsync(fileContent.ToString());

                message = ""; //Done

                if (track.Data.Count == 0)
                {
                    message = "No data";
                    this.StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading file: {file.Name}, {ex.Message}");
            }        
        }

        [JSInvokable]
        public async Task UpdateXAxisValue(int index)
        {
            currentIndex = index;

            if (track != null && index >= 0 && index < track.Data.Count)
            {
                var point = track.Data[index];
                await JSRuntime.InvokeVoidAsync("mapInterop.placeMarker", point.Latitude, point.Longitude);
            }

            this.StateHasChanged();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {            
                await JSRuntime.InvokeVoidAsync("mapInterop.initializeMap");
            }
            else
            {
                if (track == null || track.Data.Count == 0)
                    return;

                var plotlyDatasource = new PlotlyDatasource(track);
                var data = plotlyDatasource.getData();
                var layout = plotlyDatasource.getLayout();

                //Load chart data
                var dotNetObjectReference = DotNetObjectReference.Create(this);
                await JSRuntime.InvokeVoidAsync("plotlyInterop.createChart", "plotlyChart", data, layout, dotNetObjectReference);

                //Load map data
                var coordinates = track.Data.Select(d => new { lat = d.Latitude, lng = d.Longitude }).ToList();
                await JSRuntime.InvokeVoidAsync("mapInterop.drawPathOnMap", coordinates);
            }
        }
    }
}
