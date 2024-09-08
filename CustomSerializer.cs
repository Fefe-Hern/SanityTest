using Newtonsoft.Json.Linq;
using Sanity.Linq;
using Sanity.Linq.BlockContent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanityTest
{
    public class CustomSerializer : SanityHtmlSerializers
    {
        /// <summary>
        /// Try to serialize the special "section-header" style. Else fallback to default overloaded serializer.
        /// </summary>
        /// <returns>The class="section-header" block, or the original serialized result.</returns>
        public Task<string> SerializeBlockAsync(JToken input, SanityOptions sanity, object context = null)
        {
            var text = new StringBuilder();
            var tag = "";
            tag = input["style"]?.ToString() ?? "span";
            if(tag.Equals("section-header"))
            {
                var markDefs = input["markDefs"];

                // iterate through children and apply marks and add to text
                foreach (var child in input["children"])
                {
                    var start = new StringBuilder();
                    var end = new StringBuilder();

                    if (child["marks"] != null && child["marks"].HasValues)
                    {
                        foreach (var mark in child["marks"])
                        {
                            var sMark = mark?.ToString();
                            var markDef = markDefs?.FirstOrDefault(m => m["_key"]?.ToString() == sMark);
                            if (markDef != null)
                            {
                                if (TrySerializeMarkDef(markDef, context, ref start, ref end))
                                {
                                    continue;
                                }
                                else if (markDef["_type"]?.ToString() == "link")
                                {
                                    start.Append($"<a target=\"_blank\" href=\"{markDef["href"]?.ToString()}\">");
                                    end.Append("</a>");
                                }
                                else if (markDef["_type"]?.ToString() == "internalLink")
                                {
                                    start.Append($"<a href=\"{markDef["href"]?.ToString()}\">");
                                    end.Append("</a>");
                                }
                                else
                                {
                                    // Mark not supported....
                                }
                            }
                            else
                            {
                                // Default
                                start.Append($"<{mark}>");
                                end.Append($"</{mark}>");
                            }
                        }
                    }

                    text.Append(start.ToString() + child["text"] + end.ToString());
                }

                var result = $"<h2 class=\"section-header\">{text}</h2>".Replace("\n", "</br>");

                return Task.FromResult(result);
            } else
            {
                return SerializeDefaultBlockAsync(input, sanity, context);
            }
        }

        /// <summary>
        /// Special check for External Link, or ID attribute for page jumping
        /// </summary>
        /// <returns></returns>
        protected override bool TrySerializeMarkDef(JToken markDef, object context, ref StringBuilder start, ref StringBuilder end)
        {
            var type = markDef["_type"]?.ToString();
            if (type == "externalLink")
            {
                var href = markDef["href"]?.ToString();
                var blank = (bool?)markDef["blank"] ?? false;
                var target = blank ? "target='_blank' rel='noopener'" : "";

                start.Append($"<a {target} href='{href}'>");
                end.Append("</a>");

                return true;
            }

            if (type == "id")
            {
                var ID = markDef["id"]?.ToString();
                if(ID.Length > 0)
                {
                    start.Append($"<span id='{ID}'>");
                    end.Append("</span>");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Create the Image URL for an Image
        /// </summary>
        public Task<string> GetImageSrc(JToken input, SanityOptions options)
        {
            var asset = input["asset"];
            var imageRef = asset?["_ref"]?.ToString();

            if (asset == null || imageRef == null)
            {
                return Task.FromResult("");
            }

            var parameters = new StringBuilder();

            /*
            // Handle Crop and Hotspot, if provided
            var crop = input["crop"];
            var hotspot = input["hotspot"];

            if(crop != null || hotspot != null)
            {
                parameters.Append("?");
                if (crop != null)
                {
                    parameters.Append($"rect={crop["bottom"]},{},{},{}")
                }
            }
            */

            if (input["query"] != null)
            {
                parameters.Append($"?{(string)input["query"]}");
            }

            //build url
            var imageParts = imageRef.Split('-');
            var url = new StringBuilder();
            url.Append("https://cdn.sanity.io/");
            url.Append(imageParts[0] + "s/");            // images/
            url.Append(options.ProjectId + "/");             // projectid/
            url.Append(options.Dataset + "/");             // dataset/
            url.Append(imageParts[1] + "-");             // asset id-
            url.Append(imageParts[2] + ".");             // dimensions.
            url.Append(imageParts[3]);                       // file extension
            url.Append(parameters.ToString());                          // ?crop etc..

            return Task.FromResult(url.ToString());
        }

        /// <summary>
        /// Creates the HTML Scaffold for an image, figure, alt text and caption
        /// </summary>
        public async Task<string> SerializeCustomImageAsync(JToken input, SanityOptions options)
        {
            string getUrl = await GetImageSrc(input, options);
            if (getUrl != null || !getUrl.Equals(""))
            {
                var altText = input["alt"];
                var subtitle = input["subtitle"];
                var html = new StringBuilder();

                html.Append("<figure>");
                html.Append($"<img src=\"{getUrl}\" ");
                if (altText != null)
                {
                    html.Append($"alt=\"{altText}\" ");
                }
                html.Append("/>");

                if(subtitle != null)
                {
                    html.Append($"<caption>{subtitle}</caption>");
                }

                html.Append("</figure>");

                return await Task.FromResult(html.ToString());
            } else
            {
                return await Task.FromResult("");
            }
        }
    }
}
