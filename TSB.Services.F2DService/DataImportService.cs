namespace TSB.Services.F2DService
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.ServiceProcess;
    using System.Text;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using TSB.Services.F2DService.Models.Parameters;
    using TSB.Services.F2DService.Operations;

    public partial class DataImportService : ServiceBase
    {
        private Definition _definition;
        private List<ProfileBase> _profiles;

        private readonly DbImportOperatorManager _manager;
        public DataImportService()
        {
            InitializeComponent();
            _manager = new Operations.DbImportOperatorManager();
            Reload();
            fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            fileSystemWatcher.Path = _definition.Folder;
        }

        private async void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            switch (e.Name)
            {
                case "RateX.txt":
                    {
                        var defProfile = _definition.Profiles.FirstOrDefault(o => o.ProfileName.Equals(e.Name));
                        var rateXPath = Path.Combine(_definition.Folder, defProfile.SourceFileName);
                        var rateX = File.ReadAllLines(rateXPath);
                        var list = rateX.Where(o => !string.IsNullOrWhiteSpace(o)).ToList();
                        var date = list.Last().Split('\t')[0];
                        var valueList = new List<object>();
                        var valueParamList = new List<string>();

                        if (DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                        {
                            list.Remove(list.LastOrDefault());
                            var profile = (CurFxDepProfile)_profiles.FirstOrDefault(o => o.ProfileName.Equals(defProfile.ProfileName));
                            //var cmd = $"INSERT INTO {profile.TableName} ({string.Join(",", profile.Columns)}) VALUES ";
                            var cmd = $"INSERT INTO {profile.TableName} VALUES ";
                            var ii = 0;
                            list.ForEach(o =>
                            {
                                var row = o.Split('\t');
                                var code = profile.CodeMapping.FirstOrDefault(c => c.Tags.Contains(row[0])).Code;
                                var currency = row[0].Substring(0, 3);
                                var desc = row[0].Substring(3);
                                var rate = decimal.Parse(row[1]);
                                valueList.Add(date);
                                valueList.Add(code);
                                valueList.Add(currency);
                                valueList.Add(desc);
                                valueList.Add(rate);

                                var val = "";
                                for (int i = valueList.Count; i > valueList.Count - 5; i--)
                                {
                                    if (i != valueList.Count - 4)
                                    {
                                        val += $"@p{ii},";
                                    }
                                    else
                                    {
                                        val += $"@p{ii}";
                                    }
                                    ii++;
                                }
                                val = $"({val})";
                                valueParamList.Add(val);
                            });
                            cmd += $"{string.Join(",", valueParamList)};";

                            var result = await _manager.SetDataAsync(profile.ProfileName, cmd, valueList.ToArray());
                        }
                        else
                        {
                            //throw new ArgumentException($"{e.Name} format is not valid");                            
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void Reload()
        {
            _profiles = new List<ProfileBase>();

            var definitionRaw = File.ReadAllText(Properties.Settings.Default.PATH_DEFINITION);
            _definition = JsonConvert.DeserializeObject<Definition>(definitionRaw);

            Parallel.ForEach(_definition.Profiles, o =>
            {
                var profileRaw = File.ReadAllText(o.FileName);
                ProfileBase profile = null;
                if (o.ProfileName.Equals("RateX.txt"))
                {
                    profile = JsonConvert.DeserializeObject<CurFxDepProfile>(profileRaw);
                }
                else
                {
                    profile = JsonConvert.DeserializeObject<ProfileBase>(profileRaw);
                }
                _profiles.Add(profile);
            });
        }

        protected override void OnStart(string[] args)
        {
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        protected override void OnStop()
        {
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        protected override void OnContinue()
        {
            base.OnContinue();
        }

#if DEBUG
        internal void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
            this.OnStop();
        }
#endif
    }
}
