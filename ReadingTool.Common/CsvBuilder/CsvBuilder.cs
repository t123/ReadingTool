#region License
// CsvBuilder.cs is part of ReadingTool.Common
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
using System.Data;
using System.Linq;
using System.Text;

namespace ReadingTool.Common.CsvBuilder
{
    public enum CsvType
    {
        CSV = 0,
        TSV = 1
    }

    public class CsvBuilder
    {
        private readonly CsvType _csvType;
        private readonly bool _includeQuotes;
        private CsvRow _header;
        private IList<CsvRow> _rows;

        #region constructors
        public CsvBuilder()
            : this(CsvType.CSV, true)
        {

        }

        public CsvBuilder(CsvType csvType)
            : this(csvType, true)
        {

        }

        public CsvBuilder(bool includeQuotes)
            : this(CsvType.CSV, includeQuotes)
        {

        }

        public CsvBuilder(CsvType csvType, bool includeQuotes)
        {
            _csvType = csvType;
            _includeQuotes = includeQuotes;
            _header = new CsvRow();
            _rows = new List<CsvRow>();
        }
        #endregion

        public void AddHeader(string[] names)
        {
            _header.AddColumn(names);
        }

        public void AddHeader(string name)
        {
            _header.AddColumn(name);
        }

        public void AddRow(string[] values)
        {
            if(values == null) throw new NoNullAllowedException("Array cannot be null");
            if(values.Length != _header.Data.Count) throw new Exception("Mismatched bewteen row length and header length");

            CsvRow row = new CsvRow();
            row.AddColumn(values);
            _rows.Add(row);
        }

        public void AddRow(string value)
        {
            AddRow(new[] { value });
        }

        public CsvType CsvType
        {
            get { return _csvType; }
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool excludeHeader)
        {
            StringBuilder sb = new StringBuilder();
            char joinCharacter = _csvType == CsvType.CSV ? ',' : '\t';
            string joinString = _includeQuotes ? @"""" + joinCharacter + @"""" : joinCharacter.ToString();

            if(!excludeHeader)
            {
                sb.AppendFormat(@"{0}{1}{0}{2}",
                                _includeQuotes ? @"""" : "",
                                string.Join(joinString, _header.Data.Select(x => x).ToArray()),
                                Environment.NewLine
                    );
            }

            foreach(var row in _rows)
            {
                sb.AppendFormat(@"{0}{1}{0}{2}",
                _includeQuotes ? @"""" : "",
                string.Join(joinString, row.Data.Select(x => x).ToArray()),
                Environment.NewLine
                );
            }

            return sb.ToString();
        }
    }

    public class CsvRow
    {
        public List<string> Data { get; set; }

        public CsvRow()
        {
            Data = new List<string>();
        }

        public void AddColumn(string data)
        {
            Data.Add(data);
        }

        public void AddColumn(string[] data)
        {
            Data.AddRange(data);
        }
    }
}