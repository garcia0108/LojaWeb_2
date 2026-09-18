using LojaWeb_2.Models;

namespace LojaWeb_2.Models
{
    public class PromocaoItemViewModel
    {
        public Produto? Produto { get; set; }

        public Combo? Combo { get; set; }

        public ProdutoCor? Cor { get; set; }

        public bool EhCombo =>
            Combo != null;

        public string Nome =>
            EhCombo
                ? Combo!.Nome
                : Produto!.Nome;

        public decimal PrecoOriginal =>
            EhCombo
                ? Combo!.ValorOriginal
                : Produto!.Preco;

        public decimal PrecoPromocional =>
            EhCombo
                ? Combo!.Preco
                : Produto!.PrecoPromocional ?? Produto!.Preco;

        public decimal PercentualDesconto
        {
            get
            {
                if (PrecoOriginal <= 0 ||
                    PrecoPromocional >= PrecoOriginal)
                {
                    return 0;
                }

                return
                    ((PrecoOriginal - PrecoPromocional)
                    / PrecoOriginal) * 100;
            }
        }

        public string? Imagem =>
             EhCombo
                 ? Combo!.Imagem
                 : Cor?.ImagemPrincipal?.Caminho
                     ?? Cor?.Imagens?
                         .OrderBy(i => i.Id)
                         .Select(i => i.Caminho)
                         .FirstOrDefault()
                     ?? Produto!.Imagem;
    }
}