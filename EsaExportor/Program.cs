using HeyRed.MarkdownSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Utf8Json;
namespace EsaExportor
{



    class Program
    {
        static string endpoint = "https://api.esa.io/v1";

        private static string fromTeam;
        private static string filePath;

        public enum ExportOption
        {
            Json = 0,
            Markdown
        }


        static void Main(string[] args)
        {
            if (args.Length != 3) return;

            var token = args[0];
            fromTeam = args[1];
            var option = Enum.Parse<ExportOption>(args[2]);
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
                        ToLocal(post, option);
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

        private static void ToLocal(Post post, ExportOption option)
        {
            string text = "";
            try
            {
                text = new Markdown().Transform(post.body_md);
            }
            catch (Exception)
            {

                return;
            }

            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml(text);

            var images = html.DocumentNode
                .SelectNodes(@"//img");

            var tite = new String(post.full_name.Replace("/","_").Where(x => !System.IO.Path.GetInvalidFileNameChars().Contains(x)).ToArray());
            var path = option == ExportOption.Json ? $@"{filePath}\{post.number}" :
                $@"{filePath}\{tite}";


            if (images != null)
            {
                DownloadImages(images, path);

            }

            if(option == ExportOption.Json)
            {
                var jsonData = JsonSerializer.Serialize(post);

                System.IO.File.WriteAllBytes($@"{path}.json", jsonData);
            }
            else
            {

                var md = post.body_md;
                if (images != null)
                {
                    var files = System.IO.Directory.GetFiles(path)
                        .Select(x => System.IO.Path.GetFileName(x))
                        .ToArray();

                    foreach (var item in images)
                    {
                        var url = item.Attributes["src"].Value;
                        var fileName = files.FirstOrDefault(x => item.Attributes["src"].Value.Contains(x));
                        if(!string.IsNullOrEmpty(fileName))
                        {
                            md = md.Replace(url, $@"./{tite}/{fileName}");
                        }
                    }

                }

                System.IO.File.WriteAllText($@"{path}.md", md);

            }


        }

        private static void DownloadImages(HtmlAgilityPack.HtmlNodeCollection images, string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            using (var imageRequest = new System.Net.WebClient())
            {
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
    }
}
