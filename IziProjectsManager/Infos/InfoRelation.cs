using System;
using IziHardGames.Projects.DataBase.Models;

namespace IziHardGames.Projects
{
	/// <summary>
	/// <see cref="IziModelRelation"/>
	/// </summary>
	public class InfoRelation
    {
        public Guid guid;
        public InfoBase? from;
        public InfoBase? to;
        public ERelationsFlags flags;
    }
}