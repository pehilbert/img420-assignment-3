using Godot;
using System;
using static Godot.SpringBoneSimulator3D;

public partial class Player : RigidBody2D
{
	[Export] public int Speed = 400;
	[Export] public int BulletSpeed = 800;
	[Export] public double FireRate = 5.0f; // bullets/second
	[Export] public double Damage = 1.0f;
	[Export] public Color InvincibilityColor = new Color(1, 1, 1, 0.5f);
	[Export] public PackedScene BulletScene;

	private bool canFire = true;
	private Timer fireTimer;

	public override void _Ready()
	{
		base._Ready();

		fireTimer = new Timer();
		fireTimer.WaitTime = 1.0f / FireRate;
		fireTimer.Timeout += () => canFire = true;
		AddChild(fireTimer);

		var entityManager = GetNodeOrNull<EntityManager>("EntityManager");
		
		if (entityManager != null)
		{
			entityManager.InvincibilityChanged += (bool invincible) =>
			{
				var sprite = GetNodeOrNull<Polygon2D>("Polygon2D");
				if (sprite != null)
				{
					sprite.Modulate = invincible ? InvincibilityColor : sprite.Color;
				}
			};
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		// Look at mouse position
		var mousePosition = GetGlobalMousePosition();
		var sprite = GetNodeOrNull<Polygon2D>("Polygon2D");
		sprite.LookAt(mousePosition);

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

		LinearVelocity = moveDirection.Normalized() * Speed;
		
		if (Input.IsActionPressed("fire"))
		{
			fire();
		}
	}

	private void fire()
	{
		if (canFire)
		{
			canFire = false;
			fireTimer.Start();

			var bullet = BulletScene.Instantiate<RigidBody2D>();
			bullet.Position = Position;
			bullet.LookAt(GetGlobalMousePosition());
			bullet.LinearVelocity = new Vector2(BulletSpeed, 0).Rotated(bullet.Rotation);

			// Enable contact monitoring for collision detection
			bullet.ContactMonitor = true;

			bullet.BodyEntered += (Node body) =>
			{
				if (!(body is Player))
				{
					var entityManager = body.GetNodeOrNull<EntityManager>("EntityManager");
					if (entityManager != null)
					{
						entityManager.TakeDamage(Damage);
					}
					bullet.QueueFree();
				}
			};

			GetParent().AddChild(bullet);
		}
	}
}
