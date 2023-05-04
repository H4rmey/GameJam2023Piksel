using Godot;
using Godot.Collections;

public partial class SpaceShip : Node3D
{
	[Export]
	public NodePath pathPlayer;
	[Export]
	public NodePath pathCows;
	[Export]
	public NodePath pathEndPoints;
	[Export]
	public NodePath pathBetweenPoints;
    [Export]
	public double speedToEnd = 5;
    [Export]
	public double speedToCow = 3;
    [Export]
	public double speedToBetween = 0.5f;

    [Export]
	public double holdTimeCow = 5;
    [Export]
	public double holdTimeEnd = 3;
    [Export]
	public double holdTimeBetween = 0.5f;
    [Export]
	public float cowHoverBelow = 1;
	
	private Vector3 nextPosition;
	private int currentNodeIndex;
	private Node3D currentTarget;
	private Node3D cowTarget;

    private CharacterBody3D player;
    private Array<Node3D> cows = new Array<Node3D>();
    private Array<Node3D> cowsOriginal = new Array<Node3D>();
    private Array<Node3D> endPoints = new Array<Node3D>();
    private Array<Node3D> betweenPoints = new Array<Node3D>();

	public Tween tween; 

    public override void _Ready()
    {
        player = GetNode<CharacterBody3D>(pathPlayer);

		InitMovePoints();

		this.GlobalPosition = endPoints[0].GlobalPosition;

		tween = GetTree().CreateTween();
		tween.Finished += OnTweenFinished;
		
		AbductCow();
    }
	

    public override void _Process(double delta)
    {
    }

	private void OnTweenFinished()
	{
		GD.Print("Tween has finished :D");
	}

	private void AbductCow()
	{
		//quickly move between points
		tween.SetLoops(1);
		tween.SetTrans(Tween.TransitionType.Quart);
		CalculateNextEndPoint(betweenPoints);
		tween.TweenProperty(this, "position", new Vector3(nextPosition.X, this.GlobalPosition.Y, nextPosition.Z), speedToBetween);
		tween.TweenInterval(holdTimeBetween);
		CalculateNextEndPoint(betweenPoints);
		tween.TweenProperty(this, "position", new Vector3(nextPosition.X, this.GlobalPosition.Y, nextPosition.Z), speedToBetween);
		tween.TweenInterval(holdTimeBetween);
		CalculateNextEndPoint(betweenPoints);
		tween.TweenProperty(this, "position", new Vector3(nextPosition.X, this.GlobalPosition.Y, nextPosition.Z), speedToBetween);
		tween.TweenInterval(holdTimeBetween);
		//
		//	move to cow
		CalculateNextEndPoint(cows);
		tween.TweenProperty(this, "position", new Vector3(nextPosition.X, this.GlobalPosition.Y, nextPosition.Z), speedToCow);
		tween.TweenProperty(currentTarget, "position", new Vector3(nextPosition.X, this.GlobalPosition.Y-cowHoverBelow, nextPosition.Z), holdTimeCow);
		tween.TweenInterval(holdTimeCow/3.0f);
		//
		//	move to the endpoint/homebase
		cowTarget = currentTarget;
		CalculateNextEndPoint(endPoints);
		tween.TweenProperty(this, "position", new Vector3(nextPosition.X, this.GlobalPosition.Y, nextPosition.Z), speedToEnd);
		//move cow as well
		tween.Parallel().TweenProperty(cowTarget, "position", new Vector3(nextPosition.X, this.GlobalPosition.Y-cowHoverBelow, nextPosition.Z), speedToEnd);
		tween.TweenInterval(holdTimeEnd);
		tween.TweenCallback(Callable.From(() => cows.Remove(cowTarget)));
	}

	private void CalculateNextEndPoint(Array<Node3D> nodes)
	{
		// Generate the next index
		RandomNumberGenerator random = new RandomNumberGenerator();
		int nextIndex = random.RandiRange(0, nodes.Count-1);

		if (currentNodeIndex == nextIndex )
		{
			int m = random.RandiRange(0, 1);
			nextIndex = (m == 0) ? (nextIndex-1)%nodes.Count : (nextIndex+1)%nodes.Count;
		}

		currentNodeIndex = nextIndex;	
		currentTarget = nodes[nextIndex];
		nextPosition = nodes[nextIndex].GlobalPosition;
	}

	private void InitMovePoints()
	{
		for (int i = 0; i < GetNode<Node3D>(pathCows).GetChildCount(); i++) 
		{
        	cows.Add(GetNode<Node3D>(pathCows).GetChild<Node3D>(i));
        	cowsOriginal.Add(GetNode<Node3D>(pathCows).GetChild<Node3D>(i));
			SetCowScript(i);
		}
		for (int i = 0; i < GetNode<Node3D>(pathEndPoints).GetChildCount(); i++) {
			endPoints.Add(GetNode<Node3D>(pathEndPoints).GetChild<Node3D>(i));
		}
		for (int i = 0; i < GetNode<Node3D>(pathBetweenPoints).GetChildCount(); i++) 
		{
			betweenPoints.Add(GetNode<Node3D>(pathBetweenPoints).GetChild<Node3D>(i));
		}
	}

	private void SetCowScript(int i)
	{
			//set cow script
			Cow cowScript = cows[i] as Cow;
			cowScript.spaceShipScript = this as SpaceShip;
			cowScript.spaceShip = this;
	}
	
}
