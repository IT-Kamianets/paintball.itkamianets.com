using ITKamianets.Paintball.Core;
using NUnit.Framework;

namespace ITKamianets.Paintball.Core.Tests
{
    public class MatchScoreTests
    {
        [Test]
        public void FirstTeamToTheLimitWins()
        {
            var score = new MatchScore(3);
            Assert.IsTrue(score.TryScore(TeamId.Red, TeamId.Blue));
            Assert.IsFalse(score.Winner.HasValue);
            score.TryScore(TeamId.Red, TeamId.Blue);
            score.TryScore(TeamId.Red, TeamId.Blue);

            Assert.IsTrue(score.Winner.HasValue);
            Assert.AreEqual(TeamId.Red, score.Winner.Value);
            Assert.AreEqual(3, score.Red);
            Assert.IsFalse(score.TryScore(TeamId.Blue, TeamId.Red));
            Assert.AreEqual(0, score.Blue);
        }

        [Test]
        public void SameTeamDoesNotScore()
        {
            var score = new MatchScore(10);
            Assert.IsFalse(score.TryScore(TeamId.Blue, TeamId.Blue));
            Assert.AreEqual(0, score.Blue);
            Assert.AreEqual(0, score.Red);
        }
    }
}
