namespace ITKamianets.Paintball.Core
{
    /// <summary>Team points for one match. The first team to reach the limit wins.</summary>
    public sealed class MatchScore
    {
        public int Limit { get; }
        public int Red { get; private set; }
        public int Blue { get; private set; }
        public TeamId? Winner { get; private set; }

        public MatchScore(int limit)
        {
            Limit = limit;
        }

        public int Of(TeamId team) => team == TeamId.Red ? Red : Blue;

        /// <summary>Gives the killer's team a point. Same-team hits and points after the match is won do nothing.</summary>
        public bool TryScore(TeamId killer, TeamId victim)
        {
            if (Winner.HasValue || killer == victim)
            {
                return false;
            }

            if (killer == TeamId.Red)
            {
                Red++;
            }
            else
            {
                Blue++;
            }

            if (Red >= Limit)
            {
                Winner = TeamId.Red;
            }
            else if (Blue >= Limit)
            {
                Winner = TeamId.Blue;
            }

            return true;
        }
    }
}
