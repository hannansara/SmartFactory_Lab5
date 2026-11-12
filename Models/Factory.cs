using System;
using System.Collections.Generic;
using System.Drawing;
using static smartFactoryApp.Models.Parameters;

namespace smartFactoryApp.Models;

public class Factory
{
    Robot robot;

    List<ProductionLine> productionLines;

    List<LegoBrick> bricks;

    // Ab so vielen Bricks auf dem Recycling-Band wird zuerst dort abgeholt
    private const int recyclingSchwelle = 2;

    // Aktuelle Bestellung des Kunden (am Anfang leer)
    public Order Order { get; private set; } = new Order(0, 0, 0, 0);


    public double RobotX => robot.Location.X;
    public double RobotY => robot.Location.Y;
    public double RobotAngle => robot.Angle;

    public bool HatBrickInHand => robot.HatBrick;


    public IReadOnlyList<LegoBrick> Bricks => bricks;

    internal IReadOnlyList<ProductionLine> ProductionLines => productionLines;


    public Factory()
    {
        robot = new Robot(ViewParameters.RobotPosition, ViewParameters.Borders);

        productionLines = new List<ProductionLine>();
        bricks = new List<LegoBrick>();

        productionLines.Add(new ProductionLine(ViewParameters.RedProductionLinePosition, LegoColor.red, Direction.down));
        productionLines.Add(new ProductionLine(ViewParameters.YellowProductionLinePosition, LegoColor.yellow, Direction.down));
        productionLines.Add(new ProductionLine(ViewParameters.BlueProductionLinePosition, LegoColor.blue, Direction.left));
        productionLines.Add(new ProductionLine(ViewParameters.GreenProductionLinePosition, LegoColor.green, Direction.right));

        // Die Linien starten leer. Bricks kommen erst durch SetupProductionLines().
    }


    // ==========================================
    // SETUP: Linien mit zufälligen Bricks füllen
    // ==========================================

    public void SetupProductionLines()
    {
        bricks.Clear();

        // Recycling-Band ist wieder leer
        LegoBrick.RecyclingLeeren();

        foreach (var productionLine in productionLines)
        {
            productionLine.Fill();
            bricks.AddRange(productionLine.Bricks);
        }

        CreateOrder();   // neue Bestellung bei jedem Setup
    }


    // ==========================================
    // ORDER: zufällige Bestellung aus 8 Bricks, die es wirklich gibt
    // ==========================================

    public void CreateOrder()
    {
        List<LegoBrick> kopie = ErreichbareBricks();
        int anzahl = Math.Min(8, kopie.Count);

        // Reihenfolge der Enum-Werte: yellow = 0, blue = 1, red = 2, green = 3
        int[] zaehler = new int[4];

        for (int i = 0; i < anzahl; i++)
        {
            int index = Random.Shared.Next(kopie.Count);
            zaehler[(int)kopie[index].Color]++;
            kopie.RemoveAt(index);   // jeden Brick nur einmal ziehen
        }

        Order = new Order(zaehler[0], zaehler[1], zaehler[2], zaehler[3]);
    }

    // Alle Bricks, vor denen der Roboter stehen kann
    private List<LegoBrick> ErreichbareBricks()
    {
        List<LegoBrick> liste = new List<LegoBrick>();

        foreach (var linie in productionLines)
        {
            foreach (var brick in linie.Bricks)
            {
                if (Logistics.IstFrei(linie.AnfahrPose(brick)))
                {
                    liste.Add(brick);
                }
            }
        }

        return liste;
    }


    // ==========================================
    // LOGISTIK: welcher Brick, von wo?
    // ==========================================

    // Wie viele Bricks liegen gerade auf dem Recycling-Band?
    private int AnzahlImRecycling()
    {
        int anzahl = 0;

        foreach (var brick in bricks)
        {
            if (brick.AufRecyclingBand)
            {
                anzahl++;
            }
        }

        return anzahl;
    }

    // Wo muss der Roboter stehen, um einen Brick vom Recycling-Band aufzunehmen?
    private static Pose RecyclingPose(LegoBrick brick)
    {
        // Roboter kommt von oben und schaut nach unten (wie bei den unteren Linien)
        Point hand = new Point(brick.Location.X, ViewParameters.MultiColorRect.Top - Logistics.Abstand);

        return Logistics.PoseFuerHand(hand, 180);
    }

