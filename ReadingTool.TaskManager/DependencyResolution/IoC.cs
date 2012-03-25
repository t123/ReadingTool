#region License
// IoC.cs is part of ReadingTool.TaskManager
// 
// ReadingTool.TaskManager is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.TaskManager is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.TaskManager. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System.Configuration;
using MongoDB.Driver;
using ReadingTool.Services;
using StructureMap;

namespace ReadingTool.TaskManager.DependencyResolution
{
    public static class IoC
    {
        public static void Initialize()
        {
            ObjectFactory.Initialize(x =>
                                         {
                                             x.Scan(scan =>
                                                        {
                                                            scan.TheCallingAssembly();
                                                            scan.AssemblyContainingType<IUserService>();
                                                            scan.WithDefaultConventions();
                                                        });
                                             x.For<MongoDatabase>().Use(
                                                 y => MongoServer
                                                          .Create(ConfigurationManager.ConnectionStrings["default"].ConnectionString)
                                                          .GetDatabase(ConfigurationManager.AppSettings["DBName"])
                                                 );
                                         });
        }
    }
}