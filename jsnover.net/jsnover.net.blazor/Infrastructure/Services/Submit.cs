using jsnover.net.blazor.DataTransferObjects.Common;
using jsnover.net.blazor.Infrastructure.SqlRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace jsnover.net.blazor.Infrastructure.Services
{
    public class Submit
    {
        private readonly EmailService _emailService;

        public Submit(EmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task<bool> SubmitContactRequest(ContactModel contactRequest)
        {
            if (RegexUtilities.IsValidEmail(contactRequest.Email))
            {
                var saved = await JsnoRepo.SubmitContactRequest(ContactModel.MapToDto(contactRequest));
                if (saved)
                    await _emailService.NotifySnover(contactRequest);

                return saved;
            }
            return false;
        }
    }
}
