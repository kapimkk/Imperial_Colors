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
├── scripts/                            # Scripts SQL auxiliares
├── docs/                               # Relatórios de testes
├── icons/                              # Logos e ícones
├── .env                                # Credenciais (não commitar)
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

### 3. Configurar credenciais (.env)

Copie o arquivo `.env` na raiz do projeto e ajuste os valores:

```env
DB_HOST=localhost
DB_PORT=5432
DB_NAME=imperial_colors
DB_USER=postgres
DB_PASSWORD=SuaSenha
DB_SSL_MODE=Prefer

ADMIN_USERNAME=admin
ADMIN_PASSWORD=Admin@1234
ADMIN_EMAIL=admin@imperialcolors.local
```

> O `.env` é copiado automaticamente para a pasta de saída no build. **Nunca** commite senhas no repositório.

### 3.1 Dados da empresa (appsettings.json)

Os dados exibidos no cupom e cabeçalhos vêm de `src/ImperialColors.UI/appsettings.json` (copiado para a pasta de saída). Edite **sem recompilar** — reinicie o app após alterar:

```json
"DadosEmpresa": {
  "NomeFantasia": "Imperial Colors",
  "RazaoSocial": "Imperial Colors Tintas e Revestimentos LTDA",
  "Subtitulo": "Tintas e Revestimentos",
  "CNPJ": "00.000.000/0001-00",
  "Endereco": "Rua das Tintas, nº 100 - Bairro Centro, Curitiba - PR",
  "Telefone": "(41) 99999-9999"
}
```

Variáveis de ambiente (`.env`) sobrescrevem o JSON: `EMPRESA_NOME`, `EMPRESA_CNPJ`, `EMPRESA_ENDERECO`, `EMPRESA_TELEFONE`, etc.

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

Ou abra `ImperialColors.slnx` no Visual Studio e pressione **F5**.

---

## Autenticação e Usuários

### Login inicial (administrador)

Na primeira execução, o sistema cria (ou garante) o usuário admin definido no `.env`:

| Campo | Valor padrão |
|---|---|
| Usuário | `admin` |
| Senha | `Admin@1234` |

### Cadastro de novos usuários

1. Na tela de login, aba **Cadastrar**
2. Após o cadastro, a conta fica com status **Aguardando aprovação**
3. Um administrador aprova em **Configurações → Gestão de Usuários**

### Valores de status no banco (`usuarios.status`)

| Valor | Significado |
|---|---|
| `1` | Aguardando aprovação (não pode entrar) |
| `2` | Aprovado (pode entrar) |
| `3` | Cancelado |

Para aprovar manualmente via SQL, use `scripts/aprovar_usuario.sql`:

```sql
UPDATE usuarios SET status = 2 WHERE username = 'seu_usuario';
```

### Recuperar senha do admin

No `.env`, defina temporariamente:

```env
ADMIN_RESET_PASSWORD=true
```

Reinicie o app. A senha do `ADMIN_USERNAME` será redefinida para `ADMIN_PASSWORD`. Remova ou comente a linha depois.

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
- **Modal de fechamento** com formas de pagamento:
  - Dinheiro (valor recebido + troco automático)
  - Cartão Débito / Crédito (1x a 12x) / Pix / Boleto
- Vinculação opcional de cliente
- Atualização automática do estoque ao confirmar venda

### Cupom Não Fiscal
- Gerado automaticamente após cada venda
- Exibe forma de pagamento, parcelas e troco (quando aplicável)
- Impressão direta na impressora configurada em Periféricos
- Opções: imprimir, visualizar, salvar PDF

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
- **Navegação por cards** para submódulos (Geral, Periféricos, Gestão de Usuários)
- **Periféricos:** seleção de impressora para cupom + teste de leitor de código de barras
- Gestão de usuários (Admin)
- Informações do sistema

---

## Interface (WPF)

O sistema utiliza tema centralizado em `Resources/AppTheme.xaml`:

- **Menu lateral:** indicador amarelo (3px) + fundo destacado na aba ativa
- **Inputs:** altura mínima 36px, texto centralizado verticalmente
- **Botões:** hover suave (amarelo escuro / borda amarela nos secundários), cursor `Hand`
- **Scrollbars:** estilo fino minimalista com margem interna em modais
- **Configurações:** cards clicáveis com ícone, título e descrição

