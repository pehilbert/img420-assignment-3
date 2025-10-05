using Godot;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

public partial class Enemy : RigidBody2D
{
	// Adjustable in the Inspector
	[Export] public float MaxSpeed = 400f;
	[Export] public float SeparationWeight = 2.0f;
	[Export] public float AlignmentWeight = 5.0f;
	[Export] public float CohesionWeight = 0.2f;
	[Export] public float FollowWeight = 300.0f;
	[Export] public float FollowRadius = 700.0f;
	[Export] public int PointValue = 100;
	[Export] public int PlayerCollisionDamage = 1;
	[Export] public int SelfCollisionDamage = 1;
	[Export] public int BulletDamage = 1;
	[Export] public double FireInterval = 2.0f;
	[Export] public double FireIntervalVariance = 1.0f;
    [Export] public float BulletSpeed = 800f;
    [Export] public PackedScene BulletScene;

    private List<Enemy> _neighbors = new();
	private Area2D _detectionArea;
	private RigidBody2D _target;
	private Timer _fireTimer;

    public override void _Ready()
	{
        _detectionArea = GetNode<Area2D>("DetectionArea");
		_detectionArea.BodyEntered += OnBodyEntered;
		_detectionArea.BodyExited += OnBodyExited;

		var randomAngle = GD.Randf() * Mathf.Pi * 2;
		var randomVelocity = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * MaxSpeed;
		LinearVelocity = randomVelocity;

		LookAt(Position + LinearVelocity);

		_fireTimer = new Timer();
		_fireTimer.WaitTime = FireInterval + GD.RandRange(-FireIntervalVariance, FireIntervalVariance);
		_fireTimer.Autostart = true;
		_fireTimer.Timeout += () =>
		{
			FireBullet();
		};
		AddChild(_fireTimer);
		_fireTimer.Start();
    }

	public void SetTarget(RigidBody2D Target)
	{
		_target = Target;
	}

	public void FireBullet()
	{
        if (_target != null && IsInstanceValid(_target) && Position.DistanceTo(_target.GetPosition()) < FollowRadius)
		{
            var bullet = BulletScene.Instantiate<RigidBody2D>();
			bullet.Position = Position;
			bullet.LookAt(_target.Position);
			bullet.LinearVelocity = new Vector2(BulletSpeed, 0).Rotated(bullet.Rotation);

			// Enable contact monitoring for collision detection
			bullet.ContactMonitor = true;

			bullet.BodyEntered += (Node body) =>
			{
				if (!(body is Enemy))
				{
					var entityManager = body.GetNodeOrNull<EntityManager>("EntityManager");
					if (entityManager != null)
					{
						entityManager.TakeDamage(BulletDamage);
					}
					bullet.QueueFree();
				}
			};

            GetParent().AddChild(bullet);
        }

		_fireTimer.Start();
    }

	public override void _PhysicsProcess(double delta)
	{
		Vector2 separationVector = Separation() * SeparationWeight;

		Vector2 alignmentVector = Alignment() * AlignmentWeight;
		Vector2 cohesionVector = Cohesion() * CohesionWeight;
		Vector2 followVector = Centralization() * FollowWeight;
		//GD.Print($"The value of my variable is: {followVector}");

		LinearVelocity += (separationVector + alignmentVector + cohesionVector + followVector) * (float)delta;

		// Clamp velocity to prevent boids from moving too fast
		LinearVelocity = LinearVelocity.LimitLength(MaxSpeed);
		//GD.Print($"The value of my variable is: {Velocity}");

		if (_target != null && IsInstanceValid(_target))
		{
            LookAt(_target.Position);
        }
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Enemy enemy && body != this)
		{
			_neighbors.Add(enemy);
		}
	}

	private void OnBodyExited(Node2D body)
	{
		if (body is Enemy enemy && body != this)
		{
			_neighbors.Remove(enemy);
		}
	}

	// Rule 1: Separation—Avoid crowding neighbors
	private Vector2 Separation()
	{
		if (_neighbors.Count == 0) return Vector2.Zero;

		Vector2 steer = Vector2.Zero;
		foreach (var neighbor in _neighbors)
		{
			Vector2 diff = Position - neighbor.Position;
			steer += diff.Normalized() / diff.Length();
		}
		//GD.Print($"The value of my variable is: {steer}");
		return steer.Normalized();
	}

	// Rule 2: Alignment—Steer towards the average heading of neighbors
	private Vector2 Alignment()
	{
		if (_neighbors.Count == 0) return Vector2.Zero;

		Vector2 averageVelocity = Vector2.Zero;
		foreach (var neighbor in _neighbors)
		{
			averageVelocity += neighbor.LinearVelocity;
		}
		averageVelocity /= _neighbors.Count;
		return averageVelocity.Normalized();
	}

	// Rule 3: Cohesion—Move towards the average position of neighbors
	private Vector2 Cohesion()
	{
		if (_neighbors.Count == 0) return Vector2.Zero;

		Vector2 centerOfMass = Vector2.Zero;
		foreach (var neighbor in _neighbors)
		{
			centerOfMass += neighbor.Position;
		}
		centerOfMass /= _neighbors.Count;

		Vector2 directionToCenter = centerOfMass - Position;
		return directionToCenter.Normalized();
	}

	private Vector2 Centralization()
	{
		if (_target != null && IsInstanceValid(_target))
		{
			////GD.Print($"The value of my variable is: {_target.GetPosition()}");
			////GD.Print($"The value of my variable is: {Position}");
			if (Position.DistanceTo(_target.GetPosition()) < FollowRadius)
			{
				return Vector2.Zero;
			}
			else
			{
				return ((_target.GetPosition() - Position).Normalized());
			}
		}
		else
		{
			return Vector2.Zero;
		}

	}
}
