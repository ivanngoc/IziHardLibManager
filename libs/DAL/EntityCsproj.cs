﻿using System.Collections.Generic;
using System.Text.Json.Serialization;
using IziLibrary.Database.DataBase.EfCore;

namespace IziHardGames.DotNetProjects
{
    public class EntityCsproj : IziEntity
    {
        public CsprojId EntityCsprojId { get; set; }
        public ICollection<CsProjectAtDevice> CsProjectAtDevices { get; set; } = null!;
        public ICollection<CsprojRelation> AsChild { get; set; } = null!;
        public ICollection<CsprojRelation> AsParent { get; set; } = null!;
    }

    public class CsprojRelationAtDevice
    {
        public int Id { get; set; }
        public int RelationId { get; set; }
        public Guid DeviceId { get; set; }
        public string Include { get; set; } = null!;

        public Device Device { get; set; } = null!;
        [JsonIgnore] public CsprojRelation Relation { get; set; } = null!;
    }

    public class CsprojRelation
    {
        public int Id { get; set; }
        public CsprojId? ParentId { get; set; }
        public CsprojId? ChildId { get; set; }
        public EntityCsproj? Parent { get; set; }
        public EntityCsproj? Child { get; set; }
        public ICollection<CsprojRelationAtDevice> RelationsAtDevice { get; set; }
    }
}