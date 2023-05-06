using Godot;
using Godot.Collections;

public partial class CowHandler : Node3D
{
	[Export]
	public Node3D pathCows;
	[Export]
	public NodePath pathSpawnpoints;
	[Export]
	public Area3D spaceShip;
	[Export]
	public int level;
	[Export]
	public int nofCows = 3;

    private Array<Node3D> cows = new Array<Node3D>();
    private Array<Node3D> spwns = new Array<Node3D>();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		for (int i = 0; i < GetNode<Node3D>(pathSpawnpoints).GetChildCount(); i++) 
		{
        	spwns.Add(GetNode<Node3D>(pathSpawnpoints).GetChild<Node3D>(i));
 		}

		SpawnCows();
	}

	private void SpawnCows()
	{
		nofCows = 3 + level * 2;

		if (nofCows >= spwns.Count)
		{
			nofCows = spwns.Count-1;
		}

		if (nofCows <= 0)
		{
			nofCows = 3;
		}
		
		RandomNumberGenerator random = new RandomNumberGenerator();

		for (int i = 0; i < nofCows; i++) 
		{
			float randomHeight = random.RandfRange(3, 6);
			
			PackedScene cowScene 	= ResourceLoader.Load<PackedScene>("res://Cows/Scenes/Cow.tscn");
			Cow cow 				= (Cow)cowScene.Instantiate();
			cow.Name 				= "Cow_" + i;
			cow.spaceShipScript 	= spaceShip as SpaceShip;
			cow.spaceShip 			= spaceShip;
			pathCows.AddChild(cow);		
			cow.GlobalPosition 		= new Vector3(spwns[i].GlobalPosition.X, randomHeight, spwns[i].GlobalPosition.Z);	
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void SetCowScript(int i)
	{
		//set cow script
		Cow cowScript = cows[i] as Cow;
		cowScript.spaceShipScript = spaceShip as SpaceShip;
		cowScript.spaceShip = spaceShip;
	}	
}
