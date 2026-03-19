using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConfortAnimal.Models
{
    public class Avaliacao
    {

        public int id {  get; set; }

        public int bovinoId { get; set; }
        public string? ProprietarioId { get; set; } // → guarda o ID no banco(chave estrangeira)
        public int ambienteId { get; set; }


        public double valorITU { get; set; }

        public string? resultado { get; set; }

        public DateTime dataAvaliacao { get; set; }  // → nova propriedade para armazenar a data da avaliação


        [ForeignKey("ProprietarioId")]
        public IdentityUser? Proprietario { get; set; }     // → navegação para carregar os dados do utilizador
        
        
        [ForeignKey("ambienteId")]                      //  → novo atributo para indicar a chave estrangeira
        public Ambiente? Ambiente { get; set; }        //  → nova propriedade de navegação para acessar os dados do ambiente
                                                
        
        [ForeignKey("bovinoId")]                      //  → novo atributo para indicar a chave estrangeira
        public Bovino? Bovino { get; set; }          //  → nova propriedade de navegação para acessar os dados do bovino

    

        //ProprietarioId  → guarda o ID no banco(chave estrangeira)
        //Proprietario    → navegação para carregar os dados do utilizador

    }
}
