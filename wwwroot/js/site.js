function initializeMap() {
    const map = new google.maps.Map(document.getElementById("map"), {
        zoom: 8,
        center: { lat: 37.7749, lng: -122.4194 } // Centered at San Francisco
    });

    window.map = map; // Store map instance globally
}

function drawPathOnMap(coordinates) {
    const flightPath = new google.maps.Polyline({
        path: coordinates,
        geodesic: true,
        strokeColor: "#FF0000",
        strokeOpacity: 1.0,
        strokeWeight: 2
    });

    flightPath.setMap(window.map);
}
