using Sanity.Linq;
using Sanity.Linq.CommonTypes;
using Sanity.Linq.Extensions;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanityTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Obtain the slug from browser request
            // /terms-conditions-sanity will return, /terms-conditions-sanity?test=param should work too.
            string slugToGet = "/terms-conditions-sanity";

            // Generate options
            var options = new SanityOptions
            {
                ProjectId = "{PROJECTID}",
                Dataset = "{DATASET}",
                Token = "{TOKEN}",
                UseCdn = false,
                ApiVersion = "v1"
            };

            // Create Sanity Context with options
            var sanity = new SanityDataContext(options);

            // Use a custom serializer for HTML, lets us change "section-header" to <h2 class="section-header">, or add ID attributes.
            CustomSerializer customSerializer = new CustomSerializer();
            sanity.AddHtmlSerializer("block", customSerializer.SerializeBlockAsync);
            sanity.AddHtmlSerializer("image", customSerializer.SerializeCustomImageAsync);

            // Use LINQ to obtain the first page that matches
            var pages = sanity.DocumentSet<Webpage>();
            var webpage = await pages.Where(p => p.Slug.current == slugToGet).FirstOrDefaultAsync();

            // Use HtmlBuilder to generate the HTML for sections (Body and Images)
            string htmlBody = await sanity.HtmlBuilder.BuildAsync(webpage.Body);
            string mainImageHtml = webpage.MainImage is null ? "" : await webpage.MainImage.ToHtmlAsync(sanity.HtmlBuilder);

            // Generate HTML yourself from the strings, pass to frontend if exists
            var pageTitle = webpage.Title;
            var ogImageUrl = GetImageUrl(webpage.ogImage, options);

            Console.WriteLine("Debug Breakpoint here");
        }

        public static string GetImageUrl(SanityImage image, SanityOptions options)
        {
            if (image != null)
            {
                string imageRef = image.Asset.Ref;
                var imageParts = imageRef.Split('-');
                var url = new StringBuilder();
                url.Append("https://cdn.sanity.io/");
                url.Append(imageParts[0] + "s/");            // images/
                url.Append(options.ProjectId + "/");             // projectid/
                url.Append(options.Dataset + "/");             // dataset/
                url.Append(imageParts[1] + "-");             // asset id-
                url.Append(imageParts[2] + ".");             // dimensions.
                url.Append(imageParts[3]);                       // file extension
                return url.ToString();
            }
            return "";
        }
    }
}
