using System;
using Zen.Base.Extension;

namespace Zen.Base.Distributed
{
    public static class Factory
    {
        public static Ticket GetTicket(string serviceDescriptor, TimeSpan timeOut = default)
        {
            // First, let's check if a ticket already exists for that Locator:

            var key = serviceDescriptor.Sha512Hash();

            var ret = Ticket.Get(key);
            var mustCreate = false;

            if (ret == null)
            {
                // Current.Log.Add(serviceDescriptor + ": no ticket, creating.");
                mustCreate = true;
            }
            else
            {
                if (ret.TimeOut <= DateTime.Now)
                {
                    mustCreate = true;
                    ret.Stop("Timed out");
                }
            }

            if (!mustCreate) return ret;

            if (timeOut.Equals(default)) timeOut = new TimeSpan(0, 0, 5, 0);

            ret = new Ticket
            {
                Id = key,
                ServiceDescriptor = serviceDescriptor,
                TimeOut = DateTime.Now.Add(timeOut),
                Comments = "Ticket created"
            };

            ret = ret.Save();

            return ret;
        }
    }
}