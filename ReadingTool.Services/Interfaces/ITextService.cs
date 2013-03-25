using System;
using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public interface ITextService
    {
        void Save(Text text);
        void Delete(long id);
        Text FindOne(long id);
        int Import(TextImport import);
    }
}