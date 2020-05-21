namespace TSB.Services.F2DService.Models.Parameters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CurFxDepProfile : ProfileBase
    {
        public List<CodeMappingInfo> CodeMapping { get; set; }

        public CurFxDepProfile()
        {
            CodeMapping = new List<CodeMappingInfo>();
        }

        public class CodeMappingInfo
        {
            public string Code { get; set; }
            public List<string> Tags { get; set; }

            public CodeMappingInfo()
            {
                Tags = new List<string>();
            }
        }
    }
}
