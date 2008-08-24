using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Mathematics;
using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerPhysics.Interfaces
{
    public interface IFluidContainer
    {
        bool Intersect(AABB aabb);
        bool Contains(ref Vector2 vector);
    }
}