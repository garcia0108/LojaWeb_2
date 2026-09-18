using LojaWeb_2.Data;
using LojaWeb_2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LojaWeb_2.Controllers
{
    [Authorize(Roles = "Administrador, Funcionario")]
    public class ClientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Clientes
        public async Task<IActionResult> Index(string? pesquisa)
        {
            var query = _context.Clientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                pesquisa = pesquisa.Trim();

                query = query.Where(c =>
                    c.Nome.Contains(pesquisa) ||
                    (c.CpfCnpj != null &&
                     c.CpfCnpj.Contains(pesquisa)) ||
                    (c.Email != null &&
                     c.Email.Contains(pesquisa)) ||
                    (c.Telefone != null &&
                     c.Telefone.Contains(pesquisa)) ||
                    (c.Cidade != null &&
                     c.Cidade.Contains(pesquisa)));
            }

            var clientes = await query
                .OrderBy(c => c.Nome)
                .ToListAsync();

            // Manter pesquisa na tela
            ViewBag.Pesquisa = pesquisa;

            return View(clientes);
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // GET: Clientes/Create
        public IActionResult Create()
        {
            return View();
        }

        //POST: Ciente/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cliente cliente)
        {
            // Evita espaços desnecessários
            cliente.Nome = cliente.Nome?.Trim() ?? string.Empty;
            cliente.CpfCnpj = cliente.CpfCnpj?.Trim();

            // Validação de CPF/CNPJ
            if (!string.IsNullOrWhiteSpace(cliente.CpfCnpj))
            {
                var documento = new string(
                    cliente.CpfCnpj
                        .Where(char.IsDigit)
                        .ToArray());

                if (documento.Length == 11)
                {
                    if (!ValidarCpf(documento))
                    {
                        ModelState.AddModelError(
                            nameof(cliente.CpfCnpj),
                            "O CPF informado é inválido.");
                    }
                }
                else if (documento.Length == 14)
                {
                    if (!ValidarCnpj(documento))
                    {
                        ModelState.AddModelError(
                            nameof(cliente.CpfCnpj),
                            "O CNPJ informado é inválido.");
                    }
                }
                else
                {
                    ModelState.AddModelError(
                        nameof(cliente.CpfCnpj),
                        "Informe um CPF com 11 dígitos ou um CNPJ com 14 dígitos.");
                }
            }

            cliente.Email = cliente.Email?.Trim();
            cliente.Telefone = cliente.Telefone?.Trim();
            cliente.Endereco = cliente.Endereco?.Trim();
            cliente.Numero = cliente.Numero?.Trim();
            cliente.Bairro = cliente.Bairro?.Trim();
            cliente.Cidade = cliente.Cidade?.Trim();
            cliente.Estado = cliente.Estado?.Trim().ToUpper();
            cliente.Cep = cliente.Cep?.Trim();

            // Verifica CPF/CNPJ duplicado
            if (!string.IsNullOrWhiteSpace(cliente.CpfCnpj))
            {
                var cpfCnpjExiste = await _context.Clientes
                    .AnyAsync(c => c.CpfCnpj == cliente.CpfCnpj);

                if (cpfCnpjExiste)
                {
                    ModelState.AddModelError(
                        nameof(cliente.CpfCnpj),
                        "Já existe um cliente cadastrado com este CPF/CNPJ.");
                }
            }

            // Verifica e-mail duplicado
            if (!string.IsNullOrWhiteSpace(cliente.Email))
            {
                var emailExiste = await _context.Clientes
                    .AnyAsync(c => c.Email != null &&
                                   c.Email.ToLower() == cliente.Email.ToLower());

                if (emailExiste)
                {
                    ModelState.AddModelError(
                        nameof(cliente.Email),
                        "Já existe um cliente cadastrado com este e-mail.");
                }
            }

            if (ModelState.IsValid)
            {
                cliente.DataCadastro = DateTime.Now;
                cliente.Ativo = true;

                _context.Clientes.Add(cliente);

                await _context.SaveChangesAsync();

                TempData["Sucesso"] =
                    "Cliente cadastrado com sucesso!";

                return RedirectToAction(nameof(Index));
            }

            return View(cliente);
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FindAsync(id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Cliente cliente)
        {
            if (id != cliente.Id)
            {
                return NotFound();
            }

            // Limpa os dados
            cliente.Nome = cliente.Nome?.Trim() ?? string.Empty;
            cliente.CpfCnpj = cliente.CpfCnpj?.Trim();

            // Validação de CPF/CNPJ
            if (!string.IsNullOrWhiteSpace(cliente.CpfCnpj))
            {
                var documento = new string(
                    cliente.CpfCnpj
                        .Where(char.IsDigit)
                        .ToArray());

                if (documento.Length == 11)
                {
                    if (!ValidarCpf(documento))
                    {
                        ModelState.AddModelError(
                            nameof(cliente.CpfCnpj),
                            "O CPF informado é inválido.");
                    }
                }
                else if (documento.Length == 14)
                {
                    if (!ValidarCnpj(documento))
                    {
                        ModelState.AddModelError(
                            nameof(cliente.CpfCnpj),
                            "O CNPJ informado é inválido.");
                    }
                }
                else
                {
                    ModelState.AddModelError(
                        nameof(cliente.CpfCnpj),
                        "Informe um CPF com 11 dígitos ou um CNPJ com 14 dígitos.");
                }
            }

            cliente.Email = cliente.Email?.Trim();
            cliente.Telefone = cliente.Telefone?.Trim();
            cliente.Endereco = cliente.Endereco?.Trim();
            cliente.Numero = cliente.Numero?.Trim();
            cliente.Bairro = cliente.Bairro?.Trim();
            cliente.Cidade = cliente.Cidade?.Trim();
            cliente.Estado = cliente.Estado?.Trim().ToUpper();
            cliente.Cep = cliente.Cep?.Trim();

            // Verifica CPF/CNPJ duplicado em OUTRO cliente
            if (!string.IsNullOrWhiteSpace(cliente.CpfCnpj))
            {
                var cpfCnpjExiste = await _context.Clientes
                    .AnyAsync(c =>
                        c.CpfCnpj == cliente.CpfCnpj &&
                        c.Id != cliente.Id);

                if (cpfCnpjExiste)
                {
                    ModelState.AddModelError(
                        nameof(cliente.CpfCnpj),
                        "Já existe outro cliente cadastrado com este CPF/CNPJ.");
                }
            }

            // Verifica e-mail duplicado em OUTRO cliente
            if (!string.IsNullOrWhiteSpace(cliente.Email))
            {
                var emailExiste = await _context.Clientes
                    .AnyAsync(c =>
                        c.Email != null &&
                        c.Email.ToLower() == cliente.Email.ToLower() &&
                        c.Id != cliente.Id);

                if (emailExiste)
                {
                    ModelState.AddModelError(
                        nameof(cliente.Email),
                        "Já existe outro cliente cadastrado com este e-mail.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cliente);

                    await _context.SaveChangesAsync();

                    TempData["Sucesso"] =
                        "Cliente alterado com sucesso!";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }
            }

            return View(cliente);
        }

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes
                .FindAsync(id);

            if (cliente == null)
            {
                return NotFound();
            }

            _context.Clientes.Remove(cliente);

            await _context.SaveChangesAsync();

            TempData["Sucesso"] =
                "Cliente excluído com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        // POST: Clientes/AtivarInativar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtivarInativar(int id)
        {
            var cliente = await _context.Clientes
                .FindAsync(id);

            if (cliente == null)
            {
                return NotFound();
            }

            cliente.Ativo = !cliente.Ativo;

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = cliente.Ativo
                ? "Cliente ativado com sucesso!"
                : "Cliente inativado com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes
                .Any(c => c.Id == id);
        }

        //Validação de CPF
        private bool ValidarCpf(string cpf)
        {
            cpf = new string(cpf
                .Where(char.IsDigit)
                .ToArray());

            if (cpf.Length != 11)
                return false;

            if (cpf.Distinct().Count() == 1)
                return false;

            int[] numeros = cpf
                .Select(c => int.Parse(c.ToString()))
                .ToArray();

            int soma = 0;

            for (int i = 0; i < 9; i++)
            {
                soma += numeros[i] * (10 - i);
            }

            int resto = soma % 11;

            int digito1 = resto < 2 ? 0 : 11 - resto;

            if (numeros[9] != digito1)
                return false;

            soma = 0;

            for (int i = 0; i < 10; i++)
            {
                soma += numeros[i] * (11 - i);
            }

            resto = soma % 11;

            int digito2 = resto < 2 ? 0 : 11 - resto;

            return numeros[10] == digito2;
        }

        //Validação do CNPJ
        private bool ValidarCnpj(string cnpj)
        {
            cnpj = new string(
                cnpj.Where(char.IsDigit).ToArray());

            if (cnpj.Length != 14)
                return false;

            if (cnpj.Distinct().Count() == 1)
                return false;

            int[] numeros = cnpj
                .Select(c => int.Parse(c.ToString()))
                .ToArray();

            int[] pesos1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            int soma = 0;

            for (int i = 0; i < 12; i++)
            {
                soma += numeros[i] * pesos1[i];
            }

            int resto = soma % 11;

            int digito1 = resto < 2 ? 0 : 11 - resto;

            if (numeros[12] != digito1)
                return false;

            int[] pesos2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            soma = 0;

            for (int i = 0; i < 13; i++)
            {
                soma += numeros[i] * pesos2[i];
            }

            resto = soma % 11;

            int digito2 = resto < 2 ? 0 : 11 - resto;

            return numeros[13] == digito2;
        }
    }
}