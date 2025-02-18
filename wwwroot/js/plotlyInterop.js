window.plotlyInterop = {
    createChart: function (chartId, data, layout, object) {
        console.info('Plotly chart created');
        Plotly.newPlot(chartId, data, layout);
        
        var chart = document.getElementById(chartId);
        chart.on('plotly_hover', function (eventdata) {
            
            var xValue = eventdata.points[0].x;
            console.log('plotly_hover event triggered, xValue:', xValue);
            
            object.invokeMethodAsync('UpdateXAxisValue', xValue)
                .then(function () {
                    console.info('Successfully invoked .NET method');
                })
                .catch(function (err) {
                    console.error('Error invoking .NET method:', err);
                });
        });
    }
};
