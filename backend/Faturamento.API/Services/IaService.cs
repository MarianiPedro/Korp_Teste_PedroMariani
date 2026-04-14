using Faturamento.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace Faturamento.API.Services;

public class IaService
{
    private readonly FaturamentoDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IaService> _logger;

    public IaService(
        FaturamentoDbContext context,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<IaService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> SugerirProdutosAsync()
    {
        // LINQ: buscar os produtos mais usados em notas fechadas
        var produtosMaisUsados = await _context.ItensNota
            .Include(i => i.NotaFiscal)
            .Where(i => i.NotaFiscal.Status == Models.StatusNota.Fechada)
            .GroupBy(i => new { i.ProdutoId, i.ProdutoDescricao })
            .Select(g => new
            {
                g.Key.ProdutoDescricao,
                TotalUsado = g.Sum(i => i.Quantidade)
            })
            .OrderByDescending(x => x.TotalUsado)
            .Take(10)
            .ToListAsync();

        if (!produtosMaisUsados.Any())
            return "Ainda não há histórico suficiente para sugestões. Cadastre e imprima algumas notas primeiro!";

        var historico = string.Join("\n", produtosMaisUsados
            .Select(p => $"- {p.ProdutoDescricao}: {p.TotalUsado} unidades usadas"));

        var prompt = $"""
            Você é um assistente de ERP. Com base no histórico de produtos mais utilizados em notas fiscais:

            {historico}

            Sugira de forma curta e objetiva (máximo 3 linhas) quais produtos o usuário provavelmente vai querer incluir na próxima nota fiscal e por quê.
            Responda em português.
            """;

        try
        {
            var apiKey = _configuration["OpenRouter:ApiKey"];
            var model = _configuration["OpenRouter:Model"] ?? "mistralai/mistral-7b-instruct";

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:4200");
            client.DefaultRequestHeaders.Add("X-Title", "Korp ERP");

            var body = new
            {
                model,
                max_tokens = 300,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenRouter retornou erro: {Status}", response.StatusCode);
                return "Não foi possível obter sugestões no momento.";
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            var texto = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return texto ?? "Sem sugestões disponíveis.";
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao chamar OpenRouter: {Msg}", ex.Message);
            return "Serviço de IA temporariamente indisponível.";
        }
    }
}