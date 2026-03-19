using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConfortAnimal.Models
{
    public class Ambiente
    {
        public int id {  get; set; }

        public double temperatura { get; set; }

        public double umidade { get; set; }

        public string? local { get; set; }

        public DateTime dataRegisto { get; set; }

        public string? ProprietarioId { get; set; }//          → guarda o ID no banco(chave estrangeira)

        [ForeignKey("ProprietarioId")]
        public IdentityUser? Proprietario { get; set; } // → navegação para carregar os dados do utilizador

        //        **Por que precisas dos dois? --> 

        //ProprietarioId  → guarda o ID no banco(chave estrangeira)
        //Proprietario    → navegação para carregar os dados do utilizador
    }
}
