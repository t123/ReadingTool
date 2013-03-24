using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public interface IParserService
    {
        string Parse(bool asParallel, Language l1Language, Language l2Language, Term[] terms, Text text);
    }
}
