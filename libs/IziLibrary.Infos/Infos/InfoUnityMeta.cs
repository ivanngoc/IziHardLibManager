using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace IziHardGames.Projects
{
    public class InfoUnityMeta : InfoBase
    {
        private InfoAsmdef infoAsmdef;
        public InfoAsmdef InfoAsmdef => infoAsmdef;
        public const string EXTENSION = ".meta";

        public InfoUnityMeta(FileInfo info, InfoAsmdef infoAsmdef) : base(info)
        {
            this.infoAsmdef = infoAsmdef;
        }

        public override async Task ExecuteAsync()
        {
            var metaString = await File.ReadAllTextAsync(FileInfo.FullName);
            Content = metaString;
            SetGuidGenerated(System.Guid.NewGuid());

            var splits = metaString.Split('\n');

            foreach (var line in splits)
            {
                var entry = line.TrimStart();
                if (entry.StartsWith("guid"))
                {
                    string guid = entry.Split(':')[1].Trim();
                    var guidAsStruct = System.Guid.Parse(guid);
                    SetGuidFounded(guidAsStruct);
                    Console.WriteLine($"Guid:{guid}");
                }
                else
                {

                }
            }
        }

        public void PutRelation(List<InfoRelation> result)
        {
            var connection = new InfoRelation()
            {
                from = this,
                to = infoAsmdef,
                flags = ERelationsFlags.None,
            };
            result.Add(connection);
        }

        public async Task OverrideGuidInFileAsync(Guid guidStruct)
        {
            SetGuid(guidStruct);
            string text = await File.ReadAllTextAsync(FileInfo.FullName).ConfigureAwait(false);

            if (OverrideGuid(text, guidStruct, out var newText))
            {
                goto FINISH;
            }
            throw new FormatException($"This file must have guid line. Content:{Environment.NewLine}{text}");
            FINISH:
            {
                await File.WriteAllTextAsync(FileInfo.FullName, newText);
                this.Content = await File.ReadAllTextAsync(FileInfo.FullName).ConfigureAwait(false);
            }
        }

        public static bool OverrideGuid(string text, Guid guid, out string newText)
        {
            var lines = text.Split('\n');
            newText = text;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("guid", StringComparison.InvariantCultureIgnoreCase))
                {
                    lines[i] = $"guid: {guid.ToString("N")}";
                    newText = lines.Aggregate((x, y) => x + '\n' + y) + '\n';
                    return true;
                }
            }
            return false;
        }

        public static async Task SetGuidAsync(FileInfo fileInfo, Guid guid)
        {
            string text = await File.ReadAllTextAsync(fileInfo.FullName).ConfigureAwait(false);
            if (OverrideGuid(text, guid, out var newText))
            {
                await File.WriteAllTextAsync(fileInfo.FullName, newText);
            }
            else
            {
                throw new FormatException(text);
            }
        }
        public static async ValueTask<Guid> GetGuidAsync(FileInfo meta)
        {
            string text = await File.ReadAllTextAsync(meta.FullName);
            var lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("guid"))
                {
                    return System.Guid.Parse(lines[i].Split(':')[1]);
                }
            }
            throw new System.FormatException(text);
        }
    }
}