# Imperial Colors - Sistema de Gestão

Sistema de gestão desktop completo para a empresa **Imperial Colors - Tintas e Revestimentos**, desenvolvido com **.NET 10**, **WPF** e **PostgreSQL**.

---

## Tecnologias

| Componente | Tecnologia |
|---|---|
| Linguagem | C# (.NET 10) |
| Interface | WPF (Windows Presentation Foundation) |
| Banco de Dados | PostgreSQL |
| ORM | Entity Framework Core 10 |
| Arquitetura | Clean Architecture |
| PDF | iText 9 |
| Excel | ClosedXML |
| DI | Microsoft.Extensions.Hosting |

---

## Arquitetura do Projeto

```
ImperialColors/
├── src/
│   ├── ImperialColors.Domain/          # Entidades, Interfaces, Enums, Exceções
│   ├── ImperialColors.Application/     # Services, DTOs, Use Cases
│   ├── ImperialColors.Infrastructure/  # EF Core, Repositórios, Migrations
│   └── ImperialColors.UI/              # WPF, Views, ViewModels
├── .gitignore
├── ImperialColors.slnx
└── README.md
```

---

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- Windows 10/11 (WPF é exclusivo para Windows)
- Visual Studio 2022+ ou Rider (recomendado)

---

## Instalação e Configuração

### 1. Clonar o repositório

```bash
git clone https://github.com/seu-usuario/imperial-colors.git
cd imperial-colors
```

### 2. Configurar o PostgreSQL

Abra o **pgAdmin** ou **psql** e crie o banco de dados:

```sql
CREATE DATABASE imperial_colors;
CREATE USER imperial_user WITH ENCRYPTED PASSWORD 'SuaSenha123';
GRANT ALL PRIVILEGES ON DATABASE imperial_colors TO imperial_user;
```

### 3. Configurar a string de conexão

Edite o arquivo `src/ImperialColors.UI/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=imperial_colors;Username=postgres;Password=SuaSenha;"
  }
}
```

**Parâmetros da string de conexão:**

| Parâmetro | Descrição | Exemplo |
|---|---|---|
| `Host` | Endereço do servidor PostgreSQL | `localhost` ou `192.168.1.100` |
| `Port` | Porta (padrão: 5432) | `5432` |
| `Database` | Nome do banco | `imperial_colors` |
| `Username` | Usuário do PostgreSQL | `postgres` |
| `Password` | Senha do usuário | `SuaSenha123` |

### 4. Executar as Migrations

```bash
cd imperial-colors
dotnet ef migrations add InitialCreate --project src/ImperialColors.Infrastructure --startup-project src/ImperialColors.Infrastructure
dotnet ef database update --project src/ImperialColors.Infrastructure --startup-project src/ImperialColors.Infrastructure
```

> **Nota:** O sistema aplica as migrations automaticamente ao iniciar. Se preferir, execute manualmente com os comandos acima.

### 5. Executar a aplicação

```bash
dotnet run --project src/ImperialColors.UI
```

Ou abra `ImperialColors.sln` no Visual Studio e pressione **F5**.

---

## Configuração para Dois Computadores (Rede Local)

O sistema suporta acesso simultâneo de múltiplos computadores. Siga os passos abaixo:

### No computador SERVIDOR (onde o PostgreSQL está instalado)

#### 1. Configurar o PostgreSQL para aceitar conexões remotas

