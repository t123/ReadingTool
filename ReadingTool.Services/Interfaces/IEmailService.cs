using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public interface IEmailService
    {
        void ResetPasswordInstructions(User user);
        void ResetSuccess(User user);
    }
}
