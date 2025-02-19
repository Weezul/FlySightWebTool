window.mapInterop = {
    // Load the Google Maps JavaScript API asynchronously
    loadGoogleMapsApi: function (apiKey, callback) {
        if (typeof google !== 'undefined') {
            callback();
            return;
        }

        const script = document.createElement('script');
        script.src = `https://maps.googleapis.com/maps/api/js?key=${apiKey}&callback=initMap&loading=async`;
        script.async = true;
        script.defer = true;
        window.initMap = function() {
            if (typeof callback === 'function') {
                callback();
            }
            mapInterop.initializeMap();
        };
        document.head.appendChild(script);
    },

    // Initialize the Google Map
    initializeMap: function () {
        if (typeof google === 'undefined' || typeof google.maps === 'undefined') {
            setTimeout(mapInterop.initializeMap, 500); // Retry after 500ms if google or google.maps is not defined
            return;
        }

        const mapOptions = {
            center: { lat: 47.3769, lng: 8.5417 }, // Centered at Zurich
            zoom: 12,
            mapTypeId: google.maps.MapTypeId.ROADMAP,
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
            disableDefaultUI: true, // Disables user interaction
            draggable: false,
            scrollwheel: false
        };

        const map = new google.maps.Map(document.getElementById("map"), mapOptions);

        window.map = map; // Store map instance globally
    },

    // Draw the flight path on the map
    drawPathOnMap: function (coordinates) {
        // Remove existing path if any
        if (window.flightPath) {
            window.flightPath.setMap(null);
        }

        // Remove grayscale effect
        window.map.setOptions({
            styles: null,
            disableDefaultUI: false,
            draggable: true,
            scrollwheel: true,
            zoom: 8,
            mapTypeId: google.maps.MapTypeId.HYBRID
        });

        window.flightPath = new google.maps.Polyline({
            path: coordinates,
            geodesic: true,
            strokeColor: "#FF0000",
            strokeOpacity: 1.0,
            strokeWeight: 2
        });

        window.flightPath.setMap(window.map);

        // Adjust map center and zoom to fit the path
        const bounds = new google.maps.LatLngBounds();
        coordinates.forEach(coord => {
            bounds.extend(new google.maps.LatLng(coord.lat, coord.lng));
        });
        window.map.fitBounds(bounds);
    },

    // Place or move a marker on the map
    placeMarker: function (lat, lon) {        
        if (!window.marker) {
            window.marker = new google.maps.Marker({
                position: { lat: lat, lng: lon },
                map: window.map
            });
        } else {
            window.marker.setPosition({ lat: lat, lng: lon });
        }
    }
};

// Ensure initMap is defined globally
window.initMap = window.mapInterop.initializeMap;