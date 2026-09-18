using LojaWeb_2.Data;
using LojaWeb_2.Models;
using Microsoft.EntityFrameworkCore;

namespace LojaWeb_2.Services
{
    public class FreteService
    {
        private readonly ApplicationDbContext _context;

        public FreteService(ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // CALCULAR FRETE DO CARRINHO
        // =========================================

        public async Task<List<OpcaoFrete>> CalcularFrete(
            List<ItemCarrinho> carrinho,
            string cepDestino)
        {
            decimal pesoTotal = 0;


            // =========================================
            // PRODUTOS E COMBOS
            // =========================================

            foreach (var item in carrinho)
            {
                if (item.EhCombo && item.ComboId.HasValue)
                {
                    var combo = await _context.Combos
                        .Include(c => c.Itens)
                            .ThenInclude(i => i.Produto)
                        .FirstOrDefaultAsync(
                            c => c.Id == item.ComboId.Value);

                    if (combo != null)
                    {
                        foreach (var comboItem in combo.Itens)
                        {
                            if (comboItem.Produto != null)
                            {
                                pesoTotal +=
                                    comboItem.Produto.Peso *
                                    comboItem.Quantidade *
                                    item.Quantidade;
                            }
                        }
                    }
                }
                else
                {
                    var produto = await _context.Produtos
                        .FirstOrDefaultAsync(
                            p => p.Id == item.ProdutoId);

                    if (produto != null)
                    {
                        pesoTotal +=
                            produto.Peso *
                            item.Quantidade;
                    }
                }
            }


            // =========================================
            // SIMULAÇÃO DAS OPÇÕES
            // =========================================

            var opcoes = new List<OpcaoFrete>();


            // PAC

            decimal valorPac =
                CalcularValorBase(
                    pesoTotal,
                    15.00m);

            opcoes.Add(new OpcaoFrete
            {
                Nome = "PAC",
                Valor = valorPac,
                PrazoDias = 7
            });


            // SEDEX

            decimal valorSedex =
                CalcularValorBase(
                    pesoTotal,
                    25.00m);

            opcoes.Add(new OpcaoFrete
            {
                Nome = "SEDEX",
                Valor = valorSedex,
                PrazoDias = 3
            });


            return opcoes;
        }


        // =========================================
        // CÁLCULO SIMULADO
        // =========================================

        private decimal CalcularValorBase(
            decimal peso,
            decimal valorBase)
        {
            decimal adicionalPeso =
                peso * 5.00m;

            return Math.Round(
                valorBase + adicionalPeso,
                2);
        }
    }


    // =========================================
    // OPÇÃO DE FRETE
    // =========================================

    public class OpcaoFrete
    {
        public string Nome { get; set; }
            = string.Empty;

        public decimal Valor { get; set; }

        public int PrazoDias { get; set; }
    }
}