Edite o arquivo `postgresql.conf` (geralmente em `C:\Program Files\PostgreSQL\15\data\`):

```conf
# Altere ou descomente esta linha:
listen_addresses = '*'
```

Edite o arquivo `pg_hba.conf` (mesmo diretório):

```conf
# Adicione esta linha no final para permitir a rede local:
host    imperial_colors    postgres    192.168.1.0/24    md5
# Ajuste a faixa de IP conforme sua rede (ex: 192.168.0.0/24)
```

#### 2. Reiniciar o serviço PostgreSQL

```powershell
# No PowerShell como Administrador:
Restart-Service postgresql-x64-15
```

#### 3. Configurar o Firewall do Windows

```powershell
# No PowerShell como Administrador:
New-NetFirewallRule -DisplayName "PostgreSQL" -Direction Inbound -Protocol TCP -LocalPort 5432 -Action Allow
```

### Nos computadores CLIENTES

#### Edite o `appsettings.json` de cada computador:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=192.168.1.100;Port=5432;Database=imperial_colors;Username=postgres;Password=SuaSenha;"
  }
}
```

> Substitua `192.168.1.100` pelo IP do computador servidor.

#### Testar conectividade:

```powershell
# Testar se a porta está acessível:
Test-NetConnection -ComputerName 192.168.1.100 -Port 5432
```

Se o resultado mostrar `TcpTestSucceeded: True`, a conexão está funcionando.

---

## Módulos do Sistema

### Dashboard
- Total de vendas do dia e do mês
- Alertas de estoque baixo
- Quantidade de clientes cadastrados
- Resumo financeiro

### Estoque
- Cadastro completo de produtos (código interno, código de barras, categoria, marca, etc.)
- Controle de movimentações (entrada, saída, ajuste)
- Alertas de estoque baixo
- Busca por nome, código interno ou código de barras
- Suporte a leitura de código de barras (conecte o leitor USB e use no campo de busca)

### PDV - Ponto de Venda
- Interface rápida para vendas
- Busca de produtos por nome, código ou código de barras
- Cálculo automático de totais
- Aplicação de descontos
- Vinculação opcional de cliente
- Atualização automática do estoque ao confirmar venda

### Cupom Não Fiscal
- Gerado automaticamente após cada venda
- Opções: imprimir, visualizar, salvar PDF
- Disponível também no histórico de vendas

### Clientes
- Cadastro completo (nome, contatos, endereço)
- Busca rápida
- Vinculação com vendas

### Fornecedores / Mercadorias
- Cadastro de fornecedores
- Lista de compras com controle de itens comprados

### Relatórios
- Vendas por período (PDF e Excel)
- Estoque completo (PDF e Excel)
- Produtos com estoque baixo
- Produtos sem estoque

### Configurações
- Teste de conexão com o banco
- Informações do sistema

---

## Guia de Testes

### Cadastro de produtos
1. Acesse **Estoque** no menu lateral
2. Clique em **+ Novo Produto**
3. O código interno é gerado automaticamente (ou clique em "Gerar")
4. Para usar código de barras: conecte o leitor USB e posicione o cursor no campo "Código de Barras"
5. Preencha os demais campos e clique em **Salvar Produto**

### Realizando uma venda (PDV)
1. Clique em **PDV - Nova Venda** no menu lateral
2. Digite o nome, código ou código de barras do produto no campo de busca
3. Selecione o produto da lista ou pressione Enter
4. Ajuste a quantidade clicando nos botões +/- ou editando diretamente
5. Selecione o cliente (opcional)
6. Aplique desconto se necessário
7. Clique em **✓ FINALIZAR VENDA**
8. O cupom será exibido automaticamente

### Impressão de cupom
- Após a venda: o cupom é exibido automaticamente
- No histórico: acesse **Vendas**, selecione uma venda e clique em **Cupom**

### Cadastro de clientes
1. Acesse **Clientes** no menu
2. Clique em **+ Novo Cliente**
3. Preencha os dados e salve

### Relatórios
1. Acesse **Relatórios** no menu
2. Defina o período de datas
3. Clique no botão do relatório desejado (PDF ou Excel)
4. Escolha onde salvar o arquivo

### Fornecedores
1. Acesse **Mercadorias** no menu
2. Clique em **+ Novo Fornecedor** e preencha os dados

---

## Comandos Úteis

```bash
# Compilar a solução completa
dotnet build

# Executar a aplicação
dotnet run --project src/ImperialColors.UI

# Criar nova migration
dotnet ef migrations add NomeDaMigration --project src/ImperialColors.Infrastructure --startup-project src/ImperialColors.Infrastructure

# Aplicar migrations no banco
dotnet ef database update --project src/ImperialColors.Infrastructure --startup-project src/ImperialColors.Infrastructure

# Reverter migration
dotnet ef migrations remove --project src/ImperialColors.Infrastructure --startup-project src/ImperialColors.Infrastructure
```

---

## Estrutura do Banco de Dados

| Tabela | Descrição |
|---|---|
| `produtos` | Cadastro de produtos |
| `categorias` | Categorias de produtos |
| `marcas` | Marcas de produtos |
| `movimentacoes_estoque` | Histórico de movimentações |
| `clientes` | Cadastro de clientes |
| `vendas` | Registro de vendas |
| `itens_venda` | Itens de cada venda |
| `fornecedores` | Cadastro de fornecedores |
| `listas_compra` | Listas de compras |
| `itens_lista_compra` | Itens de cada lista |

---

## Troubleshooting

### Erro: "Não foi possível conectar ao banco"
1. Verifique se o PostgreSQL está rodando
2. Confirme as credenciais no `appsettings.json`
3. Use a função "Testar Conexão" em **Configurações**

### Erro de migrations ao iniciar
```bash
dotnet ef database update --project src/ImperialColors.Infrastructure --startup-project src/ImperialColors.Infrastructure
```

### Erro em rede local: "Connection refused"
1. Verifique se `listen_addresses = '*'` está no `postgresql.conf`
2. Verifique se a regra de firewall está ativa
3. Confirme que o IP do servidor está correto no `appsettings.json` do cliente

---

## Licença

Desenvolvido para uso exclusivo da **Imperial Colors - Tintas e Revestimentos**.
