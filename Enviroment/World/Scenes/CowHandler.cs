using Godot;
using Godot.Collections;

public partial class CowHandler : Node3D
{
	[Export]
	public NodePath pathCows;

    private Array<Node3D> cows = new Array<Node3D>();
    private Array<Node3D> cowsOriginal = new Array<Node3D>();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		for (int i = 0; i < GetNode<Node3D>(pathCows).GetChildCount(); i++) 
		{
        	cows.Add(GetNode<Node3D>(pathCows).GetChild<Node3D>(i));
        	cowsOriginal.Add(GetNode<Node3D>(pathCows).GetChild<Node3D>(i));
 		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
