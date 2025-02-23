const updateWeatherTimeoutMs = 300_000;
const pingTimeoutMs = 300_000;
const vkActivityTimeoutMs = 10_000;

window.onload = function () {
    window.setInterval(updateWeatherDashboard, updateWeatherTimeoutMs);
    window.setInterval(updatePingResult, pingTimeoutMs);
    window.setInterval(updateVkActivity, vkActivityTimeoutMs);
}

function updateWeatherDashboard() {
    updateDashbordBlock('weather-dashboard', 'WeatherDashboard');
}

function updatePingResult() {
    updateDashbordBlock('ping-result', 'PingResult');
}

function updateVkActivity() {
    updateDashbordBlock('vk-activity', 'VkActivity');
}

function updateDashbordBlock(elementId, handler) {
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
