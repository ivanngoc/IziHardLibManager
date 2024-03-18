using System.Text.Json;

namespace IziHardGames.Projects
{
	public static class Shared
    {
        public static readonly JsonSerializerOptions jOptions = new JsonSerializerOptions() { WriteIndented = true };
    }
}