using Microsoft.AspNetCore.Mvc;
using LinkEncurtador.DAO.FireBase;
using LinkEncurtador.Data.Models;
using System.Net.Http.Headers;
using LinkEncurtador.Data.DTOs;

namespace LinkEncurtador.Controllers
{
    [ApiController]
    [Route("")]
    public class LinkEncurtadorController : Controller
    {
        [HttpGet("/links")]
        public IActionResult GetLinks()
        {
            return Ok(FireBaseConnection.Load());
        }

        [HttpGet("/{short_url}")]
        public IActionResult GetLink([FromRoute] string short_url)
        {
            string url_original = FireBaseConnection.LoadShort(short_url);

            if (!string.IsNullOrEmpty(url_original))
            {
                return Redirect(url_original);
            }
            return BadRequest(new { error = "Não foi possível encontrar a url informada" });
        }

        [HttpPost("/api/shorten")]
        public IActionResult PostLink([FromBody] ShortenDTO urlOriginal)
        {
            if (string.IsNullOrWhiteSpace(urlOriginal.Url_Original))
            {
                return BadRequest(new { error = "Necessário informar a url original" });
            }
            if (!urlOriginal.Url_Original.StartsWith("https://"))
            {
                if (!urlOriginal.Url_Original.StartsWith("http://"))
                {
                    urlOriginal.Url_Original = $"https://{urlOriginal.Url_Original}";
                }
            }

            LinkEncurtadorModel link = new(urlOriginal.Url_Original);

            string verificaShort = FireBaseConnection.LoadShort(link.short_url);


            while (!string.IsNullOrWhiteSpace(verificaShort) || !string.IsNullOrEmpty(verificaShort))
            {
                link.short_url = LinkEncurtadorModel.GenerateShortId();
                verificaShort = FireBaseConnection.LoadShort(link.short_url);
            }

            if (FireBaseConnection.Insert(link))
            {
                return Ok(new { short_url = $"{link.short_url}" });
            }
            return BadRequest(new { error = "Não foi possível encurtar" });
        }

        [HttpDelete("/api/links/{short_code}")]
        public IActionResult DeleteLink([FromRoute] string short_code)
        {
            if (string.IsNullOrWhiteSpace(short_code))
            {
                return BadRequest(new { error = "Necessário informar a url original" });
            }
            if (FireBaseConnection.Delete(short_code))
            {
                return Ok(new { message = "Link deletado com sucesso" });
            }
            return NotFound(new { message = "Link não encontrado" });
        }
    }
}
