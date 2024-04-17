using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Indexes;
using OrchardCore.Media.Settings;
using YesSql.Sql;

namespace OrchardCore.Media
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ShellDescriptor _shellDescriptor;

        public Migrations(IContentDefinitionManager contentDefinitionManager, ShellDescriptor shellDescriptor)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _shellDescriptor = shellDescriptor;
        }

        // New installations don't need to be upgraded, but because there is no initial migration record,
        // 'UpgradeAsync' is called in a new 'CreateAsync' but only if the feature was already installed.
        public async Task<int> CreateAsync()
        {
            if (_shellDescriptor.WasFeatureAlreadyInstalled("OrchardCore.Media"))
            {
                await UpgradeAsync();
            }

            // Shortcut other migration steps on new content definition schemas.
            return 1;
        }

        // Upgrade an existing installation.
        private async Task UpgradeAsync()
        {
            await _contentDefinitionManager.MigrateFieldSettingsAsync<MediaField, MediaFieldSettings>();
        }

        public async Task<int> UpdateFrom1Async()
        {
            await SchemaBuilder.CreateMapIndexTableAsync<MediaInfoIndex>(table => table
                .Column<string>(nameof(MediaInfoIndex.UserId), column => column.WithLength(26))
                .Column<string>(nameof(MediaInfoIndex.Path))
            );

            await SchemaBuilder.AlterIndexTableAsync<MediaInfoIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(MediaInfoIndex)}_DocumentId",
                    "DocumentId",
                    nameof(MediaInfoIndex.UserId),
                    nameof(MediaInfoIndex.Path)
                )
            );

            return 2;
        }
    }
}
