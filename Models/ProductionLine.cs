using System;
using System.Collections.Generic;
using System.Drawing;

namespace smartFactoryApp.Models
{
    internal class ProductionLine
    {
        private Point location;

        private List<LegoBrick> bricks = new List<LegoBrick>();

        private Parameters.Direction side;

        public Point Location { get => location; }
        public Parameters.LegoColor Color { get; }
        public IReadOnlyList<LegoBrick> Bricks => bricks;

        public ProductionLine(Point location, Parameters.LegoColor color, Parameters.Direction side)
        {
            this.location = location;
            Color = color;
            this.side = side;
            // Linie startet leer
        }

        // Das Rechteck der Linie (senkrechte Linien sind hoch, waagerechte breit)
        private Rectangle Rechteck
        {
            get
            {
                bool senkrecht = side == Parameters.Direction.left ||
                                 side == Parameters.Direction.right;

                int breite = senkrecht ? ViewParameters.ProductionLineHeight : ViewParameters.ProductionLineWidth;
                int hoehe  = senkrecht ? ViewParameters.ProductionLineWidth : ViewParameters.ProductionLineHeight;

                return new Rectangle(location.X, location.Y, breite, hoehe);
            }
        }

        // Wo muss der Roboter stehen, um diesen Brick aufzunehmen?
        public Pose AnfahrPose(LegoBrick brick)
        {
            return Logistics.PickUpPose(brick, side, Rechteck);
        }

        // Zufällige Farbe, je 25 %
        private static Parameters.LegoColor RandomColor()
        {
            Parameters.LegoColor[] farben =
            {
                Parameters.LegoColor.red,
                Parameters.LegoColor.yellow,
                Parameters.LegoColor.green,
                Parameters.LegoColor.blue
            };

            return farben[Random.Shared.Next(4)];
        }

        // Linie mit zufälligen Bricks füllen
        public void Fill()
        {
            bricks.Clear();

            // Bewegung der Linie von vorne beginnen
            tickCount = 0;
            versatz = 0;

            int count = Parameters.bricksPerLine;
            int spacing = ViewParameters.ProductionLineWidth / count;

            // Links/rechts-Linien (blau, grün): Bricks stehen senkrecht.
            // Oben/unten-Linien (rot, gelb): Bricks liegen waagerecht.
            bool senkrecht = side == Parameters.Direction.left ||
                             side == Parameters.Direction.right;

            for (int i = 0; i < count; i++)
            {
                Point brickLocation;

                // Linie oben oder unten
                if (side == Parameters.Direction.up ||
                    side == Parameters.Direction.down)
                {
                    int x = location.X + i * spacing + spacing / 2 - 10;
                    int y;

                    if (side == Parameters.Direction.up)
                    {
                        y = location.Y + ViewParameters.ProductionLineHeight - 5;
                    }
                    else
                    {
                        y = location.Y + 5;
                    }

                    brickLocation = new Point(x, y);
                }
                // Linie links oder rechts
                else
                {
                    int y = location.Y + i * spacing + spacing / 2 - 10;
                    int x;

                    if (side == Parameters.Direction.left)
                    {
                        x = location.X + ViewParameters.ProductionLineHeight - 5;
                    }
                    else
                    {
                        x = location.X + 5;
                    }

                    brickLocation = new Point(x, y);
                }

                bricks.Add(new LegoBrick(brickLocation, RandomColor()) { Senkrecht = senkrecht });
            }
        }

        private int tickCount = 0;
        private const int stepsPerDirection = 10;

        // Wie weit die Bricks gerade von ihrer Startposition entfernt sind
        private int versatz = 0;

        public void Bewegen()
        {
            bool outward = tickCount < stepsPerDirection;

            tickCount = (tickCount + 1) % (2 * stepsPerDirection);

            bool waagerecht = side == Parameters.Direction.up ||
                              side == Parameters.Direction.down;

            for (int i = 0; i < bricks.Count; i++)
            {
                // Oben/unten: horizontal bewegen
                if (waagerecht)
                {
                    if (outward)
                    {
                        bricks[i].Bewegen(Parameters.Direction.right);
                    }
                    else
                    {
                        bricks[i].Bewegen(Parameters.Direction.left);
                    }
                }
                // Links/rechts: vertikal bewegen
                else
                {
                    if (outward)
                    {
                        bricks[i].Bewegen(Parameters.Direction.down);
                    }
                    else
                    {
                        bricks[i].Bewegen(Parameters.Direction.up);
                    }
                }
            }

            // Neuen Versatz merken
            if (outward)
            {
                versatz += Parameters.step;
            }
            else
            {
                versatz -= Parameters.step;
            }

            // Zurückgekehrte Bricks passend zu den anderen auf die Linie setzen
            for (int i = 0; i < bricks.Count; i++)
            {
                LegoBrick brick = bricks[i];

                if (!brick.ZurueckGesetzt)
                {
                    continue;
                }

                if (waagerecht)
                {
                    brick.Location = new Point(brick.StartPosition.X + versatz, brick.StartPosition.Y);
                }
                else
                {
                    brick.Location = new Point(brick.StartPosition.X, brick.StartPosition.Y + versatz);
                }

                brick.ZurueckGesetzt = false;
            }
        }
    }
}