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
	public double speed;
	[Export]
	public int nof_passes;
	
	private int currentNodeIndex;
	private Vector3 nextPosition;

	private CharacterBody3D player;
	private Array<Node3D> cows = new Array<Node3D>();
	private Array<Node3D> endPoints = new Array<Node3D>();
	private Array<Node3D> betweenPoints = new Array<Node3D>();

	public override void _Ready()
	{
		player = GetNode<CharacterBody3D>(pathPlayer);
		for (int i = 0; i < GetNode<Node3D>(pathCows).GetChildCount(); i++) 
		{
			cows.Add(GetNode<Node3D>(pathCows).GetChild<Node3D>(i));
		}
		for (int i = 0; i < GetNode<Node3D>(pathEndPoints).GetChildCount(); i++) {
			endPoints.Add(GetNode<Node3D>(pathEndPoints).GetChild<Node3D>(i));
		}
		for (int i = 0; i < GetNode<Node3D>(pathBetweenPoints).GetChildCount(); i++) 
		{
			betweenPoints.Add(GetNode<Node3D>(pathBetweenPoints).GetChild<Node3D>(i));
		}
		
		CalculateNextEndPoint();

		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(this, "position", nextPosition, 5.0f);
		tween.SetTrans(Tween.TransitionType.Quart);
	}

	public override void _Process(double delta)
	{
		LookAtPlayer();
	}
	
	private void LookAtPlayer()
	{
		if (player == null) {
			GD.Print("Error, player not found for cow: ", this);
			return; 
		}
		this.LookAt(player.GlobalPosition, Vector3.Up);
	}

	private void CalculateNextEndPoint()
	{
		// Generate the next index
		RandomNumberGenerator random = new RandomNumberGenerator();
		int nextIndex = random.RandiRange(0, endPoints.Count-1);

		if (currentNodeIndex == nextIndex )
		{
			int m = random.RandiRange(0, 1);
			nextIndex = (m == 0) ? (nextIndex-1)%endPoints.Count : (nextIndex+1)%endPoints.Count;
		}
		
		nextPosition = endPoints[nextIndex].GlobalPosition;
	}

	private void moveToCow()
	{
		
	}
}
