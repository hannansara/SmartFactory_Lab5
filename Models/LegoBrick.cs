using System.Drawing;

namespace smartFactoryApp.Models
{
    public class LegoBrick : Beweger
    {
        public Parameters.LegoColor Color { get; }

        // Wird ein Brick aufgenommen, ist er nicht mehr auf dem Recycling-Band
        private bool gedockt;

        public bool Gedockt
        {
            get { return gedockt; }
            set
            {
                gedockt = value;

                if (value)
                {
                    aufRecyclingBand = false;
                }
            }
        }

        public double Angle { get; set; }

        public Parameters.Quality Quality { get; set; }

        public bool HorizontalNachUndock { get; set; }

        // true = Brick steht senkrecht (auf blauer/grüner Linie), false = waagerecht
        public bool Senkrecht { get; set; }

        // true = Brick ist gerade zur Startposition zurückgekehrt.
        // Die ProductionLine setzt ihn dann passend zu den anderen Bricks.
        public bool ZurueckGesetzt { get; set; }

        // Position des Bricks auf seiner Linie (der Roboter fährt davor)
        public Point StartPosition => startPosition;

        // true = Brick liegt noch frei auf seiner Linie
        public bool IstVerfuegbar => !Gedockt && !aufRecyclingBand && !HorizontalNachUndock;

        // true = Brick liegt auf dem Recycling-Band (kann von dort wieder aufgenommen werden)
        public bool AufRecyclingBand => aufRecyclingBand;

        private Point startPosition;

        // Brick liegt auf Recycling-Förderband und bleibt dort stehen
        private bool aufRecyclingBand = false;


        // ===================== RECYCLING-BAND: 4 PLÄTZE =====================

        private const int anzahlPlaetze = 4;

        // Welcher Brick liegt auf welchem Platz? (null = Platz frei)
        private static LegoBrick?[] recyclingBelegung = new LegoBrick?[anzahlPlaetze];

        // Alle Plätze leeren (bei jedem neuen Setup)
        public static void RecyclingLeeren()
        {
            for (int i = 0; i < anzahlPlaetze; i++)
            {
                recyclingBelegung[i] = null;
            }
        }

        // Erster freier Platz. -1 = Recycling-Band ist voll.
        // Ein Platz ist frei, wenn kein Brick mehr darauf liegt (z.B. vom Roboter abgeholt).
        private static int FreierPlatz()
        {
            for (int i = 0; i < anzahlPlaetze; i++)
            {
                LegoBrick? brick = recyclingBelegung[i];

                if (brick == null || !brick.aufRecyclingBand)
                {
                    return i;
                }
            }

            return -1;
        }


        public LegoBrick(Point location, Parameters.LegoColor color)
            : base(location, ViewParameters.BrickBorders)
        {
            Color = color;
            Gedockt = false;
            Angle = 0;
            Quality = Parameters.Quality.unknown;
            HorizontalNachUndock = false;
            aufRecyclingBand = false;
            ZurueckGesetzt = false;
            startPosition = location;
        }


        public override void Bewegen(Parameters.Direction dir)
        {
            // Gedockter Brick bewegt sich nicht selbst
            if (Gedockt)
            {
                return;
            }

            // Brick liegt auf Recycling-Band: nicht mehr bewegen
            if (aufRecyclingBand)
            {
                return;
            }

            if (HorizontalNachUndock)
            {
                AutomatischBewegen();
                return;
            }

            base.Bewegen(dir);
        }


        private void AutomatischBewegen()
        {
            // GOOD -> zu den Kunden
            if (Quality == Parameters.Quality.good)
            {
                base.Bewegen(Parameters.Direction.right);

                if (Location.X >= ViewParameters.FactoryZonesRect.Right)
                {
                    ZurStartPosition();
                }

                return;
            }

            // BAD -> nach links zum Recycling
            if (Quality == Parameters.Quality.bad)
            {
                base.Bewegen(Parameters.Direction.left);

                // Linke Seite erreicht
                if (Location.X <= ViewParameters.FactoryZonesRect.Left)
                {
                    int platz = FreierPlatz();

                    // Recycling-Band voll (4 Bricks): Brick geht zurück auf seine Linie
                    if (platz < 0)
                    {
                        ZurStartPosition();
                        return;
                    }

                    int abstand = ViewParameters.MultiColorRect.Width / anzahlPlaetze;

                    int x = ViewParameters.MultiColorRect.Left + abstand / 2 + platz * abstand;
                    int y = ViewParameters.MultiColorRect.Top + 5;

                    // Platz belegen
                    recyclingBelegung[platz] = this;

                    // Auf das Recycling-Förderband setzen
                    Location = new Point(x, y);

                    // Senkrechte Bricks um 90° drehen, damit sie waagerecht liegen
                    if (Senkrecht)
                    {
                        Angle = 90;
                    }
                    else
                    {
                        Angle = 0;
                    }

                    Quality = Parameters.Quality.unknown;
                    HorizontalNachUndock = false;
                    Gedockt = false;

                    // Brick bleibt auf dem Recycling-Band stehen
                    aufRecyclingBand = true;
                }

                return;
            }
        }


        // Brick -> zurück zur Originalposition auf seiner Linie
        private void ZurStartPosition()
        {
            Location = startPosition;
            Quality = Parameters.Quality.unknown;
            HorizontalNachUndock = false;
            Gedockt = false;
            aufRecyclingBand = false;
            Angle = 0;

            // Die ProductionLine setzt ihn gleich passend zu den anderen Bricks
            ZurueckGesetzt = true;
        }


        public override string ToString()
        {
            return $"{Color}";
        }
    }
}