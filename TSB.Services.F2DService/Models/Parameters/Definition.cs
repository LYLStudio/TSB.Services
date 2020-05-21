namespace TSB.Services.F2DService.Models.Parameters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public partial class Definition
    {
        public string Folder { get; set; }
        public List<Profile> Profiles { get; set; }

        public Definition()
        {
            Profiles = new List<Profile>();
        }
    }
}
