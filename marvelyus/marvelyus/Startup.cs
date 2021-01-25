using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(marvelyus.Startup))]
namespace marvelyus
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
