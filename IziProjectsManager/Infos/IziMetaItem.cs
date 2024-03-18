using System;
using System.Text.Json.Nodes;

namespace IziHardGames.Projects
{
    /// <summary>
    /// <see cref="InfoIziProjectsMeta"/>
    /// </summary>
    public class IziMetaItem
    {
        public const string PROP_GUID = "guid";
        public const string PROP_FNAME = "fileName";
        public const string PROP_PATH_REL = "pathRelative";


        public Guid guid;
        public string fileName = string.Empty;
        public string pathRelative = string.Empty;

        public IziMetaItem()
        {

        }
        public IziMetaItem(JsonObject jObj)
        {
            var nodeGuid = jObj[PROP_GUID] ?? throw new NullReferenceException("No property GUID founded");
            var nodeFileName = jObj[PROP_FNAME] ?? throw new NullReferenceException("No property GUID founded");
            var nodePathRelative = jObj[PROP_PATH_REL] ?? throw new NullReferenceException("No property GUID founded");

            guid = Guid.Parse((string)nodeGuid! ?? throw new NullReferenceException());
            fileName = (string)nodeFileName! ?? throw new NullReferenceException();
            pathRelative = (string)nodePathRelative! ?? throw new NullReferenceException();
        }

        public IziMetaItem(Guid guidStruct, string fileName, string pathRelative)
        {
            this.guid = guidStruct;
            this.fileName = fileName;
            this.pathRelative = pathRelative;
        }

        internal JsonObject AsJsonObject()
        {
            JsonObject j = new JsonObject();
            j[PROP_GUID] = guid.ToString("D");
            j[PROP_FNAME] = fileName;
            j[PROP_PATH_REL] = pathRelative;
            return j;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guidStruct"></param>
        /// <param name="filename"></param>
        /// <param name="pathRelative"></param>
        /// <returns>
        /// <see langword="true"/> - all fields equal. No Diff detected
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        internal bool Ensure(Guid guidStruct, string filename, string pathRelative)
        {
            var result = this.guid == guidStruct && this.fileName == filename && this.pathRelative == pathRelative;
            this.guid = guidStruct;
            this.fileName = filename;
            this.pathRelative = pathRelative;
            return result;
        }
    }
}