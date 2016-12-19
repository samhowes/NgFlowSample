using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Flow.Net.Example.Startup))]

namespace Flow.Net.Example
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
