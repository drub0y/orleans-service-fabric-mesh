using System;
using System.Threading.Tasks;
using Foo.Interfaces;
using Orleans;
using Orleans.Providers;

namespace Foo.Implementation
{
    [StorageProvider(ProviderName = "Foos")]
    public class FooGrain : Grain<FooState>, IFoo
    {
        public async Task<string> JustFooIt()
        {
            this.State.Counter++;

            await this.WriteStateAsync();

            return $"{this.State.Counter} - {DateTime.UtcNow.Ticks}";
        }
    }
}
