namespace ThePhotographersForecast.Data
{
    public class UsnoData
    {
        public class Rootobject
        {
            public string apiversion { get; set; }
            public int day { get; set; }
            public int month { get; set; }
            public int numphases { get; set; }
            public Phasedata[] phasedata { get; set; }
            public int year { get; set; }
        }

        public class Phasedata
        {
            public int day { get; set; }
            public int month { get; set; }
            public string phase { get; set; }
            public string time { get; set; }
            public int year { get; set; }
        }


    }
}
