namespace PowerManagerConfig
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class RevisionAttribute : Attribute
    {
        public RevisionAttribute(string revision) 
        {
            this.Revision = revision;
        }

        public string Revision { get; }
    }
}
