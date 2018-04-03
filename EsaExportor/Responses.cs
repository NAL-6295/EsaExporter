using System;
using System.Collections.Generic;
using System.Text;

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
}
