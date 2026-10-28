using UnityEngine;

namespace ITKamianets.Paintball.Core
{
    public enum TeamId
    {
        Red,
        Blue,
    }

    public static class TeamColors
    {
        public static readonly Color Red = new Color(0.82f, 0.16f, 0.16f);
        public static readonly Color Blue = new Color(0.18f, 0.38f, 0.86f);

        public static Color Of(TeamId team) => team == TeamId.Red ? Red : Blue;

        public static string Name(TeamId team) => team == TeamId.Red ? "Червоні" : "Сині";
    }
}
