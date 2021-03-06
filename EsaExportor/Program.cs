﻿using HeyRed.MarkdownSharp;
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
        private static string teamPostUrl;

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

            teamPostUrl = $"https://{fromTeam}.esa.io/posts/";

            if (!System.IO.Directory.Exists(filePath))
            {
                System.IO.Directory.CreateDirectory(filePath);
            }


            var indexContent = "## 索引\n";

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
                        indexContent += $"- [{post.full_name}]({post.number}.md)\n";

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

            if (option == ExportOption.Markdown)
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(filePath, "index.md"), indexContent);
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

            var anchors = html.DocumentNode
                .SelectNodes(@"//a");

            var tite = new String(post.full_name.Replace("/", "_").Where(x => !System.IO.Path.GetInvalidFileNameChars().Contains(x)).ToArray());
            var path = $@"{filePath}\{post.number}";


            if (images != null)
            {
                DownloadImages(images, path);

            }

            if (option == ExportOption.Json)
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
                        string url = "";
                        try
                        {
                            url = item.Attributes["src"].Value;
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        var fileName = files.FirstOrDefault(x => item.Attributes["src"].Value.Contains(x));
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            md = md.Replace(url, $@"./{post.number}/{fileName}");
                        }
                    }


                }

                if (anchors != null)
                {
                    foreach (var item in anchors)
                    {
                        string url = "";
                        try
                        {
                            url = item.Attributes["href"].Value;
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        if (url.IndexOf(teamPostUrl) < 0)
                        {
                            continue;
                        }

                        md = md.Replace(url, $"./{post.number}.md");

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
