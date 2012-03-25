#region License
// MongoIdJsonContractResolver.cs is part of ReadingTool
// 
// ReadingTool is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ReadingTool.Binders
{
    public class MongoIdJsonContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            if(typeof(ObjectId).IsAssignableFrom(objectType))
            {
                return new JsonPrimitiveContract(objectType)
                {
                    IsReference = false,
                    Converter = new MongoIdConverter(),
                };
            }

            return base.CreateContract(objectType);
        }

        private class MongoIdConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, (value ?? "").ToString());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                ObjectId id;
                if(ObjectId.TryParse((reader.Value ?? "").ToString(), out id))
                    return id;

                return ObjectId.Empty;
            }

            public override bool CanConvert(Type objectType)
            {
                return typeof(ObjectId).IsAssignableFrom(objectType);
            }
        }
    }
}