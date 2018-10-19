using System;
using System.Threading.Tasks;
using Orleans;

namespace Foo.Interfaces
{
    public interface IFoo : IGrainWithGuidKey
    {
        Task<string> JustFooIt();
    }
}
