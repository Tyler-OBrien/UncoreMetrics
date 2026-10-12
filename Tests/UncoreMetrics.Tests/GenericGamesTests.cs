using UncoreMetrics.Data.GameData;
using UncoreMetrics.Steam_Collector;
using UncoreMetrics.Steam_Collector.Game_Collectors;

namespace UncoreMetrics.Tests;

public class GenericGamesTests
{
    [Fact]
    public void EveryGenericGame_IsInTheSharedGameList()
    {
        foreach (var game in GenericGames.All.Values)
        {
            var listed = Assert.Single(StaticGameInfo.Games, g => g.AppId == game.AppId);
            Assert.Equal(game.Name, listed.Name);
        }
    }

    [Fact]
    public void GenericGames_HaveUniqueAppIds_AndDontShadowDedicatedResolvers()
    {
        var resolvers = new GameResolvers();
        Assert.Equal(GenericGames.All.Count, GenericGames.All.Values.Select(g => g.AppId).Distinct().Count());
        foreach (var type in GenericGames.All.Keys)
        {
            Assert.True(resolvers.DoesGameResolverExist(type));
            Assert.NotNull(resolvers.GetGenericGame(type));
        }

        Assert.Null(resolvers.GetGenericGame("Rust"));
    }
}
