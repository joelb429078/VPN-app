using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VPNProject.ViewModel
{
    public class ServerModel
    {
        public int ID {  get; set; }
        public string Username { get; set; }

        public string Password { get; set; }

        public string Server { get; set; }

        public string Country { get; set; }

        public string FlagImageUrl { get; set; }



        /*extras for adding further more customised servers
        public string Email { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        public string PasswordSalt { get; set; } = string.Empty;

        public int Port { get; set; }   
        public int PortNumber { get; set; }*/
    }
}
