﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace Legendary.Pages
{
    public class Logout : Page
    {
        public Logout(Core LegendaryCore) : base(LegendaryCore)
        {
        }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            S.User.LogOut();

            return Redirect("/login");
        }
    }
}
