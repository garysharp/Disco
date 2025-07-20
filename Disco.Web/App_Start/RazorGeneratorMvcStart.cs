using RazorGenerator.Mvc;
using System.Web.Mvc;
using System.Web.WebPages;

[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(Disco.Web.RazorGeneratorMvcStart), "Start")]

namespace Disco.Web
{
    public static class RazorGeneratorMvcStart
    {
        public static void Start()
        {
            var engine = new PrecompiledMvcEngine(typeof(RazorGeneratorMvcStart).Assembly)
            {
#if DEBUG
                UsePhysicalViewsIfNewer = true
#else
                UsePhysicalViewsIfNewer = false
#endif
            };

            ViewEngines.Engines.Insert(0, engine);

            // StartPage lookups are done by WebPages. 
            VirtualPathFactoryManager.RegisterVirtualPathFactory(engine);
        }
    }
}