using System;
using System.Collections.Generic;
using System.Text;

namespace ThePhotographersForecast.Data
{
    public class AzureMapsSearchResponse
    {
        public List<SearchResult> Results { get; set; }
    }

    public class SearchResult
    {
        public Address Address { get; set; }
        public Position Position { get; set; }
    }

    public class Address
    {
        public string FreeformAddress { get; set; }
    }

    public class Position
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

}
