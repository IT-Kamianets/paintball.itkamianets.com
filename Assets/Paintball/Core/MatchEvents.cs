namespace ITKamianets.Paintball.Core
{
    public struct PaintballElimination
    {
        public TeamId KillerTeam;
        public TeamId VictimTeam;
        public bool KillerIsPlayer;
        public bool VictimIsPlayer;
    }

    public struct PaintballMatchEnded
    {
        public TeamId Winner;
    }
}
