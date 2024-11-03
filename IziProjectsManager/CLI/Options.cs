using CommandLine;

namespace IziHardGames.Projects
{
    public class Options
    {
        [Option('n', "name", Required = true, HelpText = "Set the name.")]
        public string Name { get; set; }

        [Option('a', "age", Required = false, HelpText = "Set the age.")]
        public int Age { get; set; }
    }
}
