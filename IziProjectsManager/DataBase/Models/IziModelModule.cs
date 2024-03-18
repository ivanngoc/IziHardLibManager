using System;
using System.Collections.Generic;
using IziHardGames.FileSystem.NetStd21;

namespace IziHardGames.Projects.DataBase.Models
{
    public abstract class IziFileUnit
    {
        public uint Id { get; set; }
        public IziModelModule? Module { get; set; }
        public string PathFull { get; set; } = string.Empty;
        public string PathRelative { get; set; } = string.Empty;
        /// <summary>
        /// Filename with extension
        /// </summary>
        public string Filename { get; set; } = string.Empty;
        public DateTime DateTimeCreate { get; set; }
        public DateTime DateTimeModify { get; set; }
        public IziFileUnit()
        {

        }
        protected IziFileUnit(InfoBase info)
        {
            Module = new IziModelModule(info);
            DateTimeCreate = info.DateTimeCreate;
            DateTimeModify = info.DateTimeModify;
            this.PathFull = info.FileInfo!.FullName!;
        }
    }

    public abstract class IziInProjectUnit : IziFileUnit
    {
        public string Content { get; set; } = string.Empty;
        /// <summary>
        /// Модуль/проект принадлежит третьей стороне. Не мой проект
        /// </summary>
        public bool IsThirdParty { get; set; }

        public IziInProjectUnit()
        {

        }
        public IziInProjectUnit(InfoBase info) : base(info)
        {
            Content = info.Content;
            IsThirdParty = info.IsThirdParty;
        }
    }

    public class IziModelModule
    {
        public uint Id { get; set; }
        /// <summary>
        /// <see cref="EModuleType"/>
        /// </summary>
        public uint Type { get; set; }
        /// <summary>
        /// <see cref="EModuleFlags"/>
        /// </summary>
        public long Flags { get; set; }
        /// <summary>
        /// PostgreSQL type is 'uuid'
        /// </summary>
        public Guid Guid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public IziModelModule()
        {

        }

        public IziModelModule(InfoBase info)
        {
            uint type = IziProjects.GetType(info.GetType());
            var name = info.FileInfo!.FileNameWithoutExtension();
            if (info.GuidStruct == default || string.IsNullOrEmpty(info.Guid)) throw new FormatException($"Guid Is Not set. Type:{info.GetType().FullName}");
            this.Guid = info.GuidStruct;
            this.Type = type;
            this.Name = name;
            this.Description = info.Description ?? string.Empty;
            if (info.IsRoot)
            {
                Flags |= (long)EModuleFlags.Root;
            }
        }
    }

    public class IziModelSln : IziInProjectUnit
    {
        public IziModelSln()
        {

        }
        public IziModelSln(InfoSln info) : base(info)
        {

        }
    }

    public class IziModelCsproj : IziInProjectUnit
    {
        public IziModelCsproj()
        {

        }
        public IziModelCsproj(InfoCsproj info) : base(info)
        {

        }
    }

    public class IziModelUnityMeta : IziInProjectUnit
    {
        public string GuidAssociated { get; set; } = string.Empty;
        public IziModelUnityMeta()
        {

        }
        public IziModelUnityMeta(InfoUnityMeta infoMeta) : base(infoMeta)
        {
            GuidAssociated = infoMeta.InfoAsmdef.Guid;
        }
    }

    /// <summary>
    /// <see cref="InfoAsmdef"/>
    /// </summary>
    public class IziModelUnityAsmdef : IziInProjectUnit
    {
        public bool IsNoUnityEngingeRef { get; set; }
        public IziModelUnityAsmdef()
        {

        }
        public IziModelUnityAsmdef(InfoAsmdef info) : base(info as InfoBase)
        {
            this.IsNoUnityEngingeRef = info.IsNoUnityEngineRefs;
        }
    }
    public class IziModelMeta : IziInProjectUnit
    {
        public IList<IziModelUnityAsmdef> Asmdefs { get; set; }
        public IList<IziModelCsproj> Csprojs { get; set; }

        public IziModelMeta()
        {
            IsThirdParty = false;
        }
        public IziModelMeta(InfoIziProjectsMeta meta) : base(meta as InfoBase)
        {
            IsThirdParty = false;
        }
    }

    public class IziModelUnityPackageJson : IziInProjectUnit
    {
        public string PackageName { get; set; } = string.Empty;
        public string PackageVersion { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        public IziModelUnityPackageJson()
        {

        }
        public IziModelUnityPackageJson(InfoPackageJson info) : base(info)
        {
            PackageName = info.PackageName;
            PackageVersion = info.PackageVersion;
            DisplayName = info.DisplayName;
        }
    }

    public class IziModelCsprojNesting
    {
        public uint Id { get; set; }
        public IziModelCsproj? Root { get; set; }
        public List<IziModelCsproj> Nests { get; set; } = new List<IziModelCsproj>();
    }

    public class IziModelDependecy : IziFileUnit
    {
        public string Version { get; set; } = string.Empty;

        public IziModelDependecy()
        {

        }

        public IziModelDependecy(InfoDependecy info)
        {
            Module = new IziModelModule(info);
        }
    }

    /// <summary>
    /// Не путать с <see cref="IziModelDependecy"/>.
    /// Relation это соответствие одного модуля другому. при этом модуль не используется как зависимость.
    /// </summary>
    public class IziModelRelation
    {
        public uint Id { get; set; }
        public IziModelModule? From { get; set; }
        public IziModelModule? To { get; set; }
        /// <summary>
        /// <see cref="ERelationsFlags"/>
        /// </summary>
        public long RelationFlags { get; set; }

        public IziModelRelation()
        {

        }
    }

    public class IziModelInfoDll : IziFileUnit
    {
        public IziModelInfoDll()
        {

        }
        public IziModelInfoDll(InfoDll infoDll) : base(infoDll)
        {

        }
    }

    public class IziModelDll : IziFileUnit
    {
        public IziModelDll()
        {

        }
        public IziModelDll(DllRecord item)
        {
            this.Module = new IziModelModule()
            {
                Guid = item.guid,
            };
            this.PathFull = item.pathAbsolute;
            this.PathRelative = item.pathRelative;
            this.Filename = item.filename;
        }
    }

    public class IziModelRelationsForCsprojAndAsmdef
    {
        public uint Id { get; set; }
        public IziModelCsproj? Csproj { get; set; }
        public IziModelUnityAsmdef? Asmdef { get; set; }
    }
}
