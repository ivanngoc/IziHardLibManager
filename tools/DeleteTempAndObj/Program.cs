namespace DeleteTempAndObj
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var env = Environment.GetEnvironmentVariable("");

            //var dir = "C:\\Users\\ivan\\Documents\\.izi_modules\\modules";
            var dir = "C:\\Users\\ivan\\Documents\\.unity\\GameProject5\\Assets\\modules";
            var di = new DirectoryInfo(dir);
            DeleteTempAndObj(di);
        }

        public static void DeleteTempAndObj(DirectoryInfo di)
        {
            if (di.Exists)
            {
                var subdirs = di.GetDirectories();
                if (di.GetFiles().Where(x => x.Extension == ".csproj").Any())
                {
                    var toDelete = subdirs.Where(x => x.Name == "obj" || x.Name == "bin");
                    foreach (var del in toDelete)
                    {
                        del.Delete(true);
                        Console.WriteLine($"Deleted: {del.FullName}");
                    }
                }
                foreach (var item in subdirs)
                {
                    DeleteTempAndObj(item);
                }
            }
        }
    }
}
