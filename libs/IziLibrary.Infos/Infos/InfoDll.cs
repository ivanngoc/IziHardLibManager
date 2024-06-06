using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using IziHardGames.FileSystem.NetStd21;

namespace IziHardGames.Projects
{
    public class InfoDll : InfoBase
    {
        public const string FILE_NAME = "izhg_dlls.json";
        private Dictionary<Guid, DllRecord> keyValuePairs = new Dictionary<Guid, DllRecord>();
        public IEnumerable<DllRecord> Dlls => keyValuePairs.Values;
        public bool IsChanged { get; set; }

        public InfoDll(FileInfo info) : base(info)
        {

        }

        public static async ValueTask<InfoDll?> CreateDefaultAsync(string fullPath)
        {
            FileInfo fileInfo = new FileInfo(fullPath);
            InfoDll infoDll = new InfoDll(fileInfo);
            Guid guid = System.Guid.NewGuid();
            infoDll.SetGuidGenerated(guid);

            var dir = fileInfo.Directory!;
            var files = dir.GetFiles();

            foreach (var file in files)
            {
                if (file.Extension == ".dll")
                {
                    DllRecord dllRecord = new DllRecord()
                    {
                        guid = System.Guid.NewGuid(),
                        filename = file.Name,
                        pathRelative = UtilityForPath.AbsToRelative(dir, file.FullName),
                        pathAbsolute = file.FullName,
                    };
                    infoDll.keyValuePairs.Add(dllRecord.guid, dllRecord);
                }
            }
            var json = infoDll.ToStringJson(Shared.jOptions);
            await File.WriteAllTextAsync(fullPath, json).ConfigureAwait(false);
            return infoDll;
        }

        public string ToStringJson(JsonSerializerOptions jOptions)
        {
            JsonObject j = new JsonObject();
            JsonArray jsonArray = new JsonArray();
            foreach (var val in keyValuePairs)
            {
                jsonArray.Add(val.Value.ToJsonObject());
            }
            j["guid"] = this.GuidStruct.ToString("D");
            j["unity_dlls"] = jsonArray;
            return j.ToJsonString(jOptions);
        }

        public override async Task ExecuteAsync()
        {
            var text = await File.ReadAllTextAsync(FileInfo!.FullName).ConfigureAwait(false);
            this.Content = text;
            JsonObject j = JsonNode.Parse(text)!.AsObject();
            SetGuidFounded(System.Guid.Parse((string)j["guid"]!));
            var array = j["unity_dlls"]!.AsArray();

            for (int i = 0; i < array.Count; i++)
            {
                DllRecord dllRecord = new DllRecord(array[i]!.AsObject()!);
                keyValuePairs.Add(dllRecord.guid, dllRecord);
            }
            IsExecuted = true;
        }

        public bool TryGetByFileName(string name, out DllRecord record)
        {
            return keyValuePairs.Values.TryFindFirst(x => x.filename == name, out record);
        }
        public void Add(DllRecord record)
        {
            keyValuePairs.Add(record.guid, record);
        }
        public override string ToString()
        {
            return ToStringJson(Shared.jOptions);
        }
    }

    public class DllRecord
    {
        public Guid guid;
        public string filename = string.Empty;
        public string pathRelative = string.Empty;
        public string pathAbsolute = string.Empty;

        public DllRecord()
        {

        }
        public DllRecord(JsonObject json)
        {
            guid = Guid.Parse((string)json["guid"]!);
            filename = (string)json["filename"]!;
            pathRelative = (string)json["pathRelative"]!;
            pathAbsolute = (string)json["pathAbsolute"]!;
        }

        public JsonObject ToJsonObject()
        {
            JsonObject j = new JsonObject();
            j["guid"] = guid.ToString("D");
            j["fileName"] = filename;
            j["pathRelative"] = pathRelative;
            j["pathAbsolute"] = pathAbsolute;
            return j;
        }
    }
}