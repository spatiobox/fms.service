using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.Core
{
    public static class Paths
    {
        //public static string LoginPath { get; set; }

        //public static string LogoutPath { get; set; }

        //public static string AuthorizePath { get; set; }

        //public static string TokenPath { get; set; }



        /// <summary>
        /// AuthorizationServer project should run on this URL
        /// </summary>
        public const string AuthorizationServerBaseAddress = "http://localhost:9000";

        /// <summary>
        /// ResourceServer project should run on this URL
        /// </summary>
        public const string ResourceServerBaseAddress = "http://localhost:8008";

        /// <summary>
        /// ImplicitGrant project should be running on this specific port '38515'
        /// </summary>
        public const string ImplicitGrantCallBackPath = "http://localhost:3000/";

        /// <summary>
        /// AuthorizationCodeGrant project should be running on this URL.
        /// </summary>
        public const string AuthorizeCodeCallBackPath = "http://localhost:3000/";

        public const string AuthorizePath = "/OAuth/Authorize";
        public const string TokenPath = "/OAuth/Token";
        public const string LoginPath = "/Account/Login";
        public const string LogoutPath = "/Account/Logout";
        public const string MePath = "/api/Identity";
    }
}