namespace Hood.Services
{
    public class FileReferenceMetadata
    {
        public bool ExcludeFromBundle { get; set; }
        public bool IsAsync { get; set; }
        public string Src { get; set; }
        public string DebugSrc { get; set; }
        public bool IsDefer { get; set; }

        public bool Equals(FileReferenceMetadata item)
        {
            if (item == null)
                return false;
            return Src.Equals(item.Src) && DebugSrc.Equals(item.DebugSrc);
        }
        public override int GetHashCode()
        {
            return Src == null ? 0 : Src.GetHashCode();
        }

    }
}