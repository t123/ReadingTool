#region License
// CassetteConfiguration.cs is part of ReadingTool.Site
// 
// ReadingTool.Site is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Site is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Site. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

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
            bundles.Add<StylesheetBundle>("Content/codemirror", new[] { "codemirror.css" });
            bundles.Add<StylesheetBundle>("Content/jPlayer", new[] { "jplayer.blue.monday.css" });
            bundles.AddPerSubDirectory<ScriptBundle>("Scripts/");
        }
    }
}