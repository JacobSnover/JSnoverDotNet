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
        public static async Task<bool> SubmitContactRequest(ContactModel contactRequest)
        {
            if (RegexUtilities.IsValidEmail(contactRequest.Email))
            {
                try
                {
                    await EmailService.NotifySnover(contactRequest);
                }
                catch (Exception)
                {

                }
                
                return await JsnoRepo.SubmitContactRequest(ContactModel.MapToDto(contactRequest));
            }
            return false;
        }
    }
}
