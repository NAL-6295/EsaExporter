using HeyRed.MarkdownSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Utf8Json;
namespace EsaExportor
{
    public class Post
    {
        public int number { get; set; }
        public string name { get; set; }
        public string full_name { get; set; }
        public bool wip { get; set; }
        public string body_md { get; set; }
        public string body_html { get; set; }
        public DateTime created_at { get; set; }
        public string message { get; set; }
        public string url { get; set; }
        public DateTime uploaded_at { get; set; }
        public string[] tags { get; set; }
        public string category { get; set; }
        public int revision_number { get; set; }
    }

    public class Posts
    {
        public Post[] posts { get; set; }
        public int total_count { get; set; }
        public int page { get; set; }
        public int per_page { get; set; }
        public int max_per_page { get; set; }
        public int? next_page { get; set; }
    }


    class Program
    {
        static string endpoint = "https://api.esa.io/v1";

        private static string fromTeam;
        private static string filePath;
        static void Main(string[] args)
        {
            if (args.Length != 2) return;

            var token = args[0];
            fromTeam = args[1];
            filePath = $@"d:\{fromTeam}.esa.io";

            if (!System.IO.Directory.Exists(filePath))
            {
                System.IO.Directory.CreateDirectory(filePath);
            }


            using (var request = new System.Net.WebClient())
            {
                request.Headers.Add("Authorization", $"Bearer {token}");

                var page = 1;
                int? nextPage = null;
                do
                {
                    var watch = new Stopwatch();
                    watch.Start();
                    var result = request.DownloadData($"{endpoint}/teams/{fromTeam}/posts?page={page}");


                    var jsons = JsonSerializer.Deserialize<Posts>(result);
                    foreach (var post in jsons.posts)
                    {
                        ToLocal(post);
                    }
                    Console.WriteLine($"page:{page}");
                    page++;
                    var remain = 12000 - watch.ElapsedMilliseconds;
                    if (remain > 0)
                    {
                        System.Threading.Thread.Sleep((int)remain);
                    }
                    nextPage = jsons.next_page;
                } while (nextPage.HasValue);

            }


        }

        private static void ToLocal(Post post)
        {
            using (var imageRequest = new System.Net.WebClient())
            {

                var text = new Markdown().Transform(post.body_md);
                var html = new HtmlAgilityPack.HtmlDocument();
                html.LoadHtml(text);

                var images = html.DocumentNode
                    .SelectNodes(@"//img");
                var path = $@"{filePath}\{post.number}";


                if (images != null)
                {
                    if (!System.IO.Directory.Exists(path))
                    {
                        System.IO.Directory.CreateDirectory(path);
                    }

                    foreach (var address in images
                        .Select(x => x.Attributes["src"]))
                    {
                        try
                        {
                            imageRequest.DownloadFile(address.Value, $@"{path}\{System.IO.Path.GetFileName(address.Value)}");
                        }
                        catch (Exception)
                        {
                            //握りつぶす
                        }
                    }
                }

            }

            var jsonData = JsonSerializer.Serialize(post);

            System.IO.File.WriteAllBytes($@"{filePath}\{post.number}.json", jsonData);

        }
    }
}
