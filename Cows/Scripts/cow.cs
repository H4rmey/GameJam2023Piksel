using Godot;
using System;

public partial class cow : Node3D
{
	[Export]
	public NodePath playerPath;

	private CharacterBody3D player;

	public override void _Ready()
	{
		player = GetNode<CharacterBody3D>(playerPath);
	}

	public override void _Process(double delta)
	{
		if (player == null) {
			GD.Print("Error, player not found for cow: ", this);
			return; 
		}
		this.LookAt(player.GlobalPosition, Vector3.Up);
	}
}
