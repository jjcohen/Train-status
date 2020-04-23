using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SoapConnector.LiveDepartureBoardWebService;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System;

namespace LDBWS
{
    public class Connection
    {
        private LDBServiceSoapClient service;
        private AccessToken accessToken;

        public Connection(Binding binding, EndpointAddress remoteAddress, string token)
        {
            accessToken = new AccessToken();
            accessToken.TokenValue = token;
            service = new LDBServiceSoapClient(binding, remoteAddress);
        }

        private int MinutesUntilTrainArrives(string str)
        {
            var dateTime = DateTime.Parse(str);
            var now = DateTime.Now;
            dateTime = dateTime < now ? dateTime.AddDays(1) : dateTime;
            return (dateTime - now).Minutes;
        }

        public async Task<IEnumerable<int>> GetMinutesUntilTrainsArrive(string fromStation, string toStation)
        {
            var x = await service.GetDepartureBoardAsync(accessToken, 150, fromStation, toStation, FilterType.to, 0, 100);
            var f = x.GetStationBoardResult.trainServices;
            return f.Select(g => MinutesUntilTrainArrives(g.std));
        }

        public async Task<IEnumerable<string>> GetNextDepartureAsync(string fromStation, string toStation)
        { 
            var x = await service.GetNextDeparturesAsync(accessToken, fromStation, new string[] { toStation }, 0, 100);
            return x.DeparturesBoard.departures.Select(g => g.service.std);
        }
    }
}
