#region License
// AnkiSync.cs is part of ReadingTool.Common
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
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Newtonsoft.Json;

namespace ReadingTool.Common.Anki
{
    sealed class AnkiSync
    {
        [DataContract]
        private class DeckResponse
        {
            [DataMember(Name = "status")]
            public string Status { get; set; }

            [DataMember(Name = "timestamp")]
            public double Timestamp { get; set; }

            [DataMember(Name = "decks")]
            public Dictionary<string, double[]> Decks { get; set; }

            public DeckResponse()
            {
                Decks = new Dictionary<string, double[]>();
            }
        }

        [DataContract]
        internal class SummaryResponse
        {
            [DataMember(Name = "delmodels")]
            public double[][] DelModels { get; set; }

            [DataMember(Name = "facts")]
            public double[][] Facts { get; set; }

            [DataMember(Name = "delcards")]
            public double[][] DelCards { get; set; }

            [DataMember(Name = "models")]
            public double[][] Models { get; set; }

            [DataMember(Name = "media")]
            public double[][] Media { get; set; }

            [DataMember(Name = "cards")]
            public double[][] Cards { get; set; }

            [DataMember(Name = "delfacts")]
            public double[][] DelFacts { get; set; }

            [DataMember(Name = "delmedia")]
            public double[][] DelMedia { get; set; }
        }

        public class Deck
        {
            public string Name { get; set; }
            public double LastModified { get; set; }
            public double LastSync { get; set; }
            public SummaryResponse SummaryResponse { get; set; }
        }

        private static class AnkiConstants
        {
            internal const string CLIENT = @"ankidroid-1.1beta21";
            internal const string LIB_ANKI = @"1.2.5";
            internal const string PROTOCOL = @"5";
            internal const string SYNC_VERSION = @"2";
            internal const string SYNC_CULTURE = "en-US";

            internal static class Urls
            {
                private const string SYNC_URL = @"http://ankiweb.net/sync/";
                internal const string SYNC_GET_DECKS = SYNC_URL + "getDecks";
                internal const string SYNC_SUMMARY = SYNC_URL + "summary";
                internal const string SYNC_APPLY_PAYLOAD = SYNC_URL + "applyPayload";
                internal const string SYNC_FINISH = SYNC_URL + "finish";
            }

            internal static class StatusCodes
            {
                internal const string ANKIWEB_STATUS_OK = "OK";
                internal const string ANKIWEB_STATUS_INVALID_USER_PASS = "invalidUserPass";
                internal const string ANKIWEB_STATUS_OLD_VERSION = "oldVersion";
                internal const string ANKIWEB_STATUS_TOO_BUSY = "AnkiWeb is too busy right now. Please try again later.";
            }
        }

        private readonly DeckResponse _deckResponse;
        private readonly Dictionary<string, Deck> _decks;
        private readonly string _username;
        private readonly string _password;

        public ReadOnlyCollection<Deck> Decks { get { return new ReadOnlyCollection<Deck>(_decks.Select(x => x.Value).ToList()); } }

        public Deck this[string key]
        {
            get
            {
                if(_decks.ContainsKey(key))
                    return _decks[key];

                return null;
            }
        }

        public bool HasDeck(string name)
        {
            return _decks.ContainsKey(name);
        }

