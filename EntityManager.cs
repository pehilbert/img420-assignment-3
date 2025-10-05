using Godot;
using System;

public partial class EntityManager : Node2D
{
	[Export] public double MaxHealth = 3f;
	[Export] public double IFramesDuration = 1f;
	[Export] public Color IFramesModulate = new Color(1, 1, 1, 0.5f);
	[Export] public bool HealthBarVisible = true;
	[Export] public double HealthBarVisibilityDuration = 2f;

    public double CurrentHealth;
	public bool Invincible = false;

	private Timer iFramesTimer;
	private Timer healthBarTimer;
	private Polygon2D healthBar;
	private Polygon2D healthBarProgress;

	[Signal]
	public delegate void HealthChangedEventHandler(double currentHealth, double maxHealth);

	[Signal]
	public delegate void DiedEventHandler(Node entityDied);

	[Signal]
	public delegate void InvincibilityChangedEventHandler(bool invincible);

    public override void _Process(double delta)
    {
        base._Process(delta);
        GlobalRotationDegrees = 0; // Prevent rotation with parent
    }

	public override void _Ready()
	{
		CurrentHealth = MaxHealth;
        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);

        if (IFramesDuration > 0)
		{
			iFramesTimer = new Timer();
			iFramesTimer.WaitTime = IFramesDuration;
			iFramesTimer.Timeout += () => {
				Invincible = false;
				EmitSignal(SignalName.InvincibilityChanged, Invincible);
			};
			AddChild(iFramesTimer);
		} 
		else
		{
			iFramesTimer = null;
		}

		healthBar = GetNode<Polygon2D>("HealthBar");
		healthBarProgress = healthBar?.GetNode<Polygon2D>("HealthBarProgress");
		
		if (healthBar != null)
		{
			healthBar.Visible = false;
        }

        if (healthBar != null && healthBarProgress != null && HealthBarVisible)
		{
			healthBarTimer = new Timer();
			healthBarTimer.WaitTime = HealthBarVisibilityDuration;
			healthBarTimer.Timeout += () =>
			{
				healthBar.Visible = false;
			};
			AddChild(healthBarTimer);

            HealthChanged += (double currentHealth, double maxHealth) =>
			{
				healthBarProgress.Scale = new Vector2((float)(currentHealth / maxHealth), 1);
				healthBar.Visible = true;
				healthBarTimer.Start();
            };
		}
    }

    public void IFrames()
	{
		if (!Invincible && iFramesTimer != null)
		{
			Invincible = true;
			iFramesTimer.Start();
			EmitSignal(SignalName.InvincibilityChanged, Invincible);
		}
	}

	public void TakeDamage(double damage)
	{
		if (!Invincible)
		{
			CurrentHealth = Math.Max(CurrentHealth - damage, 0);
			EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
			// GD.Print(GetParent().Name + " took damage, current health: " + CurrentHealth);

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
        EmitSignal(SignalName.Died, GetParent());
        GetParent().QueueFree();
    }
}
