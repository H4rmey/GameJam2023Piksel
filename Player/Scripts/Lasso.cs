using Godot;
using System;

public partial class Lasso : Node3D
{
	[Export]
	public Node3D lassoOrigin;
	[Export]
	public Node3D lassoEnd;
	[Export]
	public RayCast3D raycast;
	[Export]
	public RayCast3D lasso2;
	[Export]
	public AudioStreamPlayer3D whipSound;
	[Export]
	public AudioStreamPlayer upScoreSound;
	[Export]
	public float thickness=0.2f;
	[Export]
	public float lassoThrowSpeed = 0.2f;
	[Export]
	public float lassoThrowBackSpeed = 0.1f;
	
	public Player player;
	public Tween tweenThrow;
	public Cow cowTarget;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		player = GetParent<Player>();
		tweenThrow  = this.CreateTween();

		this.Visible = true;
		DoTweenThrow();

		this.tweenThrow.LoopFinished += OnThrowFinished;	
		this.tweenThrow.StepFinished += OnThrowStepFinished;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		this.StopWhipSound();
		this.DoLasso(this.lassoOrigin.GlobalPosition, this.lassoEnd.GlobalPosition);
	}

    public override void _PhysicsProcess(double delta)
    {
		//this.raycast.TargetPosition = this.lassoEnd.Position;

		if (raycast.GetCollider() as Node3D != null)
		{
			String targetName = raycast.Name;
			bool 	contains 	= targetName.IndexOf("Tree", StringComparison.OrdinalIgnoreCase) >= 0;

			if (!contains)
			{
				cowTarget = lasso2.GetCollider() as Cow;
			}
		}
		else
		{
			cowTarget = null;
		}
    }

	public void DoRaycast(Vector3 origin, Vector3 dest)
	{
		Vector3 direction = dest - origin;
		Vector3 pos = (dest + origin)/2;
		float length = origin.DistanceTo(dest);

		this.GlobalPosition = pos;
		this.LookAt(dest, new Vector3(0,1,0));
		this.RotateObjectLocal(new Vector3(1,0,0), -Mathf.Pi/2);
		this.Scale = new Vector3(thickness,length/2, thickness);
	}

	private void PlayWhipSound()
	{
		whipSound.Play(0.05f);
	}

	private void StopWhipSound()
	{
		if (whipSound.GetPlaybackPosition() > 0.55f)
		{
			whipSound.Stop();
		}
	}


	private void OnThrowStepFinished(long step)
	{
		if (step != 0) {
			return;
		}

		if (this.cowTarget == null) {
			return;
		}

		cowTarget.CowIsPulledByPlayer(this.player.GlobalPosition);

		if (cowTarget.is_taken) {
			upScoreSound.Play();
			player.score++;	
		}

	}

	public void thingy()
	{
		this.PlayWhipSound();
		this.Visible = true;
		this.tweenThrow.Play();
	}

    private void OnThrowFinished(long loopCount)
    {
		this.Visible = false;
        this.tweenThrow.Pause();
    }

	public void DoLasso(Vector3 origin, Vector3 dest)
	{
		Vector3 direction = dest - origin;
		Vector3 pos = (dest + origin)/2;
		float length = origin.DistanceTo(dest);

		this.GlobalPosition = pos;
		this.LookAt(dest, new Vector3(0,1,0));
		this.RotateObjectLocal(new Vector3(1,0,0), -Mathf.Pi/2);
		this.Scale = new Vector3(thickness,length/2, thickness);
	}

	public void DoTweenThrow()
	{
		Vector3 lep = this.lassoEnd.Position;
		Vector3 lop = this.lassoOrigin.Position;
		Vector3 mid = (lep + lop)/2;

		this.tweenThrow.SetLoops();
		this.tweenThrow.SetEase(Tween.EaseType.Out);
		this.tweenThrow.SetTrans(Tween.TransitionType.Quad);
		this.tweenThrow.TweenProperty(this.lassoEnd, "position", lep, lassoThrowSpeed);
		this.tweenThrow.TweenProperty(this.lassoEnd, "position", lop, lassoThrowBackSpeed);
	}
}
