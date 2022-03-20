namespace TSB.Services.F2DService.Models.Parameters
{
    using System.Collections.Generic;

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
