using LightBDD.Core.Configuration;
using LightBDD.XUnit2;
using Xunit;

[assembly: LightBddScope]
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace TheatreCenter.E2ETests;

public class LightBddIntegration : LightBddScopeAttribute
{
    protected override void OnConfigure(LightBddConfiguration configuration)
    {
    }
}
