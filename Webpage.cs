using Newtonsoft.Json;
using Sanity.Linq.CommonTypes;

namespace SanityTest
{
    public class Webpage
    {
        [JsonProperty("_id")]
        public string WebpageId { get; set; }

        [JsonProperty("_type")]
        public string DocumentType => "page";

        public string Title { get; set; }
        public SanityImage MainImage { get; set; }

        public Slug Slug { get; set; }

        [JsonProperty("breadcrumb")]
        public string BreadCrumb { get; set; }

        [JsonProperty("tagline")]

        public string TagLine { get; set; }
        public string seoTitle { get; set; }
        public string seoDescription { get; set; }
        public string ogTitle { get; set; }
        public string ogDescription { get; set; }
        public SanityImage ogImage { get; set; }
        public dynamic Body{ get; set; }
    }

    public class Slug
    {
        public string _type { get; set; }
        public string current { get; set; }
    }
}
