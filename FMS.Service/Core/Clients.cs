using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.Core
{
    public static class Clients
    {
        //public static Client Client1 { get; set; }

        //public static Client Client2 { get; set; }


        public readonly static Client Client1 = new Client
        {
            Id = "123456",
            Secret = "abcdef",
            RedirectUrl = Paths.AuthorizeCodeCallBackPath
        };

        public readonly static Client Client2 = new Client
        {
            Id = "7890ab",
            Secret = "7890ab",
            RedirectUrl = Paths.ImplicitGrantCallBackPath
        };

    }

    public class Client
    {
        public string Id { get; set; }

        public string Secret { get; set; }

        public string RedirectUrl { get; set; }
    }
}