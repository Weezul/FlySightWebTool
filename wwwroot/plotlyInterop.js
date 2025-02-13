window.plotlyInterop = {
    createChart: function (divId, data, layout) {
        Plotly.newPlot(divId, data, layout);
    }
};