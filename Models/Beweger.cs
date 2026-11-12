using System;
using System.Drawing;
using smartFactoryApp.ViewModels;
using static smartFactoryApp.Models.ViewParameters;

namespace smartFactoryApp.Models
{
    public class Beweger
    {
        // ===================== DATEN =====================

        private int step = Parameters.step;          // Schrittweite
        protected Beweger? dockobjekt;               // Brick in der "Hand" (oder null)

        public Point Location { get; set; }          // aktuelle Position
        public Rectangle Borders { get; set; }       // Spielfeld

        // Hat der Roboter gerade einen Brick in der Hand?
        public bool HatBrick => dockobjekt != null;

        // Der zuletzt abgelegte Brick (Factory braucht seine Farbe für die Order)
        public LegoBrick? LetzterBrick { get; private set; }

        protected int Step
        {
            set { step = value; }
        }

        // Hindernisse, gegen die der Roboter nicht fahren darf
        private static readonly Rectangle[] hindernisse =
        {
            RedProductionLineRect,
            YellowProductionLineRect,
            BlueProductionLineRect,
            GreenProductionLineRect,
            MultiColorRect,
            FactoryZonesRect
        };

        public Beweger(Point location, Rectangle borders)
        {
            Location = location;
            Borders = borders;
        }

        // ===================== BEWEGUNG =====================

        public virtual void Bewegen(Parameters.Direction dir)
        {
            // 1. Richtung in Zahlen übersetzen
            int dx = 0;
            int dy = 0;

            switch (dir)
            {
                case Parameters.Direction.up:        dy = -1;           break;
                case Parameters.Direction.down:      dy = 1;            break;
                case Parameters.Direction.left:      dx = -1;           break;
                case Parameters.Direction.right:     dx = 1;            break;
                case Parameters.Direction.upLeft:    dx = -1; dy = -1;  break;
                case Parameters.Direction.upRight:   dx = 1;  dy = -1;  break;
                case Parameters.Direction.downLeft:  dx = -1; dy = 1;   break;
                case Parameters.Direction.downRight: dx = 1;  dy = 1;   break;
            }

            // 2. Unbekannte Richtung: nichts tun
            if (dx == 0 && dy == 0)
            {
                return;
            }

            // 3. Neue Position ausrechnen (noch nicht bewegen)
            Point neu = new Point(Location.X + dx * step, Location.Y + dy * step);

            // 4. Nur bewegen, wenn es erlaubt ist
            if (IstInnerhalb(neu) && !HatKollision(neu))
            {
                Location = neu;
                DockobjektNachziehen();
            }
        }

        // ===================== HILFSMETHODEN =====================

        // Dreht den Punkt (x, y) um den Winkel (in Grad) um den Nullpunkt
        private static PointF Drehen(float x, float y, double winkel)
        {
            double rad = winkel * Math.PI / 180.0;

            float xNeu = (float)(x * Math.Cos(rad) - y * Math.Sin(rad));
            float yNeu = (float)(x * Math.Sin(rad) + y * Math.Cos(rad));

            return new PointF(xNeu, yNeu);
        }

        // Liegt ein Punkt im Spielfeld?
        private bool ImSpielfeld(float x, float y)
        {
            return x >= Borders.Left && x <= Borders.Right &&
                   y >= Borders.Top && y <= Borders.Bottom;
        }

        // Liegt ein Punkt in der Delivery Line? (toleranz = zusätzlicher Rand)
        private static bool InLieferzone(Point punkt, int toleranz)
        {
            return punkt.X >= FactoryZonesRect.Left - toleranz &&
                   punkt.X <= FactoryZonesRect.Right + toleranz &&
                   punkt.Y >= FactoryZonesRect.Top - toleranz &&
                   punkt.Y <= FactoryZonesRect.Bottom + toleranz;
        }

        // ===================== ECKEN DES ROBOTERS =====================

