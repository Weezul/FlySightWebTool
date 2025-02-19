window.plotlyInterop = {
    // Create a Plotly chart and set up the hover event
    createChart: function (chartId, data, layout, object) {
        console.info('Plotly chart created');
        Plotly.newPlot(chartId, data, layout);
        
        var chart = document.getElementById(chartId);
        chart.on('plotly_hover', function (eventdata) {
            var pointIndex = eventdata.points[0].pointIndex;
            
            object.invokeMethodAsync('UpdateChartCursor', pointIndex)
                .then(function () {
                    console.info('Successfully invoked .NET method');
                })
                .catch(function (err) {
                    console.error('Error invoking .NET method:', err);
                });
        });
    }
};
