#region License
// IRepository.cs is part of ReadingTool.Repository
// 
// ReadingTool.Repository is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Repository is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Repository. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ReadingTool.Repository
{
    public interface IRepository<T>
    {
        T LoadOne(object id);
        T FindOne(object id);
        T FindOne(Expression<Func<T, bool>> exp);
        T Save(T entity, bool returnUpdated = false);
        void Save(IEnumerable<T> entities);
        IQueryable<T> FindAll();
        IQueryable<T> FindAll(Expression<Func<T, bool>> exp);
        void Delete(T entity);
        void Delete(object id);
        void DeleteAll(Expression<Func<T, bool>> exp);
    }
}
