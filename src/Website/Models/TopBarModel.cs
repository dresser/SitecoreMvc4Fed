using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class TopBarModel
    {
        public string Something { get; set; } = null;

        public TopBarModel(string arg)
        {
            var a = arg.Length;
        }
    }
}