    // Sucht einen Brick der Farbe auf dem Recycling-Band
    private bool SucheImRecycling(LegoColor farbe, out string quelle, out Pose pose)
    {
        foreach (var brick in bricks)
        {
            if (brick.Color == farbe && brick.AufRecyclingBand && !brick.Gedockt)
            {
                Pose kandidat = RecyclingPose(brick);

                if (Logistics.IstFrei(kandidat))
                {
                    quelle = "recycling line";
                    pose = kandidat;
                    return true;
                }
            }
        }

        quelle = "";
        pose = default;
        return false;
    }

    // Sucht einen Brick der Farbe auf den Production Lines
    private bool SucheAufLinien(LegoColor farbe, out string quelle, out Pose pose)
    {
        foreach (var linie in productionLines)
        {
            foreach (var brick in linie.Bricks)
            {
                if (brick.Color == farbe && brick.IstVerfuegbar)
                {
                    Pose kandidat = linie.AnfahrPose(brick);

                    if (Logistics.IstFrei(kandidat))
                    {
                        quelle = $"{linie.Color} production line";
                        pose = kandidat;
                        return true;
                    }
                }
            }
        }

        quelle = "";
        pose = default;
        return false;
    }

    // Ab 2 Bricks auf dem Recycling-Band: zuerst dort suchen.
    // Sonst zuerst auf den Production Lines. Gibt es die Farbe dort nicht, wird die andere Stelle benutzt.
    private bool SuchePickUp(LegoColor farbe, out string quelle, out Pose pose)
    {
        if (AnzahlImRecycling() >= recyclingSchwelle)
        {
            if (SucheImRecycling(farbe, out quelle, out pose))
            {
                return true;
            }

            return SucheAufLinien(farbe, out quelle, out pose);
        }

        if (SucheAufLinien(farbe, out quelle, out pose))
        {
            return true;
        }

        return SucheImRecycling(farbe, out quelle, out pose);
    }

    // Wo liegt ein freier Brick dieser Farbe? (z.B. "blue production line", null = nirgends)
    public string? QuelleMitBrick(LegoColor farbe)
    {
        if (SuchePickUp(farbe, out string quelle, out _))
        {
            return quelle;
        }

        return null;
    }


    // ==========================================
    // GoToPickUp / GoToDelivery: Roboter springt direkt zum Ziel
    // ==========================================

    public bool StarteGoToPickUp()
    {
        var farbe = Order.NaechsteFarbe();

        if (farbe == null || robot.HatBrick)
        {
            return false;
        }

        if (!SuchePickUp(farbe.Value, out _, out Pose pose))
        {
            return false;
        }

        robot.SetzeAuf(pose.Location, pose.Angle);
        return true;
    }

    public bool StarteGoToDelivery()
    {
        // Nur sinnvoll, wenn die Hand einen Brick hält
        if (!robot.HatBrick)
        {
            return false;
        }

        Pose pose = Logistics.DeliveryPose();

        robot.SetzeAuf(pose.Location, pose.Angle);
        return true;
    }


    // ==========================================
    // ROBOTER (von Hand gesteuert)
    // ==========================================

    public void RobotBewegen(Direction dir)
    {
        robot.BewegenRelativ(dir);
        KontaktPruefen();
    }

    public void RobotRotateLeft()
    {
        robot.RotateLeft();
        KontaktPruefen();
    }

    public void RobotRotateRight()
    {
        robot.RotateRight();
        KontaktPruefen();
    }

    public void LegoBricksBewegen()
    {
        foreach (var productionLine in productionLines)
        {
            productionLine.Bewegen();
        }
    }

    private void KontaktPruefen()
    {
        foreach (var brick in bricks)
        {
            robot.Kontakt(brick);
        }
    }


    // ==========================================
    // UNDOCK
    // ==========================================

    public UndockResult Undock()
    {
        UndockResult result = robot.Undock();

        // Guter Brick geliefert: in der Bestellung abhaken
        if (result == UndockResult.SuccessfulGood && robot.LetzterBrick != null)
        {
            Order.BrickGeliefert(robot.LetzterBrick.Color);
        }

        return result;
    }
}