using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GMTK24;

public class Particle
{
    private readonly float _startingLifetime;

    public Particle(Vector2 position, Texture2D texture, Vector2 velocity)
    {
        Position = position;
        Texture = texture;
        Velocity = velocity;
        _startingLifetime = 0.5f;
        RemainingLifetime = _startingLifetime;
    }

    public Vector2 Position { get; set; }
    public Texture2D Texture { get; set; }
    public Vector2 Velocity { get; set; }
    public float Angle { get; set; }

    public float RemainingLifetime { get; set; }
    public float Opacity => RemainingLifetime / _startingLifetime;

    public void Update(float dt)
    {
        RemainingLifetime -= dt;
        Angle += dt * 4;
        Velocity += new Vector2(0, dt) * 500;
        Position += Velocity * dt;
    }

    public bool IsExpired()
    {
        return RemainingLifetime < 0;
    }
}
