using jsnover.net.blazor.Models;
using System;
using System.Threading.Tasks;

namespace jsnover.net.blazor.Infrastructure.SqlRepo
{
    public class JsnoRepo
    {
        public static async Task<bool> SubmitContactRequest(ContactRequest contactRequest)
        {
            try
            {
                using var db = new jsnoverdotnetdbContext();
                db.ContactRequest.Add(contactRequest);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
