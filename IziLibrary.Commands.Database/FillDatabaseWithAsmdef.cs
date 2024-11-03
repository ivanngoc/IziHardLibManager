global using @Transformer = IziHardGames.Contracts.ITransformer<(IziHardGames.IziLibrary.Metas.ForAsmdef.MetaForAsmdef, IziHardGames.IziLibrary.Metas.ForAsmdef.MetaAnalyzForAsmdef), IziHardGames.Libs.IziLibrary.Contracts.ModelAsmdef>;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.IziLibrary.Metas.Factories;
using IziHardGames.IziLibrary.Metas.Factories.Contracts;
using IziHardGames.IziLibrary.Metas.ForAsmdef;
using IziHardGames.Projects.DataBase;

namespace IziHardGames.IziLibrary.Commands.AtDataBase
{
    public class FillDatabaseWithAsmdef
    {
        private IMetaProvider<MetaAbstract> provider;
        private @Transformer transformer;
        private IAnalyzer<MetaForAsmdef, MetaAnalyzForAsmdef> analyzer;

        public FillDatabaseWithAsmdef(IMetaProvider<MetaAbstract> provider,
                                      @Transformer transformer,
                                      IAnalyzer<MetaForAsmdef, MetaAnalyzForAsmdef> analyzer)
        {
            this.provider = provider;
            this.transformer = transformer;
            this.analyzer = analyzer;
        }

        public async Task FillDatabase(CancellationToken ct = default)
        {
            using (ModulesDbContextV2 context = new ModulesDbContextV2())
            {
                await foreach (var item in provider.Provide().WithCancellation(ct).ConfigureAwait(false))
                {
                    if (item is MetaForAsmdef metaForAsmdef)
                    {
                        var analyz = await analyzer.ExecuteAsync(metaForAsmdef).ConfigureAwait(false);
                        var model = transformer.Transform((metaForAsmdef, analyz));
                        await context.AddAsync(model).ConfigureAwait(false);
                    }
                }
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
