const updateWeatherTimeoutMs = 300_000;
const pingTimeoutMs = 300_000;

window.onload = function () {
    window.setInterval(updateWeatherDashboard, updateWeatherTimeoutMs);
    window.setInterval(updatePingResult, pingTimeoutMs);
}

function updateWeatherDashboard() {
    if (window.location.pathname.toUpperCase() !== '/DASHBOARD')
        return;

    var elementId = 'weather-dashboard';

    fetch('/Dashboard?Handler=WeatherDashboard')
        .then((response) => {
            if (response.ok) {
                return response.text();
            }
            throw new Error('updateWeatherDashboard error!');
        })
        .then((responseHtml) => {
            document.getElementById(elementId).innerHTML = responseHtml;
        })
        .catch((error) => {
            console.error(error);

            document.getElementById(elementId).disable();
        });
}

function updatePingResult() {
    if (window.location.pathname.toUpperCase() !== '/DASHBOARD')
        return;

    var elementId = 'ping-result';

    fetch('/Dashboard?Handler=PingResult')
        .then((response) => {
            if (response.ok) {
                return response.text();
            }
            throw new Error('updatePingResult error!');
        })
        .then((responseHtml) => {
            document.getElementById(elementId).innerHTML = responseHtml;
        })
        .catch((error) => {
            console.error(error);

            document.getElementById(elementId).disable();
        });
}