### Persistência e performance (EF Core)

- **`IDbContextFactory<AppDbContext>`** — padrão correto para WPF assíncrono: cada operação de repositório cria um contexto curto e isolado, evitando `Cannot access a disposed context instance`
- Repositórios e serviços registrados como **Singleton**; ViewModels permanecem Transient por escopo de página (apenas estado de UI)
- Erros de banco exibem a mensagem detalhada do PostgreSQL (`DbUpdateException` + inner exception)
- Busca de produtos com debounce (300 ms) e cancelamento de buscas anteriores
- Paginação (50 itens/página), `AsNoTracking` e soft delete com Global Query Filter
- Detalhes em `docs/RELATORIO_HOMOLOGACAO_DBCONTEXT.md`, `docs/RELATORIO_ESTOQUE_PERFORMANCE.md` e `docs/RELATORIO_ERRO_SALVAMENTO_PRODUTO.md`

### Formatação visual (pt-BR)

- Cultura `pt-BR` configurada globalmente em `App.xaml.cs`
- `FormattingHelper` + conversores em `App.xaml`: moeda (`R$`), data (`dd/MM/yyyy`), data/hora, quantidade+unidade
- Cadastro de produto: valores monetários exibidos como `R$ 45,50` na edição

### Performance e paginação

- Listagens (Estoque, Clientes, Fornecedores, Vendas): **50 registros/página** com `Skip/Take` no PostgreSQL e `AsNoTracking`
- DataGrids com virtualização de linhas (`VirtualizingStackPanel.Recycling`)
- Logos em cache (`BitmapCacheOption.OnLoad`) — não recarregados a cada navegação
- PDV: desconto em **R$** ou **%** com cálculo automático do total líquido

---

## Guia de Testes

### Cadastro de produtos
1. Acesse **Estoque** no menu lateral
2. Clique em **+ Novo Produto**
3. O código interno é gerado automaticamente (ou clique em "Gerar")
4. **Categoria** e **Marca** são obrigatórias — use os botões **+** ao lado dos ComboBoxes para cadastro rápido
5. Preço de custo e venda devem ser maiores que zero; estoque inicial não pode ser negativo
6. Para usar código de barras: conecte o leitor USB e posicione o cursor no campo "Código de Barras"
7. Preencha os demais campos e clique em **Salvar Produto**
8. A listagem carrega **50 produtos por página** — use **◀ Anterior / Próxima ▶** para navegar

> **Exclusão:** o sistema usa soft delete (`ativo = false` no PostgreSQL). O registro permanece no banco para histórico, mas some de todas as telas.
> Erros de banco são exibidos com a mensagem real do PostgreSQL. Detalhes em `docs/RELATORIO_ERRO_SALVAMENTO_PRODUTO.md` e `docs/RELATORIO_ESTOQUE_PERFORMANCE.md`.

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
| `vendas` | Registro de vendas (inclui forma de pagamento, parcelas, troco) |
| `itens_venda` | Itens de cada venda |
| `fornecedores` | Cadastro de fornecedores |
| `listas_compra` | Listas de compras |
| `itens_lista_compra` | Itens de cada lista |
| `usuarios` | Usuários do sistema (login e permissões) |

---

## Troubleshooting

### Não consigo entrar / "aguardando aprovação"

1. Use o usuário **`admin`** com senha **`Admin@1234`** (conforme `.env`)
2. O campo de login aceita **usuário ou e-mail**
3. Se alterou o banco manualmente, confirme `status = 2` (número, não texto)
4. Alterar só o status **não muda a senha** — use a senha definida no cadastro
5. Recompile e execute: `dotnet run --project src/ImperialColors.UI`

### App fecha sozinho após clicar em Entrar (sem mensagem)

Esse comportamento foi corrigido. A causa era o `ShutdownMode` do WPF encerrando o app quando a tela de login fechava, **antes** de abrir a janela principal. Recompile com a versão mais recente:

```bash
dotnet build
dotnet run --project src/ImperialColors.UI
```

Credenciais padrão: `admin` / `Admin@1234`

### Executar testes de autenticação

```bash
dotnet test tests/ImperialColors.Application.Tests
```

### Erro: "Não foi possível conectar ao banco"
1. Verifique se o PostgreSQL está rodando
2. Confirme as credenciais no `.env`
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
