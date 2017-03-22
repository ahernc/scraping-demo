using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace scraping_demo
{

    /// <summary>
    /// Short little program to scrape useful things from a website.... tweak yourself as necessary.
    /// </summary>
    class Program
    {


        static string line = "-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+";

        static void Main()
        {

            Console.WriteLine("Hit Enter with no url to quit");

            while (true)
            {

                Console.Write("Enter a URL, e.g. www.irishtimes.com:  ");
                var url = Console.ReadLine();

                if (String.IsNullOrEmpty(url))
                    return;

                var httpRegex = new Regex(@"http://|https://", RegexOptions.Compiled);
                if (!httpRegex.IsMatch(url))
                    url = $"http://{url}";

                try
                {


                    var web = new HtmlWeb();

                    // Optional: This is not absolutely necessary, but it's a good idea. See comments for the OnPreRequest handler
                    web.PreRequest = new HtmlWeb.PreRequestHandler(OnPreRequest);

                    // Optional: monitor some useful data we get back in the Post Repsonse:
                    web.PostResponse = new HtmlWeb.PostResponseHandler(OnPostResponse);


                    // The result of the web.Load will fill the HtmlDocument object
                    var document = web.Load(url);


                    // And now you can parse whatever you want... 
                    // Everything you need will be in the DocumentNode.  See individual methods for usage

                    // Get all of the anchors... 
                    Console.WriteLine("Getting all the anchors...");
                    FindAllTheAnchors(web, document);

                    Console.WriteLine("h1 tags");
                    FindHTags(document, "h1");

                    Console.WriteLine("h2 tags");
                    FindHTags(document, "h2");

                    Console.WriteLine("Get some data from meta tags");
                    GetSomeDataFromMetaTags(document);

                    Console.WriteLine("Get all elements with a particular class name.  Enter a class name that you know exists: ");
                    var classToFind = Console.ReadLine();
                    FindElementsWithClassName(document, classToFind);

                    Console.WriteLine("Print between each script tag");
                    PrintContentOfScriptTags(document);

                    Console.WriteLine("Get the script tags with a particular src attribute");
                    GetScriptTagsWithMatchingSrcAttribute(document);

                    // This one demonstrates how to mix node type selectors and attributes (change this to match for whatever site you are testing)
                    Console.WriteLine("Get every anchor tag with class that contains the letters: a");
                    FindAnchorsWithClassText(document);

                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Something went wrong while scraping {url}! See Debug log...");
                    Debug.WriteLine(ex);

                }

            }

        }


        private static void GetScriptTagsWithMatchingSrcAttribute(HtmlDocument document)
        {
            Debug.WriteLine(line);
            // This should yield some results: referenced scripts within a /js folder.
            // Change for the site you are checking... 
            var nodes = document.DocumentNode.SelectNodes("//script[contains(@src,'/js')]");

            // And just output the full html
            if (nodes != null)
                foreach (var node in nodes)
                {
                    // The full html of the node is in the OuterHtml... 
                    Debug.WriteLine(node.OuterHtml);
                }
        }



        private static void GetSomeDataFromMetaTags(HtmlDocument document)
        {

            Debug.WriteLine(line);

            var metaDescription = document.DocumentNode.SelectSingleNode("//meta[@name='description']");
            if (metaDescription != null)
            {
                if (metaDescription.Attributes["content"] != null)
                    Debug.WriteLine($"meta description: {metaDescription.Attributes["content"].Value}");
            }

            var metaKeywords = document.DocumentNode.SelectSingleNode("//meta[@name='keywords']");
            if (metaKeywords != null)
            {
                if (metaKeywords.Attributes["content"] != null)
                    Debug.WriteLine($"meta keywords: {metaKeywords.Attributes["content"].Value}");
            }

            var metaViewport = document.DocumentNode.SelectSingleNode("//meta[@name='viewport']");
            if (metaViewport != null)
            {
                if (metaViewport.Attributes["content"] != null)
                    Debug.WriteLine($"meta viewport: {metaViewport.Attributes["content"].Value}");
            }

        }

        private static void FindScriptWithTextContent(HtmlDocument document, string textToFind)
        {
            Debug.WriteLine(line);


            // The example here is weak... so change to match what you know is used for anchors....
            var nodes = document.DocumentNode.SelectNodes($"//script[text()='{textToFind}']");
            if (nodes != null)
                foreach (var node in nodes)
                {
                    // The whole content of the script tag....
                    Debug.WriteLine(TidyValue(node.InnerHtml));
                }
        }



        private static void FindAnchorsWithClassText(HtmlDocument document)
        {
            Debug.WriteLine(line);

            // The example here is weak... so change to match what you know is used for anchors....
            var nodes = document.DocumentNode.SelectNodes("//a[contains(@class,'a')]");
            if (nodes != null)
                foreach (var node in nodes)
                {
                    // InnerHtml is the whole html... 
                    Debug.WriteLine(TidyValue(node.InnerHtml));

                    // InnerText is what's between the opening and closing of the h1 tag
                    Debug.WriteLine(TidyValue(node.InnerText));
                }
        }

        private static void PrintContentOfScriptTags(HtmlDocument document)
        {
            Debug.WriteLine(line);

            var nodes = document.DocumentNode.SelectNodes($"//script");
            if (nodes != null)
                foreach (var node in nodes)
                {
                    Debug.WriteLine(node.InnerText);
                }
        }



        private static void FindElementsWithClassName(HtmlDocument document, string classToFind)
        {
            Debug.WriteLine(line);

            var nodes = document.DocumentNode.SelectNodes($"//*[@class=\"{classToFind}\"]");
            if (nodes != null)
                foreach (var node in nodes)
                {
                    // InnerHtml is the whole html... 
                    Debug.WriteLine(TidyValue(node.InnerHtml));

                    // InnerText is what's between the opening and closing of the h1 tag
                    Debug.WriteLine(TidyValue(node.InnerText));
                }
        }




        private static void FindHTags(HtmlDocument document, string nodeType)
        {

            Debug.WriteLine(line);

            // e.g. //h1, //span
            var nodes = document.DocumentNode.SelectNodes($"//{nodeType}");
            if (nodes != null)
                foreach (var node in nodes)
                {
                    // InnerHtml is the whole html... 
                    Debug.WriteLine(TidyValue(node.InnerHtml));

                    // InnerText is what's between the opening and closing of the h1 tag
                    Debug.WriteLine(TidyValue(node.InnerText));
                }
        }



        /// <summary>
        /// Finds all anchors, and outputs them.  There is also some tidying up: tabs, newlines etc are removed to make it easier to see meaningful content
        /// </summary>
        /// <param name="web"></param>
        /// <param name="document"></param>
        private static void FindAllTheAnchors(HtmlWeb web, HtmlDocument document)
        {
            var anchors = document.DocumentNode.SelectNodes("//a");

            // For anchors, the href most likely won't contain the fully qualified URI.
            // Store the current path so we can build the anchors correctly
            var scheme = web.ResponseUri.Scheme;
            var partsToUri = web.ResponseUri.AbsoluteUri.Replace(scheme + "://", "").Split('/');

            // We subtract 1 here because there will always either be a physical page name (e.g. index.html), 
            // or, an empty string if the ResponseUri ended with a forwardslash
            var partsToUriCount = partsToUri.Count() - 1;




            foreach (var a in anchors)
            {

                // Always check if it's to an absolute url. If it isn't, prefix the domain we are currently in:
                if (a.Attributes["href"] != null)
                {

                    var rawAnchor = Regex.Replace(a.Attributes["href"].Value, "[\t\r\n ]", "");

                    var completeUrl = "";
                    if (rawAnchor.StartsWith(scheme + "://"))
                    {
                        // This is the best case scenario: the url will look something like http://www.site.com/folder/page.html
                        completeUrl = a.Attributes["href"].Value;
                    }
                    else
                    {
                        // This is where we need to build a useful url for subsequent queries
                        // Check if we need to navigate up the chain of directories... e.g. if a URL contains "../../somefolder/"
                        var upLevelRegex = new Regex(@"\.\./", RegexOptions.Compiled);
                        if (upLevelRegex.IsMatch(rawAnchor))
                        {

                            // if there is only one part to the responseUri, then we don't want to iterate past that.
                            // e.g. an Anchor has a href of "../folder/product.html", and the ResponseUri was simply "http://www.xyz.com/".
                            // The only thing that should happen is to remove "../"
                            if (partsToUriCount == 1)
                            {
                                completeUrl = scheme + "://" + partsToUri[0] + rawAnchor.Replace("../", "");
                            }
                            else
                            {
                                var levelsUp = upLevelRegex.Matches(rawAnchor).Count;

                                // The number of levels up is the number of directories from the partsToUri that we will ignore
                                // For example, if href = "../../products/index.html", we would drop the last two parts to Uri
                                int stopAt = partsToUriCount - levelsUp;
                                int i = 0;

                                while (i < stopAt)
                                {
                                    completeUrl = $"{completeUrl}/{partsToUri[i]}";
                                    i++;
                                }

                                completeUrl = completeUrl + rawAnchor.Replace("../", "");
                            }

                        }
                        else if (rawAnchor.StartsWith("//"))
                        {
                            // Assume the same scheme can be prefixes
                            completeUrl = scheme + ":" + rawAnchor.Replace("../", "");
                        }
                        else // if (rawAnchor.StartsWith("/"))
                        {
                            // We can just attach the  href onto the partstoUri components from earlier on.
                            completeUrl = scheme + "://" + partsToUri[0] + rawAnchor.Replace("../", "");
                        }
                    }

                    Debug.Write(completeUrl);
                    Debug.WriteLine($" | {TidyValue(a.InnerText)}"); // The text the user sees, if any
                }
                else
                {

                    Debug.Write("No href present!");
                    Debug.WriteLine($" | {TidyValue(a.InnerText)}"); // The text the user sees, if any

                }
            }
        }

        private static object TidyValue(string v)
        {
            if (!String.IsNullOrEmpty(v))
                return Regex.Replace(v.Trim(), "[\t\r\n]", " ");
            else
                return " ";
        }




        /// <summary>
        /// See what comes back in the response... 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private static void OnPostResponse(HttpWebRequest request, HttpWebResponse response)
        {

            // see what cookies came back:
            Debug.WriteLine("Cookies: ");
            foreach (Cookie cookie in response.Cookies)
            {
                Debug.WriteLine($"{cookie.Name}: {cookie.Value}");
            }
            Debug.WriteLine(line);


            // Headers: this is useful if you want to see what kind server the site runs on, among other things...
            Debug.WriteLine("Headers: ");
            foreach (var key in response.Headers.AllKeys)
            {
                Debug.WriteLine($"{key}: {response.Headers[key]}");
            }
            Debug.WriteLine(line);


        }



        /// <summary>
        /// Tweak the request with your own settings... 
        /// For example, tweak with your own timeout, user agent, or even cookies if you saved them from a previous request... 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static bool OnPreRequest(HttpWebRequest request)
        {
            // Let's not wait more than 6 seconds for a request to complete.
            request.Timeout = 6000;

            // Allow the auto redirect... no point in blocking this
            request.AllowAutoRedirect = true;

            // Setting a User Agent can decrease the risk of rejection
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";

            // Set whatever else might help to make you look like a genuine browser. I find the following helps, especially the ContentType             
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ContentType = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.KeepAlive = true;
            request.Method = "GET";
            request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
            request.Headers.Add("Accept-Encoding", "gzip, deflate");


            return true;
        }


    }
}
