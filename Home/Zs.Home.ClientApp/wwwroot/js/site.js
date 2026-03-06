const updateWeatherTimeoutMs = 300_000;
const pingTimeoutMs = 300_000;
const vkActivityTimeoutMs = 10_000;

window.onload = function () {
    window.setInterval(updateWeatherDashboard, updateWeatherTimeoutMs);
    window.setInterval(updatePingResult, pingTimeoutMs);
    window.setInterval(updateVkActivity, vkActivityTimeoutMs);

    updateWeatherDashboard();
    updatePingResult();
    updateVkActivity();
}

function updateWeatherDashboard() {
    updateDashboardBlock('weather-dashboard', 'WeatherDashboard');
}

function updatePingResult() {
    updateDashboardBlock('ping-result', 'PingResult');
}

function updateVkActivity() {
    updateDashboardBlock('vk-activity', 'VkActivity');
}

function updateDashboardBlock(elementId, handler) {
    if (window.location.pathname.toUpperCase() !== '/DASHBOARD')
        return;

    fetch(`/Dashboard?Handler=${handler}`)
        .then((response) => {
            if (response.ok) {
                return response.text();
            }
            throw new Error(`update${handler} error!`);
        })
        .then((responseHtml) => {
            document.getElementById(elementId).innerHTML = responseHtml;
        })
        .catch((error) => {
            console.error(error);

            document.getElementById(elementId).disable();
        });
}
