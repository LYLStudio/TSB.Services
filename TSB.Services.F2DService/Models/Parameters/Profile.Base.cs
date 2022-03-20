namespace TSB.Services.F2DService.Models.Parameters
{
    using System.Collections.Generic;

    public class ProfileBase
    {
        public string ProfileName { get; set; }
        public string TableName { get; set; }
        public List<string> Columns { get; set; }
        public List<string> ColumnsHis { get; set; }

        public ProfileBase()
        {
            Columns = new List<string>();
            ColumnsHis = new List<string>();
        }
    }
}