        private static PointF[] RobotEcken(Point position, double winkel)
        {
            // Mittelpunkt und halbe Größe
            float halbB = RobotWidth / 2f;
            float halbH = RobotHeight / 2f;
            float mitteX = position.X + halbB;
            float mitteY = position.Y + halbH;

            // Ecken vom Mittelpunkt aus gesehen (noch nicht gedreht)
            float[] xs = { -halbB,  halbB, halbB, -halbB };
            float[] ys = { -halbH, -halbH, halbH,  halbH };

            // Jede Ecke drehen und zur Mitte addieren
            PointF[] ecken = new PointF[4];

            for (int i = 0; i < 4; i++)
            {
                PointF gedreht = Drehen(xs[i], ys[i], winkel);
                ecken[i] = new PointF(mitteX + gedreht.X, mitteY + gedreht.Y);
            }

            return ecken;
        }

        // ===================== GRENZEN =====================

        private bool IstInnerhalb(Point position)
        {
            // Roboter: alle 4 Ecken müssen im Spielfeld liegen
            if (this is Robot robot)
            {
                PointF[] ecken = RobotEcken(position, robot.Angle);

                foreach (PointF ecke in ecken)
                {
                    if (!ImSpielfeld(ecke.X, ecke.Y))
                    {
                        return false;
                    }
                }

                return true;
            }

            // Alle anderen (Bricks): nur der Punkt selbst
            return ImSpielfeld(position.X, position.Y);
        }

        // ===================== KOLLISION =====================

        private bool HatKollision(Point position)
        {
            // Nur der Roboter kollidiert
            if (this is not Robot robot)
            {
                return false;
            }

            PointF[] ecken = RobotEcken(position, robot.Angle);

            foreach (Rectangle hindernis in hindernisse)
            {
                if (Kollision(ecken, hindernis))
                {
                    return true;
                }
            }

            return false;
        }

        // Berühren sich Roboter und Hindernis? (Separating Axis Theorem)
        private static bool Kollision(PointF[] robot, Rectangle h)
        {
            // Die 4 Ecken des Hindernisses
            PointF[] rechteck =
            {
                new PointF(h.Left, h.Top),
                new PointF(h.Right, h.Top),
                new PointF(h.Right, h.Bottom),
                new PointF(h.Left, h.Bottom)
            };

            // Die 4 Richtungen, aus denen wir schauen
            PointF[] achsen =
            {
                Achse(robot[0], robot[1]),   // senkrecht zu einer Roboterkante
                Achse(robot[1], robot[2]),   // senkrecht zur nächsten Roboterkante
                new PointF(1, 0),            // waagerecht
                new PointF(0, 1)             // senkrecht
            };

            // Gibt es eine Richtung mit Spalt? Dann keine Kollision.
            foreach (PointF achse in achsen)
            {
                if (!Ueberlappt(robot, rechteck, achse))
                {
                    return false;
                }
            }

            return true;
        }

        // Richtung senkrecht zur Linie von a nach b
        private static PointF Achse(PointF a, PointF b)
        {
            return new PointF(-(b.Y - a.Y), b.X - a.X);
        }

        // Überlappen sich zwei Formen, wenn man sie auf die Achse "projiziert"?
        private static bool Ueberlappt(PointF[] form1, PointF[] form2, PointF achse)
        {
            float[] bereich1 = Bereich(form1, achse);
            float[] bereich2 = Bereich(form2, achse);

            // bereich[0] = kleinster Wert, bereich[1] = größter Wert
            return bereich1[1] >= bereich2[0] && bereich2[1] >= bereich1[0];
        }

        // Kleinster und größter Wert einer Form auf der Achse
        private static float[] Bereich(PointF[] form, PointF achse)
        {
            float min = form[0].X * achse.X + form[0].Y * achse.Y;
            float max = min;

            foreach (PointF ecke in form)
            {
                float wert = ecke.X * achse.X + ecke.Y * achse.Y;

                if (wert < min) min = wert;
                if (wert > max) max = wert;
            }

            return new float[] { min, max };
        }