        public AnkiSync(string username, string password)
        {
            _username = username;
            _password = password;
            _deckResponse = GetDecks();

            if(!_deckResponse.Status.Equals(AnkiConstants.StatusCodes.ANKIWEB_STATUS_OK, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new AnkiSyncException(string.Format("Ankiweb returned invalid status: {0}", _deckResponse.Status));
            }

            _decks = DeckResponseToDecks(_deckResponse);
        }

        private Dictionary<string, Deck> DeckResponseToDecks(DeckResponse deckResponse)
        {
            var decks = new Dictionary<string, Deck>(deckResponse.Decks.Count);

            if(_deckResponse.Status.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach(var deck in _deckResponse.Decks)
                {
                    Deck d = new Deck()
                                   {
                                       Name = deck.Key,
                                       LastModified = deck.Value[0],
                                       LastSync = deck.Value[1]
                                   };

                    decks.Add(d.Name, d);
                }
            }

            return decks;
        }

        private DeckResponse GetDecks()
        {
            var post = new WebPostRequest(AnkiConstants.Urls.SYNC_GET_DECKS);
            post.Add("p", _password);
            post.Add("client", AnkiConstants.CLIENT);
            post.Add("u", _username);
            post.Add("v", AnkiConstants.SYNC_VERSION);
            post.Add("d", "None");
            post.Add("sources", "[]");
            post.Add("libanki", AnkiConstants.LIB_ANKI);
            post.Add("pversion", AnkiConstants.PROTOCOL);

            try
            {
                var response = post.GetResponse();
                DeckResponse deckResponse = JsonConvert.DeserializeObject<DeckResponse>(response);

                if(deckResponse == null)
                {
                    throw new AnkiSyncException("No deck response");
                }

                return deckResponse;
            }
            catch(Exception e)
            {
                throw new AnkiSyncException("Unable to decode deck response JSON", e);
            }
        }

        public Deck Summary(Deck deck)
        {
            if(deck == null)
            {
                throw new AnkiSyncException("Cannot retrieve summary of a null deck");
            }

            var post = new WebPostRequest(AnkiConstants.Urls.SYNC_SUMMARY);
            post.Add("p", _password);
            post.Add("u", _username);
            post.Add("d", deck.Name);
            post.Add("v", AnkiConstants.SYNC_VERSION);
            post.Add("lastSync", CompressAndBase64Encode(deck.LastSync.ToString(new CultureInfo(AnkiConstants.SYNC_CULTURE))));
            post.Add("base64", "true");

            try
            {
                var response = post.GetResponse();
                deck.SummaryResponse = JsonConvert.DeserializeObject<SummaryResponse>(response);
                return deck;
            }
            catch(Exception e)
            {
                throw new AnkiSyncException("Could not decode summary JSON", e);
            }
        }

        private void ApplyPayload(Deck deck, string payload)
        {
            var post = new WebPostRequest(AnkiConstants.Urls.SYNC_APPLY_PAYLOAD);
            post.Add("p", _password);
            post.Add("u", _username);
            post.Add("v", AnkiConstants.SYNC_VERSION);
            post.Add("d", deck.Name);
            post.Add("payload", CompressAndBase64Encode(payload));
            post.Add("base64", "true");
        }

        private bool Finish(Deck deck)
        {
            var post = new WebPostRequest(AnkiConstants.Urls.SYNC_FINISH);
            post.Add("p", _password);
            post.Add("u", _username);
            post.Add("v", AnkiConstants.SYNC_VERSION);
            post.Add("d", deck.Name);

            return true;
        }

        private string CompressAndBase64Encode(string data)
        {
            var tsBytes = Encoding.UTF8.GetBytes(data);
            string output;

            using(MemoryStream compressedStream = new MemoryStream())
            {
                compressedStream.Position = 0;
                using(DeflaterOutputStream deflater = new DeflaterOutputStream(compressedStream, new Deflater(Deflater.BEST_COMPRESSION)))
                {
                    deflater.Write(tsBytes, 0, tsBytes.Length);
                }

                var compressedBytes = compressedStream.ToArray();
                output = Base64Encode(compressedBytes);
            }

            return output;
        }

        private string Base64Encode(byte[] data)
        {
            try
            {
                string encodedData = Convert.ToBase64String(data);
                return encodedData;
            }
            catch(Exception e)
            {
                throw new Exception("Error in base64Encode" + e.Message);
            }
        }
    }

    public static class DateTimeHelper
    {
        internal static DateTime FromUnix(this Double timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0)
                .AddSeconds(timestamp)
                .ToLocalTime();
        }
    }
}
