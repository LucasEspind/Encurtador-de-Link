using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LinkEncurtador.Data.Models;
using FireSharp.Extensions;
using Newtonsoft.Json;
using System.Web;
using FireSharp;
using System.Collections;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LinkEncurtador.DAO.FireBase
{
    public class FireBaseConnection
    {
        private static IFirebaseConfig config = new FirebaseConfig();

        private static bool configurado = false;

        private static IFirebaseClient? client;

        public static bool Insert(LinkEncurtadorModel data)
        {
            if (!configurado)
            {
                LoadConfig("config.json");
            }


            client = new FireSharp.FirebaseClient(config);

            var response = client.Push("Urls", data);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }

        public static List<LinkEncurtadorModel> Load()
        {
            if (!configurado)
            {
                LoadConfig("config.json");
            }

            client = new FireSharp.FirebaseClient(config);

            var response = client.GetAsync("Urls").Result;

            if (response.StatusCode != HttpStatusCode.OK || string.IsNullOrWhiteSpace(response.Body))
                return new List<LinkEncurtadorModel>();

            try
            {
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, LinkEncurtadorModel>>(response.Body);
                return dictionary?.Values.ToList() ?? new List<LinkEncurtadorModel>();
            }
            catch (Exception ex)
            {
                return new List<LinkEncurtadorModel>();
            }
        }

        public static string LoadShort(string short_url)
        {
            if (!configurado)
            {
                LoadConfig("config.json");
            }


            client = new FireSharp.FirebaseClient(config);

            string retorno = "";

            var response = client.GetAsync("Urls").Result;

            if (response.StatusCode != HttpStatusCode.OK || string.IsNullOrWhiteSpace(response.Body))
                return retorno;

            try
            {
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, LinkEncurtadorModel>>(response.Body);
                foreach (var itens in dictionary)
                {
                    if (itens.Value.short_url == short_url)
                    {
                        LinkEncurtadorModel link = new(itens.Value.original_url, itens.Value.short_url, itens.Value.clicks);
                        
                        var responseUpdate = client.Update($"Urls/{itens.Key}", link);

                        if (responseUpdate.StatusCode == HttpStatusCode.OK)
                        {
                            retorno = itens.Value.original_url;
                        }
                        return retorno;
                    }
                }
            }
            catch (Exception ex)
            {
                return retorno;
            }
            return retorno;
        }

        public static bool Update(string short_code, string origial_url, int clicks)
        {
            if (!configurado)
            {
                LoadConfig("config.json");
            }


            client = new FireSharp.FirebaseClient(config);

            try
            {
                LinkEncurtadorModel link = new(origial_url, short_code, clicks);
                var responseUpdate = client.Update($"Urls/{short_code}", link);
                if(responseUpdate.StatusCode != HttpStatusCode.OK)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Delete(string short_code)
        {
            if (!configurado)
            {
                LoadConfig("config.json");
            }


            client = new FireSharp.FirebaseClient(config);

            var response = client.GetAsync("Urls").Result;

            if (response.StatusCode != HttpStatusCode.OK || string.IsNullOrWhiteSpace(response.Body))
                return false;

            try
            {
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, LinkEncurtadorModel>>(response.Body);
                foreach (var itens in dictionary)
                {
                    if (itens.Value.short_url == short_code)
                    {
                        var responseDelete = client.DeleteAsync($"Urls/{itens.Key}").Result;
                        if (responseDelete.StatusCode != HttpStatusCode.OK)
                        {
                            return false;
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }

        public static void LoadConfig(string path = null)
        {
            if (!string.IsNullOrEmpty(path))
            {
                // Modo local - usa o arquivo config.json
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                if (!File.Exists(path))
                    throw new FileNotFoundException($"Arquivo de configuração não encontrado: {path}");

                string json = File.ReadAllText(path);
                FirebaseConfig configReceiver = JsonConvert.DeserializeObject<FirebaseConfig>(json);

                config.AuthSecret = configReceiver.AuthSecret;
                config.BasePath = configReceiver.BasePath;
            }
            else
            {
                // Modo nuvem - usa variáveis de ambiente
                config.AuthSecret = Environment.GetEnvironmentVariable("FIREBASE_AUTH");
                config.BasePath = Environment.GetEnvironmentVariable("FIREBASE_BASE");
            }

            configurado = true;
        }
    }
}
