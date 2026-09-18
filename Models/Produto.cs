using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LojaWeb_2.Models
{
    public class Produto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o nome do produto.")]
        [StringLength(100)]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;


        [StringLength(500)]
        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }


        [Required(ErrorMessage = "Informe o valor.")]
        [Range(0.01, 999999.99, ErrorMessage = "Informe um valor válido.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Preço")]
        public decimal Preco { get; set; }

        // =========================================
        // DIMENSÕES E PESO PARA CÁLCULO DE FRETE
        // =========================================

        [Range(0.001, 999.999, ErrorMessage = "Informe um peso válido.")]
        [Column(TypeName = "decimal(8,3)")]
        [Display(Name = "Peso (kg)")]
        public decimal Peso { get; set; }


        [Range(0.1, 999.9, ErrorMessage = "Informe uma altura válida.")]
        [Column(TypeName = "decimal(8,2)")]
        [Display(Name = "Altura (cm)")]
        public decimal Altura { get; set; }


        [Range(0.1, 999.9, ErrorMessage = "Informe uma largura válida.")]
        [Column(TypeName = "decimal(8,2)")]
        [Display(Name = "Largura (cm)")]
        public decimal Largura { get; set; }


        [Range(0.1, 999.9, ErrorMessage = "Informe um comprimento válido.")]
        [Column(TypeName = "decimal(8,2)")]
        [Display(Name = "Comprimento (cm)")]
        public decimal Comprimento { get; set; }

        [Display(Name = "Produto em promoção")]
        public bool EmPromocao { get; set; } = false;


        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Preço promocional")]
        public decimal? PrecoPromocional { get; set; }

        // =========================================
        // TIPO DE PROMOÇÃO
        // =========================================

        [StringLength(30)]
        [Display(Name = "Tipo da promoção")]
        public string TipoPromocao { get; set; } = "Unitario";


        // =========================================
        // PROMOÇÃO POR QUANTIDADE
        // =========================================

        [Display(Name = "Quantidade mínima")]
        public int? QuantidadeMinimaPromocao { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Preço por unidade na quantidade")]
        public decimal? PrecoQuantidadePromocional { get; set; }

        // =========================================
        // KIT / COMBO
        // =========================================

        [Display(Name = "Quantidade de itens do Kit/Combo")]
        public int? QuantidadeKitCombo { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Preço do Kit/Combo")]
        public decimal? PrecoKitCombo { get; set; }


        [StringLength(10)]
        [Display(Name = "Tamanho")]
        public string? Tamanho { get; set; }


        [StringLength(50)]
        [Display(Name = "Cor")]
        public string? Cor { get; set; }

        [Required(ErrorMessage = "Selecione o público.")]
        [StringLength(20)]
        [Display(Name = "Público")]
        public string Publico { get; set; } = string.Empty;


        [Display(Name = "Imagem")]
        public string? Imagem { get; set; }


        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        [Display(Name = "Produto em destaque")]
        public bool Destaque { get; set; } = false;

        // ============================
        // CATEGORIA
        // ============================

        [Range(1, int.MaxValue, ErrorMessage = "Selecione uma categoria válida.")]
        [Display(Name = "Categoria")]
        public int CategoriaId { get; set; }

        public Categoria? Categoria { get; set; }


        // ============================
        // MARCA
        // ============================

        [Range(1, int.MaxValue, ErrorMessage = "Selecione uma marca válida.")]
        [Display(Name = "Marca")]
        public int MarcaId { get; set; }

        public Marca? Marca { get; set; }

        // ============================
        // FORNECEDOR
        // ============================
        [Display(Name = "Fornecedor")]
        public int? FornecedorId { get; set; }
        public Fornecedor? Fornecedor { get; set; }

        // =========================================
        // IMAGENS ADICIONAIS DO PRODUTO
        // =========================================

        public List<ProdutoImagem> Imagens { get; set; }
            = new List<ProdutoImagem>();

        // =========================================
        // ESTOQUE
        // =========================================

        public List<Estoque> Estoques { get; set; }
            = new List<Estoque>();

        // =========================================
        // COR
        // =========================================
        [BindNever]
        public List<ProdutoCor> Cores { get; set; }
           = new List<ProdutoCor>();
    }
}