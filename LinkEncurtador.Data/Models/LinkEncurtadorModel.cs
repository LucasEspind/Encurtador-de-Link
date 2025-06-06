using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LinkEncurtador.Data.Models
{
    public class LinkEncurtadorModel
    {
        public string short_url { get; set; }
        public string original_url { get; set; }
        public int clicks { get; set; }
        
        [JsonConstructor]
        public LinkEncurtadorModel() { }

        public LinkEncurtadorModel(string original_url)
        {
            this.original_url = original_url;
            this.short_url = GenerateShortId();
            this.clicks = 0;
        }

        public LinkEncurtadorModel(string original_url, string short_url, int clicks)
        {
            this.original_url = original_url;
            this.short_url = short_url;
            this.clicks = clicks + 1;
        }


        public static string GenerateShortId(int length = 6)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            return new string(Enumerable.Range(0, length).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }
    }
}
