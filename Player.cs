using Godot;
using System;
using static Godot.SpringBoneSimulator3D;

public partial class Player : CharacterBody2D
{
	[Export] public int Speed = 400;
	[Export] public int BulletSpeed = 800;
	[Export] public float FireRate = 5.0f; // bullets/second
	[Export] public PackedScene BulletScene;
	private bool canFire = true;
	private Timer fireTimer;

	public override void _Ready()
	{
		base._Ready();

		fireTimer = GetNode<Timer>("FireTimer");

		if (fireTimer != null)
		{
			fireTimer.WaitTime = 1.0f / FireRate;
			fireTimer.Timeout += () => canFire = true;
		}
	}

	public override void _ExitTree()
	{
		base._Ready();

		if (fireTimer != null)
		{
			fireTimer.Timeout -= () => canFire = true;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		// Look at mouse position
		var mousePosition = GetViewport().GetMousePosition();
		LookAt(mousePosition);

		// Move the player
		Vector2 moveDirection = Vector2.Zero;

		if (Input.IsActionPressed("move_up"))
		{
			moveDirection.Y -= 1;
		}

		if (Input.IsActionPressed("move_down"))
		{
			moveDirection.Y += 1;
		}

		if (Input.IsActionPressed("move_left"))
		{
			moveDirection.X -= 1;
		}

		if (Input.IsActionPressed("move_right"))
		{
			moveDirection.X += 1;
		}

		Velocity = moveDirection.Normalized() * Speed;
		MoveAndSlide();

		if (Input.IsActionPressed("fire"))
		{
			fire();
		}
	}

	private void fire()
	{
		if (canFire)
		{
			// Reset canFire, start cooldown timer
			canFire = false;

			if (fireTimer != null)
			{
				fireTimer.Start();
			}

			// Fire the bullet
			var bullet = BulletScene.Instantiate<RigidBody2D>();
			bullet.Position = Position;
			bullet.LookAt(GetViewport().GetMousePosition());
			bullet.LinearVelocity = new Vector2(BulletSpeed, 0).Rotated(bullet.Rotation);

			// Prevent bullet from colliding with the player
			bullet.CollisionLayer = 2;
			bullet.CollisionMask = 0xFFFFFFFE;

			GetParent().AddChild(bullet);
		}
	}
}
