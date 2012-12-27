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
            // TODO: Configure your bundles here...
            // Please read http://getcassette.net/documentation/configuration

            // This default configuration treats each file as a separate 'bundle'.
            // In production the content will be minified, but the files are not combined.
            // So you probably want to tweak these defaults!
            //bundles.AddPerIndividualFile<StylesheetBundle>("Content");
            //bundles.AddPerIndividualFile<ScriptBundle>("Scripts");

            // To combine files, try something like this instead:
            //   bundles.Add<StylesheetBundle>("Content");
            // In production mode, all of ~/Content will be combined into a single bundle.

            // If you want a bundle per folder, try this:
            //   bundles.AddPerSubDirectory<ScriptBundle>("Scripts");
            // Each immediate sub-directory of ~/Scripts will be combined into its own bundle.
            // This is useful when there are lots of scripts for different areas of the website.

            bundles.AddPerSubDirectory<ScriptBundle>("Scripts/required");
            bundles.AddPerSubDirectory<ScriptBundle>("Scripts/other/head", bundle => bundle.PageLocation = "head");
            bundles.AddPerSubDirectory<ScriptBundle>("Scripts/other/footer", bundle => bundle.PageLocation = "body");

            bundles.Add<StylesheetBundle>("Content/less/bootstrap/bootstrap.less");
            bundles.AddPerSubDirectory<StylesheetBundle>("Content/less/required/");
            bundles.AddPerSubDirectory<StylesheetBundle>("Scripts/other/", bundle => bundle.PageLocation = "head");
        }
    }
}