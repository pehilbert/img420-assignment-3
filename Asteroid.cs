using Godot;
using System;

public partial class Asteroid : RigidBody2D
{
	[Export] public double Damage = 1.0f;
	[Export] public double SelfDamage = 1.0f;

	private EntityManager entityManager;

	public override void _Ready()
	{
		entityManager = GetNodeOrNull<EntityManager>("EntityManager");

		// When something collides with the asteroid, damage it and damage self
		BodyEntered += (Node body) =>
		{
			GD.Print("Collision with " + body.Name);

			var bodyEntityManager = body.GetNodeOrNull<EntityManager>("EntityManager");

			if (bodyEntityManager != null && !bodyEntityManager.Invincible)
			{
				bodyEntityManager.TakeDamage(Damage);

				if (entityManager != null)
				{
					entityManager.TakeDamage(SelfDamage);
				}
			}
		};
	}
}
