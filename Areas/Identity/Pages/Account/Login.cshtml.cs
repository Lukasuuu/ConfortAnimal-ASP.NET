// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;

namespace ConfortAnimal.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;  // SignInManager é um serviço do ASP.NET Core Identity que gerencia o processo de autenticação e login dos usuários.
        private readonly ILogger<LoginModel> _logger;                // ILogger é um serviço de logging do ASP.NET Core que permite registrar informações, avisos e erros durante a execução do aplicativo.

        private readonly UserManager<IdentityUser> _userManager;   // Criei uma variável para o UserManager, que é outro serviço do ASP.NET Core Identity responsável por gerenciar os usuários, incluindo criação, exclusão e recuperação de informações do usuário.

        public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger, UserManager<IdentityUser> userManager) // O construtor da classe LoginModel recebe as dependências necessárias
        {
            _signInManager = signInManager;   // Inicializa a variável _signInManager com a instância fornecida pelo construtor.
            _logger = logger;                // Inicializa a variável _logger com a instância fornecida pelo construtor.

            _userManager = userManager;    // Inicializa a variável _userManager com a instância fornecida pelo construtor.
        }

    
        [BindProperty]                            // O atributo [BindProperty] indica que a propriedade Input deve ser vinculada aos dados do formulário enviado pelo usuário.
        public InputModel Input { get; set; }    // A propriedade Input é do tipo InputModel, que é uma classe interna definida dentro da LoginModel. Ela contém as propriedades necessárias para o processo de login

        [TempData]
        public string ErrorMessage { get; set; } // A propriedade ErrorMessage é marcada com o atributo [TempData], o que significa que ela pode ser usada para armazenar temporariamente uma mensagem de erro

        public string ReturnUrl { get; set; }  // A propriedade ReturnUrl é usada para armazenar a URL para a qual o usuário deve ser redirecionado após um login bem-sucedido.


   
        public class InputModel
        {
            [Required]
            [Display(Name = "Email ou Username")]
            public string Identifier { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {

            // 1. Se veio alguma mensagem de erro, exibe na tela
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            // 2. Se não veio uma URL de retorno, vai para a página inicial
            returnUrl ??= Url.Content("~/");

            // 3. Limpa qualquer cookie de login externo (segurança)
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // 4. Guarda a URL para redirecionar após o login
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                // Tentar encontrar o usuário pelo email primeiro
                var user = await _userManager.FindByEmailAsync(Input.Identifier);

                // Se não encontrar pelo email, tentar pelo username
                if (user == null)
                {
                    user = await _userManager.FindByNameAsync(Input.Identifier);
                }

                // Se não encontrou o usuário, retornar erro
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Tentativa de autenticação inválida. Utilizador não encontrado.");
                    return Page();
                }

                // Verificar a senha com o objeto usuário
                var result = await _signInManager.PasswordSignInAsync(user.UserName, Input.Password, false, lockoutOnFailure: false);

                // Se a autenticação for bem-sucedida, fazer o login do usuário
                if (result.Succeeded)
                {
                    // Verifica se o utilizador é Admin
                    var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                    var isProprietario = await _userManager.IsInRoleAsync(user, "Proprietario");

                    if (!isAdmin && !isProprietario)
                    {
                        // Não é admin, faz logout e bloqueia
                        await _signInManager.SignOutAsync();
                        ModelState.AddModelError(string.Empty, "Acesso negado. Apenas administradores.");
                        return Page();
                    }

                    // Mensagem correta para cada role
                    if (isAdmin)
                    {
                        _logger.LogInformation("Administrador entrou.");
                    }
                    else
                    {
                        _logger.LogInformation("Proprietário entrou.");
                    }
                    return LocalRedirect(returnUrl);
                }

                // Se a autenticação falhou por outros motivos, retornar um erro genérico
                ModelState.AddModelError(string.Empty, "Tentativa de autenticação inválida.");
                return Page();
            }

            // Se chegamos até aqui, algo falhou, reexibir o formulário
            return Page();
        }
    }
}
