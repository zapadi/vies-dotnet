using System;
using Zapadi.Vies;

public class ViesManagerFixture : IDisposable
{
    public ViesManagerFixture()
    {
        ViesManager = new ViesManager();
    }

    public void Dispose()
    {
        ViesManager?.Dispose();
    }

    public ViesManager ViesManager { get; }
}