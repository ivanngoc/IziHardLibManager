namespace IziHardGames.Projects.DataBase
{
    public static class ConstantsForIziProjects
    {
        public const string DEPENDECIES_FOLDER = "~izhg-dependecies.tmp~";
        public const string ConnectionString = "Host=localhost;Database=IziProjects;Username=postgres;Password=postgres";
        public const string VALUE_AUTHOR = "Tran Ngoc Anh";

        public class Templates
        {
            public const string PACK_JSON_UNITY = InfoPackageJson.FILE_NAME;
            public const string PACK_JSON_UNITY_META = InfoPackageJson.FILE_NAME_META;
            public const string ASMDEF = "asmdef";
            public const string ASMDEF_META = "asmdef_meta";
            public const string CSPROJ = "csproj";
        }

        public class ForCsproj
        {
            public const string PROP_GUID = "ProjectGuid";
            public const string PROP_PROJ_NAME = "ProjectName";


            public const string EL_DEPENDECY_GUID = "Guid";
            public const string EL_DEPENDECY_PROJ_NAME = "ProjectName";
            public const string EL_DEPENDECY_PROJ_TAG = "IziTag";

            public const string ITEM_DEPEND_PROJ_REF = "ProjectReference";
        }

        public class Paths
        {
            public const string LIB_FOLDER = "C:\\.izhg-lib";
        }
        public class UnariCommands
        {
            public const string FIX_JUNCT = "fix-junctions";
        }
    }
}
