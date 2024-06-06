namespace IziHardGames.Projects
{
	public enum ERefType
    {
        None = 0,
        /// <summary>
        /// Ref to another .csproj
        /// </summary>
        NetSdkProjectReference,
        NetSdkReference,
        NetSdkCOMReference,
        NetSdkNativeReference,
        NetSdkContent,
        SlnCsproj,
    }
}