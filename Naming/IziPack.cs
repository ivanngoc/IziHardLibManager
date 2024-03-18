using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IziHardGames.Naming
{
    public abstract class IziPackSerializer
    {
        public abstract string Serialize(IziPack iziPack);

    }

    public class IziPackSerializer10 : IziPackSerializer
    {
        public override string Serialize(IziPack iziPack)
        {
            return
                $"{nameof(IziPack.SyntaxVersion)}: {iziPack.SyntaxVersion}{Environment.NewLine}" +
                 $"{nameof(IziPack.TimeCreate)}: {iziPack.TimeCreate}{Environment.NewLine}" +
                 $"{nameof(IziPack.timeModify)}: {iziPack.timeModify}{Environment.NewLine}" +
                 $"{nameof(IziPack.NameSchema)}: {iziPack.NameSchema}{Environment.NewLine}" +
                 $"{nameof(IziPack.guidPack)}: {iziPack.guidPack}{Environment.NewLine}" +
                 $"{nameof(IziPack.mainPurpose)}: {iziPack.mainPurpose}{Environment.NewLine}" +
                 $"{nameof(IziPack.abstraction)}: {iziPack.abstraction}{Environment.NewLine}" +
                 $"{nameof(IziPack.platform)}: {iziPack.platform}{Environment.NewLine}" +
                 $"";
        }
    }

    /// <summary>
    /// Пакет это функциональная единица библиотек. 
    /// Внутри пакета все модули являются частью одного но могут быть как в подчиненных отношения, так и быть разными формами одного и того же.
    /// </summary>
    public class IziPack
    {
        public const string fileExtension = "izipack";
        public const string separatorNameInit = "_";
        public const string separatorWord = ".";
        public const string separatorWordAlt = "-";

        public string? SyntaxVersion { get; set; } = "1.0";
        public DateTime TimeCreate { get; set; } = DateTime.Now;
        public DateTime timeModify { get; set; } = DateTime.Now;
        /// <summary>
        /// Имя для воссоздания <see cref="Name"/>
        /// </summary>
        public string? NameSchema { get; set; }
        public MappedName? Name { get; set; }

        public Guid guidPack { get; set; }

        public string? languages { get; set; }
        public string? ides { get; set; }

        // no whitespace allowed
        public string? mainPurpose { get; set; }
        public string? abstraction { get; set; }
        public string? platform { get; set; }
        public NamingPolicy? policy { get; set; }
        public string? rootFolder { get; set; }
        public List<Entry> entries { get; set; } = new List<Entry>();
        public string Serilize(IziPackSerializer serializer) => serializer.Serialize(this);
    }

    public class Entry
    {

    }

    public abstract class NamingPolicy
    {
        public NamingPolicy? versionNext;
        public NamingPolicy? versionPrev;
    }

    public class IziModuleName
    {
        public const string fileExtension = "izimod";

        public string? syntaxVersion;
        public DateTime timeCreate;
        public DateTime timeModify;
        /// <summary>
        /// Имя для воссоздания <see cref="name"/>
        /// </summary>
        public string? nameSchema;
        public MappedName? name;


        public Guid guid;
        public string? languages;
        public string? platform;

        public byte[] Serulize() => throw new NotImplementedException();
    }

    /// <summary>
    /// структурированное имя
    /// имена чередуются с сепараторами. Количество <see cref="separators"/> всегда на 1 меньше чем <see cref="units"/>
    /// </summary>
    public class MappedName
    {
        public NameUnit[]? units;
        public NameSeparator[]? separators;
    }

    public abstract class NameUnit
    {
        public string? desription;
        public string[]? options;
        public readonly List<NamingRule> namingRules = new List<NamingRule>();
    }
    public abstract class NameSeparator
    {
        public string? desription;
    }

    /// <summary>
    /// При переходе с одной версии правил именования на другую версию правила или на другую конвенцию нужно пропатчить проект с предыдущей версии на другую.
    /// Возможно каскадное обновление
    /// </summary>
    public abstract class NamingPatcher
    {

    }

    /// <summary>
    /// Unity/NetMono/NetCore/NetStd21
    /// </summary>
    public sealed class Runtime : NameUnit
    {

    }
    /// <summary>
    /// 
    /// Android/Linux/Windows/iOS
    /// </summary>
    public sealed class OperatingSystem : NameUnit
    {

    }

    /// <summary>
    /// PC/ARM/Console/NintendoSwitch/Mobile Android
    /// Device-specific platform
    /// </summary>
    public sealed class DeviceSpecificPlatform : NameUnit
    {

    }

    public sealed class CompanyNameShort : NameUnit
    {

    }
    public sealed class CompanyNameFull : NameUnit
    {

    }

    public abstract class NamingRule
    {

    }

    public sealed class KebabCase : NamingRule
    {

    }

    public sealed class LowerCase : NamingRule
    {

    }
}
