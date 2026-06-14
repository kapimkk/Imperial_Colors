using Npgsql;
using Xunit;

namespace ImperialColors.Application.Tests;

public class MigrationSchemaIntegrationTests
{
    [Fact]
    public async Task MakeCustoNullableAddUnidadeCusto_DeveEstarAplicadaNoBanco()
    {
        if (!IntegrationTestGuard.TryObterConnectionString(out var connectionString))
        {
            Assert.Fail("Defina RUN_INTEGRATION_TESTS=true e configure o arquivo .env na raiz do projeto.");
            return;
        }

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using (var cmdMigration = new NpgsqlCommand(
                         """
                         SELECT COUNT(*)
                         FROM "__EFMigrationsHistory"
                         WHERE "MigrationId" = '20260614225447_MakeCustoNullableAddUnidadeCusto'
                         """,
                         conn))
        {
            var count = Convert.ToInt32(await cmdMigration.ExecuteScalarAsync());
            Assert.Equal(1, count);
        }

        await using (var cmdCusto = new NpgsqlCommand(
                         """
                         SELECT is_nullable, data_type
                         FROM information_schema.columns
                         WHERE table_name = 'produtos' AND column_name = 'custo'
                         """,
                         conn))
        await using (var readerCusto = await cmdCusto.ExecuteReaderAsync())
        {
            Assert.True(await readerCusto.ReadAsync());
            Assert.Equal("YES", readerCusto.GetString(0));
            Assert.Equal("numeric", readerCusto.GetString(1));
        }

        await using (var cmdUnidadeCusto = new NpgsqlCommand(
                         """
                         SELECT is_nullable, data_type, character_maximum_length
                         FROM information_schema.columns
                         WHERE table_name = 'produtos' AND column_name = 'unidade_custo'
                         """,
                         conn))
        await using (var readerUnidade = await cmdUnidadeCusto.ExecuteReaderAsync())
        {
            Assert.True(await readerUnidade.ReadAsync());
            Assert.Equal("YES", readerUnidade.GetString(0));
            Assert.Equal("character varying", readerUnidade.GetString(1));
            Assert.Equal(10, readerUnidade.GetInt32(2));
        }
    }
}
