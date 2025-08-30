using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class GameModeTest {

    [TestMethod]
    public void QueueSizeIsCorrect() {
        const int TEAM_SIZE = 4;
        const int PARTY_SIZE = 2;
        const int PARTY_ID = 1;
        var gm = new GameMode(TEAM_SIZE);
        gm.Enqueue("na", PARTY_SIZE, PARTY_ID);
        Assert.AreEqual(TEAM_SIZE, gm["na"].Count);
        Assert.AreEqual(1, gm["na"][PARTY_SIZE - 1].Count);
    }

}