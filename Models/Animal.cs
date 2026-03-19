using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConfortAnimal.Models
{
    public class Animal
    {
        public int Id { get; set; }

        public string? Nome { get; set; }

        public double Peso { get; set; }

        public int Idade { get; set; }

        public string? ProprietarioId { get; set; }//          → guarda o ID no banco(chave estrangeira)

        [ForeignKey("ProprietarioId")]
        public IdentityUser? Proprietario { get; set; } // → navegação para carregar os dados do utilizador

        //        **Por que precisas dos dois? --> 

        //ProprietarioId  → guarda o ID no banco(chave estrangeira)
        //Proprietario    → navegação para carregar os dados do utilizador
    }
}
