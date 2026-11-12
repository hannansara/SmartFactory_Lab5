using System.Collections.Generic;

namespace smartFactoryApp.Models
{
    public class Order
    {
        private Dictionary<Parameters.LegoColor, int> bestellt = new Dictionary<Parameters.LegoColor, int>();
        private Dictionary<Parameters.LegoColor, int> geliefert = new Dictionary<Parameters.LegoColor, int>();

        public Order(int yellow, int blue, int red, int green)
        {
            bestellt[Parameters.LegoColor.yellow] = yellow;
            bestellt[Parameters.LegoColor.blue]   = blue;
            bestellt[Parameters.LegoColor.red]    = red;
            bestellt[Parameters.LegoColor.green]  = green;

            geliefert[Parameters.LegoColor.yellow] = 0;
            geliefert[Parameters.LegoColor.blue]   = 0;
            geliefert[Parameters.LegoColor.red]    = 0;
            geliefert[Parameters.LegoColor.green]  = 0;
        }

        // Wie viele wurden bestellt / schon geliefert?
        public int Bestellt(Parameters.LegoColor farbe) => bestellt[farbe];
        public int Geliefert(Parameters.LegoColor farbe) => geliefert[farbe];

        public int GesamtBestellt
        {
            get
            {
                int summe = 0;
                foreach (int zahl in bestellt.Values) summe += zahl;
                return summe;
            }
        }

        public int GesamtGeliefert
        {
            get
            {
                int summe = 0;
                foreach (int zahl in geliefert.Values) summe += zahl;
                return summe;
            }
        }

        public bool IstFertig => GesamtBestellt > 0 && GesamtGeliefert == GesamtBestellt;

        // Ein guter Brick wurde geliefert. true = er wurde gebraucht.
        public bool BrickGeliefert(Parameters.LegoColor farbe)
        {
            if (geliefert[farbe] < bestellt[farbe])
            {
                geliefert[farbe]++;
                return true;
            }

            return false;
        }

        // Welche Farbe fehlt noch? (null = nichts mehr offen)
        public Parameters.LegoColor? NaechsteFarbe()
        {
            foreach (var farbe in bestellt.Keys)
            {
                if (geliefert[farbe] < bestellt[farbe])
                {
                    return farbe;
                }
            }

            return null;
        }
    }
}