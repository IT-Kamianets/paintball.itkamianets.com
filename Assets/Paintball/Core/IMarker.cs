using UnityEngine;

namespace ITKamianets.Paintball.Core
{
    /// <summary>A paintball marker. The core and the bots fire through this; the weapon package implements it.</summary>
    public interface IMarker
    {
        int Balls { get; }
        int Capacity { get; }
        bool IsReloading { get; }
        bool TryFire(Ray aim, Fighter shooter);
    }
}
