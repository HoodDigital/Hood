using Hood.Extensions;
using Hood.Interfaces;
using Newtonsoft.Json;
namespace Hood.Models.Api
{
    public class MetaDataApi<TMetaData>
        where TMetaData : IMetadata
    {
        public int Id { get; set; }

        // Content
        public int ContentId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public MetaDataApi()
        {
        }

        public MetaDataApi(TMetaData cm)
        {
            if (cm == null)
                return;
            cm.CopyProperties(this);
            Value = cm.BaseValue;
        }

        public override string ToString()
        {
            try
            {
                return JsonConvert.DeserializeObject<string>(Value);
            }
            catch
            {
                if (!Value.IsSet())
                {
                    return "";
                }
                else
                {
                    return JsonConvert.DeserializeObject<string>(JsonConvert.SerializeObject(Value));
                }
            }
        }


    }

}
