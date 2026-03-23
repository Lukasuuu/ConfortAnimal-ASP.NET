# 🐄 ConfortAnimal — Sistema de Bioclimatologia Animal

<div align="center">

![Java](https://img.shields.io/badge/Java-21-orange?style=for-the-badge&logo=java)
![MySQL](https://img.shields.io/badge/MySQL-8.0-blue?style=for-the-badge&logo=mysql)
![NetBeans](https://img.shields.io/badge/NetBeans-IDE-brightgreen?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-Em%20Desenvolvimento-yellow?style=for-the-badge)
![License](https://img.shields.io/badge/Licença-Académica-lightgrey?style=for-the-badge)

<br>

 **Sistema desktop em ASP.NET para avaliação do conforto térmico de bovinos,**  
 **utilizando o Índice de Temperatura e Umidade (ITU) como base científica.**

<br>

</div>



---

## 📋 Índice

- [Sobre o Projeto](#sobre-o-projeto)
- [Funcionalidades](#funcionalidades)
- [Tecnologias](#tecnologias)
- [Arquitetura](#arquitetura)
- [Pré-requisitos](#pré-requisitos)
- [Instalação e Execução](#instalação-e-execução)
- [Roles e Permissões](#roles-e-permissões)
- [Cálculo do ITU](#cálculo-do-itu)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Contribuição](#contribuição)
- [Licença](#licença)

---

## Sobre o Projeto

O **ConfortAnimal** é uma aplicação web desenvolvida em **ASP.NET Core MVC** que permite a proprietários de bovinos registar e monitorizar as condições ambientais das suas instalações, cruzando dados de temperatura e humidade com os seus animais para calcular automaticamente o **Índice de Temperatura e Umidade (ITU)**.

O ITU é um indicador amplamente utilizado na pecuária para avaliar o nível de stress térmico em bovinos, sendo essencial para a tomada de decisões que afetam diretamente a saúde animal e a produtividade leiteira.

---

## Funcionalidades

### 🐂 Gestão de Animais
- Registo de bovinos com dados como nome, peso, idade, raça e produtividade de leite
- Listagem filtrada por proprietário
- Edição e remoção de registos

### 🌡️ Monitorização de Ambientes
- Registo de leituras de temperatura e humidade por local
- Histórico de condições ambientais por data
- Listagem filtrada por proprietário

### 📊 Avaliações de Conforto (ITU)
- Cálculo automático do ITU com base nos dados ambientais registados
- Classificação instantânea do nível de risco em 4 categorias
- Associação de avaliações a animais e ambientes específicos
- Histórico completo de avaliações por proprietário

### 🔐 Autenticação e Autorização
- Sistema de autenticação baseado em **ASP.NET Identity**
- Controlo de acesso por roles (Admin / Proprietário)
- Isolamento de dados por utilizador

## 🧪 Utilizadores de Teste

| Utilizador | Senha | Role |
|---|---|---|
| `Lucas` | `Luc@123` | Proprietário |
| `joao` | `Joao@123` | Proprietário |
| `Admin` | `Admin@123` | Administrador |

## Tecnologias

| Camada | Tecnologia |
|--------|-----------|
| Framework | ASP.NET Core MVC (.NET 8) |
| ORM | Entity Framework Core |
| Autenticação | ASP.NET Core Identity |
| Base de Dados | SQL Server / SQLite |
| Frontend | Razor Views + Bootstrap |
| Linguagem | C# |

## Arquitetura

O projeto segue o padrão **MVC (Model-View-Controller)** com herança de entidades gerida pelo Entity Framework Core através de **TPH (Table-Per-Hierarchy)**.

```
ConfortAnimal/
├── Controllers/
│   ├── AnimaisController.cs       # CRUD de bovinos
│   ├── AmbientesController.cs     # CRUD de ambientes
│   └── AvaliacoesController.cs    # Avaliações ITU
├── Models/
│   ├── Animal.cs                  # Classe base (TPH)
│   ├── Bovino.cs                  # Herda de Animal
│   ├── Ambiente.cs                # Dados ambientais
│   └── Avaliacao.cs               # Resultado ITU
├── Data/
│   └── ApplicationDbContext.cs    # Contexto EF Core
└── Views/
    ├── Animais/
    ├── Ambientes/
    └── Avaliacoes/
```


## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) ou [SQLite](https://www.sqlite.org/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) ou [VS Code](https://code.visualstudio.com/)

## Instalação e Execução

### 1. Clonar o repositório

```bash
git clone https://github.com/seu-utilizador/ConfortAnimal.git
cd ConfortAnimal
```

### 2. Configurar a ligação à base de dados

Editar o ficheiro `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ConfortAnimalDb;Trusted_Connection=True;"
  }
}
```

### 3. Aplicar as migrações

```bash
dotnet ef database update
```

### 4. Executar a aplicação

```bash
dotnet run
```

A aplicação ficará disponível em `https://localhost:5001`.


## Roles e Permissões

| Funcionalidade | Admin | Proprietário |
|----------------|:-----:|:------------:|
| Ver todos os registos | ✅ | ❌ |
| Ver os próprios registos | ✅ | ✅ |
| Criar registos | ✅ | ✅ |
| Editar registos | ✅ | ✅ (só os seus) |
| Eliminar registos | ✅ | ✅ (só os seus) |

> Os proprietários apenas têm acesso aos seus próprios animais, ambientes e avaliações. O campo `ProprietarioId` é preservado em todas as operações de edição para garantir a integridade dos dados.

## Cálculo do ITU

O cálculo do ITU utiliza a **fórmula de Thom (1959)**, padrão para bovinos:

```
ITU = (1.8 × T + 32) − (0.55 − 0.0055 × UR) × (1.8 × T − 26.8)
```

Onde:
- **T** = temperatura ambiente (°C)
- **UR** = humidade relativa (%)

### Classificação — Baêta & Souza (2010)

| ITU | Classificação | Descrição |
|-----|--------------|-----------|
| ≤ 74 | 🟢 **Conforto** | Condições ideais para o animal |
| 75 – 78 | 🟡 **Alerta** | Início de stress térmico leve |
| 79 – 88 | 🟠 **Perigo** | Stress térmico significativo |
| > 88 | 🔴 **Emergência** | Risco elevado para a saúde animal |

## Estrutura do Projeto

```
ConfortAnimal/
├── ConfortAnimal.sln
├── ConfortAnimal/
│   ├── Controllers/
│   ├── Data/
│   ├── Migrations/
│   ├── Models/
│   ├── Views/
│   ├── wwwroot/
│   ├── appsettings.json
│   └── Program.cs
└── README.md
```

## Contribuição

Contribuições são bem-vindas! Para contribuir:

1. Faça um **fork** do projeto
2. Crie uma branch para a sua feature (`git checkout -b feature/nova-funcionalidade`)
3. Faça **commit** das suas alterações (`git commit -m 'Adiciona nova funcionalidade'`)
4. Faça **push** para a branch (`git push origin feature/nova-funcionalidade`)
5. Abra um **Pull Request**

---

## Licença

Este projeto está licenciado sob a licença MIT. Consulte o ficheiro [LICENSE](LICENSE) para mais detalhes.

---
<p align="center">
  Desenvolvido com ❤️ para a monitorização do bem-estar animal
</p>
