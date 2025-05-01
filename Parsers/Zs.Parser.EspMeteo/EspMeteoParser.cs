using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Zs.Common.Exceptions;
using Zs.Common.Models;
using Zs.Parser.EspMeteo.Models;
using static Zs.Parser.EspMeteo.Models.FaultCodes;

namespace Zs.Parser.EspMeteo;

public sealed class EspMeteoParser
{
    private const StringSplitOptions SplitOptions = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

    public async Task<Models.EspMeteo> ParseAsync(string uri, CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();
        var espMeteoPageHtml = await httpClient.GetStringAsync(uri, cancellationToken);

        EnsureHtmlIsValid(espMeteoPageHtml);

        var sensors = GetSensors(espMeteoPageHtml);
        var espMeteo = new Models.EspMeteo(uri, sensors);

        return espMeteo;
    }

    private void EnsureHtmlIsValid(string? espMeteoPageHtml)
    {
        // TODO: improve validation
        var isValidHtml = espMeteoPageHtml?.Contains("<title>ESPMETEO</title>") == true;

        if (isValidHtml)
            return;

        var fault = new Fault(InvalidEspMeteoPageHtml);
        throw new FaultException(fault);
    }

    internal IReadOnlyList<Sensor> GetSensors(string html)
    {
        var xml = html.RepairXml();

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        var sensorsDiv = xmlDocument.DocumentElement!.ChildNodes[1]!.ChildNodes[2]!.ChildNodes[1]!;

        var sensors = new List<Sensor>();

        using var xmlNodeList = sensorsDiv.ChildNodes;
        var sensorDivEnumerator = xmlNodeList.GetEnumerator();
        while (sensorDivEnumerator.MoveNext())
        {
            var currentNode = (XmlNode)sensorDivEnumerator.Current!;

            var isUnfinished = true;
            while (isUnfinished)
            {
                if (currentNode.Name == "b" && !currentNode.InnerText.StartsWith("No ", StringComparison.InvariantCultureIgnoreCase))
                {
                    var sensor = GetSensor(ref currentNode, sensorDivEnumerator);
                    sensors.Add(sensor);

                    if (currentNode.Name == "b")
                        continue;
                }
                isUnfinished = false;
            }
        }

        return sensors;
    }

    private static Sensor GetSensor(ref XmlNode node, IEnumerator sensorDivEnumerator)
    {
        var sensorName = node.InnerText.TrimEnd(':');
        var parameterRows = new List<string>();

        while (sensorDivEnumerator.MoveNext())
        {
            node = (XmlNode)sensorDivEnumerator.Current!;
            if (node.Name == "b")
                break;

            if (node.NodeType == XmlNodeType.Text)
                parameterRows.Add(node.InnerText);
        }

        var parameters = parameterRows
            .SelectMany(r => r.Split(". ", SplitOptions)
                .Select(ToFloatParameter));

        return new Sensor { Name = sensorName, Parameters = parameters.ToList() };
    }

    private static Parameter ToFloatParameter(string parameter)
    {
        var nameAndValueWithUnit = parameter.Split(':', SplitOptions);
        var name = nameAndValueWithUnit[0].Trim();
        var valueAndUnit = nameAndValueWithUnit[1].Trim('.', ' ').Split(' ', SplitOptions);
        var value = float.Parse(valueAndUnit[0], CultureInfo.InvariantCulture);
        var unit = valueAndUnit[1];

        return new Parameter(name, value, unit);
    }
}
