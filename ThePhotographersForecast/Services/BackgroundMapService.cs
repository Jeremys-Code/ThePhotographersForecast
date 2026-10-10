using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.Services
{
    // Generates the map used for the page background
    public class BackgroundMapService : IBackgroundMapService
    {
        private readonly ISession _session;

        public BackgroundMapService(ISession session)
        {
            _session = session;
        }

        public Microsoft.Maui.Controls.Maps.Map CreateMap()
        {
            var map = new Microsoft.Maui.Controls.Maps.Map();

            // If session has a polygon (Coordinates is a NetTopologySuite LinearRing with lon/lat coords), add it as a layer
            try
            {
                var ring = _session?.Coordinates;
                map.AddLogicalChild(ring);
            }
            catch
            {
                // best-effort: do not throw here
            }

            return map;
        }
    }
}

// Convert a NetTopologySuite LinearRing of lon/lat coordinates into a MemoryLayer in EPSG:3857
//private MemoryLayer? CreatePolygonLayer(LinearRing? ring)
//{
//    if (ring == null || ring.IsEmpty)
//        return null;

//    // Convert each coordinate (assume X=lon, Y=lat) to Spherical Mercator
//    var ntsCoords = ring.Coordinates
//        .Select(c =>
//        {
//            var merc = SphericalMercator.FromLonLat(c.X, c.Y);
//            return new NetTopologySuite.Geometries.Coordinate(merc.x, merc.y);
//        })
//        .ToArray();

//    if (ntsCoords.Length < 3)
//        return null;

//    // Ensure ring is closed
//    if (!ntsCoords.First().Equals2D(ntsCoords.Last()))
//    {
//        var list = ntsCoords.ToList();
//        list.Add(new NetTopologySuite.Geometries.Coordinate(ntsCoords.First().X, ntsCoords.First().Y));
//        ntsCoords = list.ToArray();
//    }

//    var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 3857);
//    var shell = geometryFactory.CreateLinearRing(ntsCoords);
//    var polygon = geometryFactory.CreatePolygon(shell);

//    var feature = new GeometryFeature { Geometry = polygon };
//    feature.Styles.Add(new VectorStyle
//    {
//        Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.FromArgb(80, 0, 120, 255)),
//        Outline = new Mapsui.Styles.Pen(Mapsui.Styles.Color.DarkBlue, 2)
//    });

//    return new MemoryLayer
//    {
//        Name = "NWS Polygon",
//        Features = new[] { feature }
//    };
//}
