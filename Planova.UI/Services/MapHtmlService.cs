using System.Globalization;
using System.Text.Json;

namespace Planova.UI.Services;

public class MapHtmlService
{
    public string GenerateMapHtml(double latitude, double longitude, string projectName)
    {
        var safeName = JsonSerializer.Serialize(projectName);
        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lng = longitude.ToString(CultureInfo.InvariantCulture);
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'/>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'/>
    <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
    <style>
        html, body, #map {{ height: 100%; margin: 0; padding: 0; }}
    </style>
</head>
<body>
    <div id='map'></div>
    <div id='coords' data-lat='{lat}' data-lng='{lng}'></div>
    <script>
        var map = L.map('map').setView([{lat}, {lng}], 15);
        L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
            attribution: '&copy; OpenStreetMap contributors'
        }}).addTo(map);
        var marker = L.marker([{lat}, {lng}]).addTo(map)
            .bindPopup({safeName})
            .openPopup();

        map.on('click', function(e) {{
            var lat = e.latlng.lat.toFixed(6);
            var lng = e.latlng.lng.toFixed(6);
            document.getElementById('coords').dataset.lat = lat;
            document.getElementById('coords').dataset.lng = lng;
            marker.setLatLng(e.latlng);
            marker.setPopupContent('Lat: ' + lat + ', Lng: ' + lng);
            marker.openPopup();
        }});

        function getClickedCoords() {{
            var el = document.getElementById('coords');
            return el.dataset.lat + ',' + el.dataset.lng;
        }}
    </script>
</body>
</html>";
    }
}
