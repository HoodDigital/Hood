namespace Hood.Services
{
    public class FileReferenceMetadata
    {
        public bool Bundle { get; set; }
        public bool IsAsync { get; set; }
        public string Src { get; set; }
        public bool IsDefer { get; set; }

        public override bool Equals(object obj)
        {
            var item = obj as FileReferenceMetadata;

            if (item == null)
            {
                return false;
            }

            return this.Src.Equals(item.Src);
        }

        public override int GetHashCode()
        {
            return this.Src.GetHashCode();
        }
    }
}