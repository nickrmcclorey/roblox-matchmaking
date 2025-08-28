using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class MatchmakingTests {

    [TestMethod]
    public void CanMakeGame() {
        Assert.IsTrue(NewSingleRegionQueue([ 0, 2, 3, 4 ]).CanMakeGame(2, 2));
        Assert.IsTrue(NewSingleRegionQueue([ 2, 1, 3, 4 ]).CanMakeGame(2, 2));
        Assert.IsTrue(NewSingleRegionQueue([ 2, 2, 0, 1, 0 ]).CanMakeGame(5, 2));
        Assert.IsTrue(NewSingleRegionQueue([ 0, 1, 1, 0, 1 ]).CanMakeGame(5, 2));
        Assert.IsTrue(NewSingleRegionQueue([ 1, 1, 1, 1, 0 ]).CanMakeGame(5, 2));
    }

    [TestMethod]
    public void CantMakeGame() {
        Assert.IsFalse(NewSingleRegionQueue([ 1, 1, 3, 4 ]).CanMakeGame(2, 2));
        Assert.IsFalse(NewSingleRegionQueue([ 0, 1, 3, 1, 0 ]).CanMakeGame(5, 2));
    }

    private SingleRegionQueue NewSingleRegionQueue(int[] queueCount) {
        int id = 1;
        var queue = new SingleRegionQueue(queueCount.Length);
        for (int i = 0; i < queueCount.Length; i++) {
            for (int m = 0; m < queueCount[i]; m++) {
                queue[i].Enqueue(id);
                id++;
            }
        }
        return queue;
    }
}