        // ===================== GREIFPUNKT (TCP) =====================

        // Die "Hand": Mitte der Oberkante des Roboters
        private static Point Greifpunkt(Point position, double winkel)
        {
            float mitteX = position.X + RobotWidth / 2f;
            float mitteY = position.Y + RobotHeight / 2f;

            // Hand liegt ohne Drehung direkt über der Mitte
            PointF hand = Drehen(0, -RobotHeight / 2f, winkel);

            return new Point(
                (int)Math.Round(mitteX + hand.X),
                (int)Math.Round(mitteY + hand.Y));
        }

        // ===================== BRICK FÄHRT MIT =====================

        protected void DockobjektNachziehen()
        {
            if (dockobjekt == null)
            {
                return;
            }

            if (this is not Robot robot)
            {
                return;
            }

            dockobjekt.Location = Greifpunkt(Location, robot.Angle);

            if (dockobjekt is LegoBrick brick)
            {
                brick.Angle = robot.Angle;
            }
        }

        // ===================== BRICK AUFNEHMEN =====================

        public bool Kontakt(Beweger objekt)
        {
            // Nur der Roboter kann Bricks aufnehmen
            if (this is not Robot robot)
            {
                return false;
            }

            // Bricks in der Delivery Line werden ignoriert
            if (InLieferzone(objekt.Location, 0))
            {
                return false;
            }

            Point tcp = Greifpunkt(Location, robot.Angle);

            // Ist der Brick höchstens 10 Pixel von der Hand entfernt?
            bool nah = Math.Abs(tcp.X - objekt.Location.X) <= 10 &&
                       Math.Abs(tcp.Y - objekt.Location.Y) <= 10;

            // Aufnehmen: nah, Hand leer, es ist ein Brick, er ist noch frei
            if (nah && dockobjekt == null && objekt is LegoBrick brick && !brick.Gedockt)
            {
                brick.Gedockt = true;
                dockobjekt = brick;
                brick.Location = tcp;
                brick.Angle = robot.Angle;
            }

            return nah;
        }

        // ===================== BRICK ABLEGEN =====================

        public Parameters.UndockResult Undock()
        {
            // Fall 1: Kein Brick in der Hand
            if (dockobjekt is not LegoBrick brick)
            {
                return Parameters.UndockResult.NoBrick;
            }

            // Fall 2: Brick nicht auf der Delivery Line (10 Pixel Toleranz)
            if (!InLieferzone(brick.Location, 10))
            {
                return Parameters.UndockResult.NotOnDeliveryLine;
            }

            // Quality Check: 50 % schlecht, 50 % gut (Zufall)
            if (Random.Shared.Next(2) == 0)
            {
                brick.Quality = Parameters.Quality.bad;
            }
            else
            {
                brick.Quality = Parameters.Quality.good;
            }

            LetzterBrick = brick;

            // Brick freigeben
            brick.Gedockt = false;
            brick.HorizontalNachUndock = true;

            // Senkrechte Bricks werden um 90° gedreht, damit sie waagerecht liegen
            if (brick.Senkrecht)
            {
                brick.Angle = 90;
            }
            else
            {
                brick.Angle = 0;
            }

            // Brick 30 Pixel vom Roboter wegschieben
            if (this is Robot robot)
            {
                PointF versatz = Drehen(0, -30, robot.Angle);

                brick.Location = new Point(
                    brick.Location.X + (int)Math.Round(versatz.X),
                    brick.Location.Y + (int)Math.Round(versatz.Y));
            }

            dockobjekt = null;

            // Fall 3 und 4: Ergebnis zurückgeben
            if (brick.Quality == Parameters.Quality.good)
            {
                return Parameters.UndockResult.SuccessfulGood;
            }

            return Parameters.UndockResult.SuccessfulBad;
        }
    }
}