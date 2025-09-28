using Godot;
using System;

public partial class EntityManager : Node2D
{
	[Export] public double MaxHealth = 3f;
	[Export] public double IFramesDuration = 1f;
	[Export] public Color IFramesModulate = new Color(1, 1, 1, 0.5f);

	public double CurrentHealth;
	public bool Invincible = false;

	private Timer iFramesTimer;

	[Signal]
	public delegate void HealthChangedEventHandler(double currentHealth, double maxHealth);

	public override void _Ready()
	{
		CurrentHealth = MaxHealth;

		if (IFramesDuration > 0)
		{
			iFramesTimer = new Timer();
			iFramesTimer.WaitTime = IFramesDuration;
			iFramesTimer.Timeout += () => {
				Invincible = false;
				Modulate = new Color(1, 1, 1, 1);
			};
			AddChild(iFramesTimer);
		} 
		else
		{
			iFramesTimer = null;
		}
	}

	public void IFrames()
	{
		if (!Invincible && iFramesTimer != null)
		{
			Invincible = true;
			iFramesTimer.Start();
			Modulate = IFramesModulate;
		}
	}

	public void TakeDamage(double damage)
	{
		if (!Invincible)
		{
			CurrentHealth = Math.Max(CurrentHealth - damage, 0);
			EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
			GD.Print("Took damage, current health: " + CurrentHealth);

			if (CurrentHealth <= 0)
			{
				Die();
			} 
			else
			{
				IFrames();
			}
		}
	}

	public void Heal(double amount)
	{
		CurrentHealth = Math.Min(CurrentHealth + amount, MaxHealth);
		EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
	}

	public void Die()
	{
		GetParent().QueueFree();
	}
}
