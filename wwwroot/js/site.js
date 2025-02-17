function initializeMap() {
    if (typeof google === 'undefined') {
        setTimeout(initializeMap, 100); // Retry after 100ms if google is not defined
        return;
    }

    const mapOptions = {
        center: { lat: 47.3769, lng: 8.5417 }, // Centered at Zurich
        zoom: 12,
        styles: [
            {
                featureType: "all",
                elementType: "all",
                stylers: [
                    { saturation: -100 }, // Grayscale effect
                    { lightness: 20 }
                ]
            }
        ],
        disableDefaultUI: true, // Optional: Disables user interaction
        draggable: false,
        scrollwheel: false
    };

    const map = new google.maps.Map(document.getElementById("map"), mapOptions);

    window.map = map; // Store map instance globally
}

function drawPathOnMap(coordinates) {
    // Remove grayscale effect
    window.map.setOptions({
        styles: null,
        disableDefaultUI: false,
        draggable: true,
        scrollwheel: true
    });

    const flightPath = new google.maps.Polyline({
        path: coordinates,
        geodesic: true,
        strokeColor: "#FF0000",
        strokeOpacity: 1.0,
        strokeWeight: 2
    });

    flightPath.setMap(window.map);

    // Adjust map center and zoom to fit the path
    const bounds = new google.maps.LatLngBounds();
    coordinates.forEach(coord => {
        bounds.extend(new google.maps.LatLng(coord.lat, coord.lng));
    });
    window.map.fitBounds(bounds);
}
