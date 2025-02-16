const updateWeatherTimeoutMs = 30_000;
const pingTimeoutMs = 30_000;

window.onload = function () {
    window.setInterval(updateWeatherDashboard, updateWeatherTimeoutMs);
    window.setInterval(updatePingResult, pingTimeoutMs);
}

function updateWeatherDashboard() {
    if (window.location.pathname.toUpperCase() === "/DASHBOARD")
        $('#weather-dashboard').load('/Dashboard?Handler=WeatherDashboard');
}

function updatePingResult() {
    if (window.location.pathname.toUpperCase() === "/DASHBOARD")
        $('#ping-result').load('/Dashboard?Handler=PingResult');
}
