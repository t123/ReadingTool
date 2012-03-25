#region License
// EmailService.cs is part of ReadingTool.Services
// 
// ReadingTool.Services is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Services is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Services. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMongo.Linq;
using MongoDB.Driver;
using ReadingTool.Common;
using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public interface IEmailService
    {
        void ForgotPassword(User user, PasswordReset reset);
        EmailTemplate FindOne(string name);
        IEnumerable<EmailTemplate> FindAll();
        void Save(EmailTemplate template);
        void ResetPasswordConfirmation(User user);
        long QueuedCount();
    }

    public class EmailService : IEmailService
    {
        private readonly MongoDatabase _db;
        private readonly SystemSystemValues _values;

        public EmailService(MongoDatabase db, SystemSystemValues values)
        {
            _db = db;
            _values = values;
        }

        public void ForgotPassword(User user, PasswordReset reset)
        {
            var template = FindOne(EmailTemplate.TemplateNames.ForgotPassword);

            if(template == null)
            {
                throw new NoNullAllowedException(string.Format("Template {0} not found"));
            }

            var email = new Email();
            email.To = user.EmailAddress;
            email.Subject = string.Format(template.Subject, _values.Site.NiceName);
            email.Body = string.Format(
                template.Body,
                _values.Site.NiceName,
                reset.ResetKey,
                _values.Site.Domain,
                user.Username,
                reset.Created.AddHours(48).ToString(_values.Formats.LongDateFormat + " " + _values.Formats.Time24Format)
                );

            QueueEmail(email);
        }

        public void ResetPasswordConfirmation(User user)
        {
            var template = FindOne(EmailTemplate.TemplateNames.PasswordReset);

            if(template == null)
            {
                throw new NoNullAllowedException(string.Format("Template {0} not found"));
            }

            var email = new Email();
            email.To = user.EmailAddress;
            email.Subject = string.Format(template.Subject, _values.Site.NiceName);
            email.Body = string.Format(
                template.Body,
                _values.Site.NiceName,
                user.Username
                );

            QueueEmail(email);
        }

        public long QueuedCount()
        {
            return _db.GetCollection<Email>(Collections.Emails)
                .AsQueryable()
                .Count(x => x.Sent == null);
        }

        public EmailTemplate FindOne(string name)
        {
            return _db.GetCollection<EmailTemplate>(Collections.EmailTemplates)
                .AsQueryable()
                .FirstOrDefault(x => x.Name == name);
        }

        public IEnumerable<EmailTemplate> FindAll()
        {
            return _db.GetCollection<EmailTemplate>(Collections.EmailTemplates)
                .AsQueryable()
                .OrderBy(x => x.Name);
        }

        public void Save(EmailTemplate template)
        {
            _db.GetCollection(Collections.EmailTemplates).Save(template);
        }

        private void QueueEmail(Email email)
        {
            _db.GetCollection(Collections.Emails).Save(email);
        }
    }
}
