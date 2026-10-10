namespace ThePhotographersForecast.Models
{
    public class LottieAnimation
    {
        // Version (Bodymovin version)
        public string v { get; set; }
        // Frame rate
        public double fr { get; set; }
        // In point (start frame)
        public double ip { get; set; }
        // Out point (end frame)
        public double op { get; set; }
        // Width in pixels
        public double w { get; set; }
        // Height in pixels
        public double h { get; set; }
        // Name
        public string? nm { get; set; }
        // 3D layer flag (0 = 2D, 1 = 3D)
        public bool? ddd { get; set; }
        // Assets list
        assets?: Asset[];
         // Layers array
        public string[] layers { get; set; }
        // Fonts list
        public List<EmbeddedFont>? fonts { get; set; }
        // Character data for text layers
        public Char[]? chars { get; set; }
        // Markers/named time points
        markers?: Marker[]; 
            // Metadata information
  meta?: Metadata; 
    }


    public class BaseLayer
    {
        // Layer type
        public LayerType ty { get; set; }
        // Layer name
        public string? nm { get; set; }
        // Layer index
        ind: number; 
            // Parent layer index
  parent?: number; 
            // In point (start frame)
  ip: number; 
            // Out point (end frame)
  op: number; 
  st: number; // Start time
  sr?: number; // Time stretch (1 = normal speed)
  ks: Transform; // Transform properties
  ao?: 0 | 1; // Auto-orient along path
  ddd?: 0 | 1; // 3D layer flag
  bm?: BlendMode; // Blend mode
  tt?: MatteMode; // Matte mode
  tp?: number; // Matte parent index
  td?: 0 | 1; // Is matte layer
  hd?: boolean; // Hidden flag
  cl?: string; // CSS class name
  ln?: string; // Layer HTML ID
  ef?: Effect[]; // Effects
  masksProperties?: Mask[]; // Masks
}

    enum LayerType
    {
        Precomp = 0,
        Solid = 1,
        Image = 2,
        Null = 3,
        Shape = 4,
        Text = 5,
        Audio = 6,
        VideoPlaceholder = 7,
        ImageSequence = 8,
        Video = 9,
        ImagePlaceholder = 10,
        Guide = 11,
        Adjustment = 12,
        Camera = 13,
        Light = 14,
        Data = 15,
    }

    enum BlendMode
    {
        Normal = 0,
        Multiply = 1,
        Screen = 2,
        Overlay = 3,
        Darken = 4,
        Lighten = 5,
        ColorDodge = 6,
        ColorBurn = 7,
        HardLight = 8,
        SoftLight = 9,
        Difference = 10,
        Exclusion = 11,
        Hue = 12,
        Saturation = 13,
        Color = 14,
        Luminosity = 15,
    }
    public class ShapeLayer : BaseLayer
    {
        ty: 4; // Shape layer type
            shapes: ShapeElement[]; // Shape elements
    }
