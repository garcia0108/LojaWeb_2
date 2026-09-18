# 🛍️ LojaWeb_2

Sistema completo de e-commerce desenvolvido em **ASP.NET Core MVC**, com área administrativa, loja virtual, controle de estoque, vendas, financeiro e gerenciamento de clientes.

O projeto foi desenvolvido com foco em uma aplicação real de comércio eletrônico, integrando a operação administrativa com a experiência de compra do cliente.

---

## 🚀 Sobre o projeto

O **LojaWeb_2** é uma plataforma de e-commerce para gerenciamento de produtos, estoque, vendas e clientes, além de disponibilizar uma loja virtual para realização de compras.

O sistema possui duas áreas principais:

- 🏢 **Área Administrativa**
- 🛒 **Loja Virtual**

---

## 🛠️ Tecnologias utilizadas

- **C#**
- **ASP.NET Core MVC**
- **.NET**
- **Entity Framework Core**
- **SQL Server**
- **ASP.NET Core Identity**
- **Razor Views**
- **Bootstrap**
- **JavaScript**
- **HTML5**
- **CSS3**
- **QuestPDF**
- **Exportação para Excel**

---

## 🏢 Área Administrativa

### 📦 Produtos

- Cadastro de produtos
- Edição e exclusão
- Categorias
- Marcas
- Variações por cor
- Variações por tamanho
- Galeria de imagens
- Imagem principal por cor
- Pesquisa e filtros
- Controle de produtos ativos

### 📊 Estoque

- Entrada de produtos
- Saída de produtos
- Saldo de estoque
- Histórico de movimentações
- Controle por cor e tamanho
- Fornecedores
- Indicadores
- Alertas
- Pesquisa inteligente

### 🛒 Vendas

- Registro de vendas
- Controle de itens vendidos
- Formas de pagamento
- Baixa automática do estoque
- Histórico de vendas
- Cancelamento de vendas
- Controle de custos

### 👥 Clientes

- Cadastro de clientes
- Edição
- Exclusão
- Histórico de compras
- Dados pessoais
- Endereços

### 💰 Financeiro

- Dashboard financeiro
- Contas a receber
- Contas a pagar
- Fluxo de caixa
- Abertura de caixa
- Fechamento de caixa
- Conferência de caixa
- Controle de diferenças
- Filtros por período
- Filtros por tipo
- Filtros por status
- Filtros por forma de pagamento
- Relatórios financeiros
- Exportação para Excel
- Exportação para PDF

### 📈 Relatórios

- Relatório de produtos
- Relatório de estoque
- Relatório de movimentações
- Relatório de vendas
- Relatórios financeiros
- Exportação para PDF
- Exportação para Excel

### 🔐 Segurança

- Login
- Cadastro de usuários
- ASP.NET Core Identity
- Controle de funções e permissões
- Proteção contra acesso não autorizado
- Página personalizada de acesso negado
- Validação de autenticação

---

## 🛍️ Loja Virtual

### 🏠 Home

A página inicial apresenta:

- Banner principal
- Benefícios da loja
- Categorias
- Seleções de produtos
- Mais vendidos
- Seção institucional
- Newsletter
- Rodapé

### 🛒 Catálogo

- Listagem de produtos
- Pesquisa
- Filtros
- Ordenação
- Paginação
- Produtos por categoria
- Produtos masculinos
- Produtos femininos
- Produtos em promoção

### 🎨 Variações de produtos

Os produtos podem possuir diferentes:

- Cores
- Tamanhos
- Imagens por cor
- Imagem principal por cor

A seleção da cor altera a imagem apresentada ao cliente.

### 🛍️ Carrinho

- Adição de produtos
- Seleção de cor
- Seleção de tamanho
- Controle de quantidade
- Carrinho persistente
- Atualização de quantidade
- Produtos com diferentes variações

### 📦 Checkout

Fluxo completo de compra:

```text
Produto
   ↓
Carrinho
   ↓
Checkout
   ↓
Endereço de entrega
   ↓
Forma de pagamento
   ↓
Confirmação
   ↓
Venda
   ↓
Baixa do estoque