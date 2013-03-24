using System;
using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public interface ITextService
    {
        void Save(Text text);
        void Delete(Guid id);
        Text FindOne(Guid id);
        int Import(TextImport import);
    }
}