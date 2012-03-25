#region License
// WebPostRequest.cs is part of ReadingTool.Common
// 
// ReadingTool.Common is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Common is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Common. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace ReadingTool.Common.Anki
{
    internal class WebPostRequest
    {
        readonly WebRequest _request;
        readonly Dictionary<string, string> _queryData;

        public WebPostRequest(string url)
        {
            _request = WebRequest.Create(url);
            _request.Method = "POST";
            _request.Headers["Accept-Encoding"] = "identity";
            _request.ContentType = "application/x-www-form-urlencoded";

            _queryData = new Dictionary<string, string>();
        }

        public void Add(string key, string value)
        {
            _queryData.Add(key, value);
        }

        public string GetResponse()
        {
            string parameters = string.Join("&",
                                            _queryData.Select(
                                                x => new
                                                         {
                                                             Value = Uri.EscapeUriString(x.Key) + "=" + Uri.EscapeUriString(x.Value)
                                                         }
                                                ).Select(x => x.Value)
                );
            byte[] byteArray = Encoding.UTF8.GetBytes(parameters);
            _request.ContentLength = byteArray.Length;

            using(StreamWriter sw = new StreamWriter(_request.GetRequestStream()))
            {
                sw.Write(parameters);
                sw.Close();
            }

            string result = "";

            try
            {
                using(var response = (HttpWebResponse)_request.GetResponse())
                {
                    if(response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new AnkiSyncException(string.Format("Server response was {0}, expected OK/200", response.StatusCode));
                    }

                    using(var responseStream = response.GetResponseStream())
                    {
                        using(var inflater = new InflaterInputStream(responseStream))
                        {
                            using(var sr = new StreamReader(inflater))
                            {
                                result = sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                throw new AnkiSyncException("Unable to retrieve response from server", e);
            }

            return result;
        }
    }
}
