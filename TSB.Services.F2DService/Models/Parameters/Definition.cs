namespace TSB.Services.F2DService.Models.Parameters
{
    using System.Collections.Generic;

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
