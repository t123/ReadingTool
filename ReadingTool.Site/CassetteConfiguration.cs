using Cassette;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace ReadingTool.Site
{
    /// <summary>
    /// Configures the Cassette asset bundles for the web application.
    /// </summary>
    public class CassetteBundleConfiguration : IConfiguration<BundleCollection>
    {
        public void Configure(BundleCollection bundles)
        {
            bundles.Add<StylesheetBundle>("Content/css", new[] { "bootstrap.css", "site.css" });
            bundles.Add<StylesheetBundle>("Content/reading", new[] { "read.css" });
            bundles.Add<StylesheetBundle>("Content/fineuploader", new[] { "fineuploader.css" });
            bundles.Add<StylesheetBundle>("Content/codemirror", new[] { "codemirror.css" });
            bundles.AddPerSubDirectory<ScriptBundle>("Scripts/");
        }
    }
}