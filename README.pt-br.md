## Selecione o Idioma do README / Select your README Language

[![en](https://img.shields.io/badge/lang-en-red.svg)](https://github.com/matheushoske/CSharp-Linq-ORM-Implementation/blob/master/README.md)
[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg)](https://github.com/matheushoske/CSharp-Linq-ORM-Implementation/blob/master/README.pt-br.md)

# Projeto Simples de ORM para .NET

Bem-vindo ao Projeto Simples de ORM para .NET! Este projeto foi desenvolvido para oferecer aos desenvolvedores um recurso claro e conciso para integrar mapeamento objeto-relacional (ORM) em suas aplicações .NET, utilizando LINQ para gerenciar dados de forma eficiente. Nossa solução ORM possui duas variações:

- `ORMExemploMultiple`, que suporta múltiplos provedores de banco de dados.
- `ORMExemploSingle`, voltado especificamente para MySQL.

Esta aplicação foi projetada em português, mas sinta-se à vontade para adaptá-la para seu próprio idioma. Talvez eu também a traduza para inglês, dependendo da quantidade de solicitações.

Há um artigo em português sobre a implementação:\
[https://medium.com/@matheushoske/27e62a06e030](https://medium.com/@matheushoske/27e62a06e030)

## Descrição

Este projeto apresenta uma implementação básica de um ORM em .NET, demonstrando a construção de um provedor de consultas capaz de traduzir consultas LINQ em comandos SQL e mapear os resultados de volta para objetos .NET. O objetivo principal é fornecer uma compreensão fundamental e uma base funcional para casos de uso mais amplos ou específicos.

## Estrutura do Projeto

- **ORMExemploSingle**

  - Esta versão do ORM é configurada para interagir exclusivamente com bancos de dados MySQL. Inclui todas as configurações necessárias para gerenciar operações de banco de dados usando MySQL.

- **ORMExemploMultiple**

  - Uma versão mais versátil do ORM, projetada para suportar múltiplos provedores de banco de dados. Isso permite que a aplicação interaja com diversos tipos de bancos de dados além do MySQL, como SQL Server, PostgreSQL e outros configuráveis.

## Funcionalidades

- **Compatibilidade com LINQ**: Escreva consultas LINQ que são automaticamente traduzidas para SQL apropriado para os bancos de dados suportados.
- **Abstração de Banco de Dados**: Mínimas alterações necessárias ao alternar entre bancos de dados suportados.
- **Configuração Simples**: Fácil configuração de conexões com bancos de dados e mapeamento de entidades.
- **Consultas Eficientes**: Execução direta de SQL no servidor do banco de dados, reduzindo a sobrecarga de carregamento de grandes conjuntos de dados na memória.

## Primeiros Passos

### Pré-requisitos

1. .NET instalado na sua máquina.
2. Servidor de banco de dados adequado instalado (MySQL, PostgreSQL, etc., dependendo da versão do ORM que você deseja usar).
3. Credenciais de conexão para o banco de dados.

### Instalação

Clone o repositório para sua máquina local:

```bash
git clone https://github.com/matheushoske/CSharp-Linq-ORM-Implementation.git
cd CSharp-Linq-ORM-Implementation
```

### Exemplos

Aqui está um exemplo simples de como utilizar o ORM para buscar dados:
```cs
using (var context = new MyDbContext())
{
    var produtos = context.Products.Where(p => p.Category == "Eletrônicos").ToList();
    foreach(var produto in produtos)
    {
        Console.WriteLine($"Produto: {produto.Name}, Preço: {produto.Price}");
    }
}
```
Você pode encontrar mais exemplos na pasta Exemplos
## Contribuindo

Contribuições para aprimorar a funcionalidade, adicionar suporte a mais bancos de dados ou melhorar o código existente são bem-vindas. Sinta-se à vontade para criar um fork do repositório e enviar pull requests.

