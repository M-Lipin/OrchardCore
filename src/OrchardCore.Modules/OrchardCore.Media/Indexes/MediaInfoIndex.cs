using OrchardCore.Media.Models;
using YesSql.Indexes;

namespace OrchardCore.Media.Indexes;

public class MediaInfoIndex : MapIndex
{
    public string UserId { get; set; }

    public string Path { get; set; }
}

public class MediaInfoIndexProvider : IndexProvider<MediaInfo>
{
    public override void Describe(DescribeContext<MediaInfo> context)=>
        context.For<MediaInfoIndex>()
            .Map(mediaInfo =>
            {
                return new MediaInfoIndex
                {
                    UserId = mediaInfo.UserId,
                    Path = mediaInfo.Path
                };
            });
}
