using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using smartFactoryApp.Models;
using static smartFactoryApp.Models.Parameters;

namespace smartFactoryApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    // ===================== DATEN =====================

    private readonly Factory factory = new Factory();

    private readonly DispatcherTimer timer;        // bewegt die LegoBricks
    private readonly DispatcherTimer robotTimer;   // wiederholt die Roboter-Aktion (Move-Modus)

    private enum RobotAktion { Bewegen, LinksDrehen, RechtsDrehen }

    private RobotAktion currentRobotAktion = RobotAktion.Bewegen;
    private Direction currentRobotDirection;

    private bool moveMode = false;   // false = Step-Modus, true = Move-Modus

    public List<ProductionLineViewModel> ProductionLines { get; } = new List<ProductionLineViewModel>();
    // ObservableCollection: die Oberfläche merkt, wenn Bricks dazukommen oder verschwinden
    public ObservableCollection<LegoBrickViewModel> Bricks { get; } = new ObservableCollection<LegoBrickViewModel>();

    // ===================== WERTE FÜR DIE OBERFLÄCHE =====================

    public double RobotX => factory.RobotX;
    public double RobotY => factory.RobotY;
    public double RobotAngle => factory.RobotAngle;

    private string undockMessage = "";

    public string UndockMessage
    {
        get { return undockMessage; }
        set
        {
            undockMessage = value;
            OnPropertyChanged(nameof(UndockMessage));
        }
    }

    // ===================== KONSTRUKTOR =====================

    public MainViewModel()
    {
        foreach (var line in factory.ProductionLines)
        {
            ProductionLines.Add(new ProductionLineViewModel(line));
        }

        foreach (var brick in factory.Bricks)
        {
            Bricks.Add(new LegoBrickViewModel(brick));
        }

        timer = NeuerTimer(Timer_Tick);
        robotTimer = NeuerTimer(RobotTimer_Tick);
    }

    private static DispatcherTimer NeuerTimer(EventHandler tick)
    {
        var neuerTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(timerInterval)
        };

        neuerTimer.Tick += tick;

        return neuerTimer;
    }

    // ===================== HINWEIS-TEXT =====================

    private string hintText = "";

    public string HintText
    {
        get { return hintText; }
        set
        {
            hintText = value;
            OnPropertyChanged(nameof(HintText));
        }
    }

    // ===================== GOTO PICKUP / GOTO DELIVERY =====================

    public void GoToPickUp()
    {
        // Text aktualisieren: Er muss zum Ziel passen
        NotifyStepText();

        if (factory.HatBrickInHand)
        {
            HintText = "Hand is full: bring the brick to delivery first.";
            return;
        }

        if (factory.StarteGoToPickUp())
        {
            HintText = "Robot at the brick: drive forward to dock the brick.";
            NotifyRobotPositionChanged();
            NotifyLegoBricksPositionChanged();
        }
        else
        {
            HintText = "No brick to pick up. Press 'Setup Production Lines' first.";
        }
    }

    public void GoToDelivery()
    {
        if (factory.StarteGoToDelivery())
        {
            HintText = "Robot at the delivery line: press Undock.";
            NotifyRobotPositionChanged();
            NotifyLegoBricksPositionChanged();
        }
        else
        {
            HintText = "No brick in the hand. Pick up a brick first.";
        }
    }

    // ===================== SETUP PRODUCTION LINES =====================

    // Setup ist nur möglich, wenn keine Order läuft (noch keine Order oder Order fertig)
    public bool SetupAktiv => factory.Order.GesamtBestellt == 0 || factory.Order.IstFertig;

    public void SetupProductionLines()
    {
        // Läuft gerade eine Order, passiert nichts
        if (!SetupAktiv)
        {
            return;
        }

        HintText = "";

        // 1. Factory füllt die Linien mit zufälligen Bricks und macht eine neue Bestellung
        factory.SetupProductionLines();

        // 2. Anzeige neu aufbauen
        Bricks.Clear();

        foreach (var brick in factory.Bricks)
        {
            Bricks.Add(new LegoBrickViewModel(brick));
        }

        NotifyOrderChanged();
    }

    // ===================== ORDER =====================

    public int YellowOrdered => factory.Order.Bestellt(LegoColor.yellow);
    public int BlueOrdered   => factory.Order.Bestellt(LegoColor.blue);
    public int RedOrdered    => factory.Order.Bestellt(LegoColor.red);
    public int GreenOrdered  => factory.Order.Bestellt(LegoColor.green);

    public int YellowDelivered => factory.Order.Geliefert(LegoColor.yellow);
    public int BlueDelivered   => factory.Order.Geliefert(LegoColor.blue);
    public int RedDelivered    => factory.Order.Geliefert(LegoColor.red);
    public int GreenDelivered  => factory.Order.Geliefert(LegoColor.green);

    public string StepText
    {
        get
        {
            var order = factory.Order;

            if (order.GesamtBestellt == 0)
            {
                return "Press 'Setup Production Lines' to start.";
            }

            if (order.IstFertig)
            {
                return "Order complete!";
            }

            var farbe = order.NaechsteFarbe();

            if (farbe == null)
            {
                return "Order complete!";
            }

            string schritt = $"Step {order.GesamtGeliefert + 1}/{order.GesamtBestellt}: ";

            // Brick schon in der Hand: nur noch zur Delivery Line
            if (factory.HatBrickInHand)
            {
                return schritt + "Bring the brick to the delivery line";
            }

            // Wo liegt wirklich ein Brick dieser Farbe? (Recycling Line oder Production Line)
            string? quelle = factory.QuelleMitBrick(farbe.Value);

            string quellText = quelle ?? "a production line";

            return schritt + $"Pick {farbe} brick from {quellText} and bring to delivery";
        }
    }

    // Nur der Step-Text hat sich geändert (z.B. weil Bricks sich bewegt haben)
    private void NotifyStepText()
    {
        OnPropertyChanged(nameof(StepText));
    }

    // Sagt der Oberfläche: alle Order-Werte haben sich geändert
    private void NotifyOrderChanged()
    {
        OnPropertyChanged(nameof(YellowOrdered));
        OnPropertyChanged(nameof(BlueOrdered));
        OnPropertyChanged(nameof(RedOrdered));
        OnPropertyChanged(nameof(GreenOrdered));

        OnPropertyChanged(nameof(YellowDelivered));
        OnPropertyChanged(nameof(BlueDelivered));
        OnPropertyChanged(nameof(RedDelivered));
        OnPropertyChanged(nameof(GreenDelivered));

        OnPropertyChanged(nameof(SetupAktiv));   // Setup-Button grau oder aktiv

        NotifyStepText();
    }

    // ===================== ROBOTER: 8 RICHTUNGEN =====================

    public void MoveLeft()      => RobotSteuern(Direction.left);
    public void MoveRight()     => RobotSteuern(Direction.right);
    public void MoveUp()        => RobotSteuern(Direction.up);
    public void MoveDown()      => RobotSteuern(Direction.down);
    public void MoveUpLeft()    => RobotSteuern(Direction.upLeft);
    public void MoveUpRight()   => RobotSteuern(Direction.upRight);
    public void MoveDownLeft()  => RobotSteuern(Direction.downLeft);
    public void MoveDownRight() => RobotSteuern(Direction.downRight);

    public void RotateLeft()  => RobotStarten(RobotAktion.LinksDrehen);
    public void RotateRight() => RobotStarten(RobotAktion.RechtsDrehen);

    private void RobotSteuern(Direction direction)
    {
        RobotStarten(RobotAktion.Bewegen, direction);
    }

    // Step-Modus: Aktion einmal ausführen. Move-Modus: Timer wiederholt sie.
    private void RobotStarten(RobotAktion aktion, Direction direction = Direction.up)
    {
        currentRobotAktion = aktion;
        currentRobotDirection = direction;   // wird nur beim Bewegen benutzt

        if (moveMode)
        {
            robotTimer.Start();
        }
        else
        {
            AktionAusfuehren();
        }
    }

    // Führt die aktuelle Aktion genau einmal aus und aktualisiert die Anzeige
    private void AktionAusfuehren()
    {
        switch (currentRobotAktion)
        {
            case RobotAktion.Bewegen:
                factory.RobotBewegen(currentRobotDirection);
                break;

            case RobotAktion.LinksDrehen:
                factory.RobotRotateLeft();
                break;

            case RobotAktion.RechtsDrehen:
                factory.RobotRotateRight();
                break;
        }

        NotifyRobotPositionChanged();
        NotifyLegoBricksPositionChanged();
        NotifyStepText();   // Brick evtl. aufgenommen
    }

    // ===================== MODI =====================

    public void SetStepMode()
    {
        moveMode = false;
        robotTimer.Stop();
    }

    public void SetMoveMode()
    {
        moveMode = true;
    }

    public void StopRobot()
    {
        robotTimer.Stop();
    }

    // ===================== TIMER =====================

    public void StartTimer() => timer.Start();
    public void StopTimer()  => timer.Stop();

    private void RobotTimer_Tick(object? sender, EventArgs e)
    {
        AktionAusfuehren();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        factory.LegoBricksBewegen();
        NotifyLegoBricksPositionChanged();
        NotifyStepText();   // Bricks können zurückkommen oder weggehen
    }

    // ===================== UNDOCK =====================

    public void Undock()
    {
        UndockResult result = factory.Undock();

        UndockMessage = result switch
        {
            UndockResult.NoBrick           => "No brick available to undock.",
            UndockResult.NotOnDeliveryLine => "Undocking is only possible on the Delivery Line.",
            UndockResult.SuccessfulGood    => "Undocking successful. Brick is being delivered to the customer.",
            UndockResult.SuccessfulBad     => "Brick is defective and is being sent to the Recycling Line.",
            _                              => ""
        };

        NotifyRobotPositionChanged();
        NotifyLegoBricksPositionChanged();
        NotifyOrderChanged();   // Delivered-Zahlen und Setup-Button aktualisieren
    }

    // ===================== ANZEIGE AKTUALISIEREN =====================

    private void NotifyRobotPositionChanged()
    {
        OnPropertyChanged(nameof(RobotX));
        OnPropertyChanged(nameof(RobotY));
        OnPropertyChanged(nameof(RobotAngle));
    }

    private void NotifyLegoBricksPositionChanged()
    {
        foreach (var brick in Bricks)
        {
            brick.Refresh();
        }
    }
}