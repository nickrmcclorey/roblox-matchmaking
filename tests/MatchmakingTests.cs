using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class MatchmakingTests {

    [TestMethod]
    public void CanMakeGame() {
        Assert.IsTrue(Matchmaker.canMakeGame(new List<int> { 0, 2, 3, 4 }, 2, 2));
        Assert.IsTrue(Matchmaker.canMakeGame(new List<int> { 2, 1, 3, 4 }, 2, 2));
        Assert.IsTrue(Matchmaker.canMakeGame(new List<int> { 2, 2, 0, 1, 0 }, 5, 2));
        Assert.IsTrue(Matchmaker.canMakeGame(new List<int> { 0, 1, 1, 0, 1 }, 5, 2));
        Assert.IsTrue(Matchmaker.canMakeGame(new List<int> { 1, 1, 1, 1, 0 }, 5, 2));
    }

    [TestMethod]
    public void CantMakeGame() {
        Assert.IsFalse(Matchmaker.canMakeGame(new List<int> { 1, 1, 3, 4 }, 2, 2));
        Assert.IsFalse(Matchmaker.canMakeGame(new List<int> { 0, 1, 3, 1, 0 }, 5, 2));
    }
}